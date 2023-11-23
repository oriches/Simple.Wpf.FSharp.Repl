﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using Simple.Wpf.FSharp.Repl.Common.Services;

namespace Simple.Wpf.FSharp.Repl.Common.Core
{
    /// <summary>
    ///     Wrapper around the F# Interactive process.
    /// </summary>
    public abstract class BaseReplEngine : IReplEngine
    {
        /// <summary>
        ///     REPL engine quit line for the F# Interactive process.
        /// </summary>
        public const string QuitLine = "#quit;;";

        /// <summary>
        ///     REPL engine line termination characters.
        /// </summary>
        public const string LineTermination = ";;";

        private const string AwaitingInput = "> ";
        
        protected const string Executable = @"fsi.exe";
        protected const string ExecutableAnyCpu = @"fsiAnyCpu.exe";

        private const string BaseDirectory = @".simple.wpf.fsharp.repl";
        private const string FSharpDirectory = @"fsharp";
        private const string ZipFilename = @"fsharp.zip";

        private const string WorkingDirectoryOutput = "Working folder = \"{0}\"";
        private readonly CompositeDisposable _disposable;
        private readonly Subject<ReplProcessOutput> _outputStream;

        private readonly IProcessService _processService;
        private readonly IScheduler _scheduler;
        private readonly BehaviorSubject<State> _stateStream;

        private ReplProcess _replProcess;

        private string _startupScript;

        /// <summary>
        ///     Creates an instance of the REPL engine with the specified parameters.
        /// </summary>
        /// <param name="workingDirectory">The working directory for the F# Interactive process.</param>
        /// <param name="processService">Handles creating windows processes.</param>
        /// <param name="scheduler">The Reactive scheduler for the REPL engine, defaults to the task pool scheduler.</param>
        protected BaseReplEngine(string workingDirectory = null, IProcessService processService = null,
            IScheduler scheduler = null)
        {
            _scheduler = scheduler;
            _processService = processService ?? new ProcessService();
            _scheduler = scheduler ?? TaskPoolScheduler.Default;

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                WorkingDirectory = workingDirectory.Trim();
                var directoryInfo = new DirectoryInfo(WorkingDirectory);

                if (!directoryInfo.Exists) directoryInfo.Create();
            }
            else
            {
                WorkingDirectory = Path.GetTempPath();
            }

            _stateStream = new BehaviorSubject<State>(Core.State.Unknown);
            _outputStream = new Subject<ReplProcessOutput>();

            _disposable = new CompositeDisposable
            {
                _stateStream,
                _outputStream
            };
        }

        /// <summary>
        ///     Disposes the REPL engine, if it's been started then it will be stopped.
        /// </summary>
        public virtual void Dispose()
        {
            Stop();

            _disposable.Dispose();
        }

        /// <summary>
        ///     REPL engine output as a Reactive extensions stream.
        /// </summary>
        public IObservable<string> Output
        {
            get { return _outputStream.Where(x => !x.IsError).Select(x => x.Output); }
        }

        /// <summary>
        ///     REPL engine errors as a Reactive extensions stream.
        /// </summary>
        public IObservable<string> Error
        {
            get { return _outputStream.Where(x => x.IsError).Select(x => x.Output); }
        }

        /// <summary>
        ///     REPL engine state changes as a Reactive extensions stream.
        /// </summary>
        public IObservable<State> State => _stateStream.DistinctUntilChanged();

        /// <summary>
        ///     REPL engine working directory as a Reactive extensions stream.
        /// </summary>
        public string WorkingDirectory { get; }

        /// <summary>
        ///     Starts the REPL engine.
        /// </summary>
        /// <param name="script">The script to run at startup.</param>
        /// <returns>Returns the REPL engine.</returns>
        public IReplEngine Start(string script = null)
        {
            var state = _stateStream.Value;
            if (state != Core.State.Stopped && state != Core.State.Unknown && state != Core.State.Faulted) return this;

            _stateStream.OnNext(Core.State.Starting);

            _startupScript = script;
            _replProcess = StartProcess();

            return this;
        }

        /// <summary>
        ///     Stops the REPL engine.
        /// </summary>
        /// <returns>Returns the REPL engine.</returns>
        public IReplEngine Stop()
        {
            var state = _stateStream.Value;
            if (state == Core.State.Stopping || state == Core.State.Stopped) return this;

            _stateStream.OnNext(Core.State.Stopping);

            _replProcess.Dispose();

            _replProcess = null;
            _startupScript = null;

            _stateStream.OnNext(Core.State.Stopped);

            return this;
        }

        /// <summary>
        ///     Reset the REPL engine, if it has already been started.
        /// </summary>
        /// <returns>Returns the REPL engine.</returns>
        public IReplEngine Reset()
        {
            var state = _stateStream.Value;
            if (state == Core.State.Stopping || state == Core.State.Stopped) return this;

            _stateStream.OnNext(Core.State.Stopping);

            _replProcess.Dispose();

            _stateStream.OnNext(Core.State.Stopped);
            _stateStream.OnNext(Core.State.Starting);

            _replProcess = StartProcess();

            return this;
        }

        /// <summary>
        ///     Executes a scripts, if the REPL engine has been started.
        /// </summary>
        /// <param name="script">The script to be executed.</param>
        /// <returns>Returns the REPL engine.</returns>
        public IReplEngine Execute(string script)
        {
            var state = _stateStream.Value;
            if (state != Core.State.Running && state != Core.State.Executing) return this;

            if (script.EndsWith(LineTermination)) _stateStream.OnNext(Core.State.Executing);

            _replProcess.WriteLine(script);

            return this;
        }

        private ReplProcess StartProcess()
        {
            var process = CreateProcess();
            var tokenSource = new CancellationTokenSource();

            var disposable = Observable.Create<Unit>(o =>
                {
                    process.Start();

                    o.OnNext(Unit.Default);
                    return Disposable.Empty;
                })
                .Select(_ => ObserveStandardErrors(process, tokenSource.Token))
                .Select(_ => ObserveStandardOutput(process, tokenSource.Token))
                .Subscribe(_ => { }, e => _stateStream.OnNext(Core.State.Faulted));

            return new ReplProcess(process, Disposable.Create(() =>
            {
                tokenSource.Cancel();
                tokenSource.Dispose();

                disposable.Dispose();
            }));
        }

        private IObservable<Unit> ObserveStandardOutput(IProcess process, CancellationToken cancellationToken)
        {
            return Observable.Start(() =>
            {
                _outputStream.OnNext(new ReplProcessOutput(string.Format(WorkingDirectoryOutput, WorkingDirectory)));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var output = string.Empty;

                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var readTask = process.StandardOutputReadAsync(cancellationToken);
                            readTask.Wait(cancellationToken);

                            output += (char) readTask.Result;

                            if (output == AwaitingInput)
                            {
                                if (_stateStream.Value == Core.State.Starting &&
                                    !string.IsNullOrEmpty(_startupScript))
                                {
                                    _outputStream.OnNext(new ReplProcessOutput(output + _startupScript));

                                    _stateStream.OnNext(Core.State.Executing);
                                    _replProcess.WriteLine(_startupScript);
                                }
                                else
                                {
                                    _stateStream.OnNext(Core.State.Running);
                                }

                                break;
                            }

                            if (output.EndsWith(Environment.NewLine))
                            {
                                _outputStream.OnNext(new ReplProcessOutput(output));
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }, _scheduler);
        }

        private IObservable<Unit> ObserveStandardErrors(IProcess process, CancellationToken cancellationToken)
        {
            return Observable.Start(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var error = string.Empty;

                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var readTask = process.StandardErrorReadAsync(cancellationToken);
                            readTask.Wait(cancellationToken);

                            error += (char) readTask.Result;

                            if (error.EndsWith(Environment.NewLine))
                            {
                                _outputStream.OnNext(new ReplProcessOutput(error, true));
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }, _scheduler);
        }

        protected void ExtractFSharpBinaries(out string binaryPath)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var basePath = Path.Combine(tempPath, BaseDirectory);

                var runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.ToLower();
                var baseAndRuntimePath = Path.Combine(basePath, runtime);

                binaryPath = Path.Combine(baseAndRuntimePath, FSharpDirectory);

                if (Directory.Exists(binaryPath))
                    if (DoesVersionFileExist(binaryPath))
                        return;

                var di = new DirectoryInfo(basePath);
                if (!di.Exists) di.Create();

                di = new DirectoryInfo(binaryPath);
                if (!di.Exists)
                    di.Create();
                else
                    foreach (var file in di.EnumerateFiles())
                        file.Delete();

                var zipFilePath = Path.Combine(binaryPath, ZipFilename);
                using (var stream = File.Create(zipFilePath))
                {
                    var array = GetFSharpResource();
                    stream.Write(array, 0, array.Length);
                }

                ZipFile.ExtractToDirectory(zipFilePath, binaryPath);

                File.Delete(zipFilePath);

                CreateVersionFile(binaryPath);
            }
            catch (Exception)
            {
                // ignored
                binaryPath = null;
            }
        }

        protected abstract byte[] GetFSharpResource();

        private static void CreateVersionFile(string binaryDirectory)
        {
            var versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var versionFile = $"{versionNumber}.txt";

            var versionFilePath = Path.Combine(binaryDirectory, versionFile);

            File.CreateText(versionFilePath).Dispose();
        }

        private static bool DoesVersionFileExist(string binaryDirectory)
        {
            var versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var versionFile = $"{versionNumber}.txt";

            var versionFilePath = Path.Combine(binaryDirectory, versionFile);

            return File.Exists(versionFilePath);
        }

        protected abstract string GetExecutablePath();
        
        private IProcess CreateProcess()
        {
            var executablePath = GetExecutablePath();
            var process = _processService.StartReplExecutable(WorkingDirectory, executablePath);

            Debug.WriteLine("Working folder    = " + WorkingDirectory);
            Debug.WriteLine("Executable folder = " + executablePath);

            return process;
        }

        internal sealed class ReplProcess : IDisposable
        {
            private readonly IDisposable _disposable;
            private readonly IProcess _process;
            private bool _disposed;

            public ReplProcess(IProcess process, IDisposable disposable)
            {
                _process = process;
                _disposable = disposable;
            }

            public void Dispose()
            {
                DisposeImpl(true);
                GC.SuppressFinalize(this);
            }

            // Use C# destructor syntax for finalization code.
            ~ReplProcess()
            {
                DisposeImpl(false);
            }

            private void DisposeImpl(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _process.WriteStandardInput(QuitLine);
                        _process.WaitForExit();
                        _process.Dispose();
                    }

                    _disposable.Dispose();
                    _disposed = true;
                }
            }

            public void WriteLine(string script)
            {
                _process.WriteStandardInput(script);
            }
        }

        internal sealed class ReplProcessOutput
        {
            public ReplProcessOutput(string output, bool isError = false)
            {
                Output = output;
                IsError = isError;
            }

            public string Output { get; }

            public bool IsError { get; }
        }
    }
}