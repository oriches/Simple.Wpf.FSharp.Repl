namespace Simple.Wpf.FSharp.Repl
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading;
    using Extensions;

    public sealed class ReplEngine : IReplEngine, IDisposable
    {
        public const string QuitLine = "#quit;;";
        public const string LineTermination = ";;";
        private const string BinaryDirectory = @"FSharp";
        private const string Executable = @"fsi.exe";
        private const string AwaitingInput = "> ";

        private readonly string _baseWorkingDirectory;

        private readonly IScheduler _scheduler;
        private readonly Subject<ReplProcessOutput> _outputStream;
        private readonly BehaviorSubject<State> _stateStream;
        private readonly CompositeDisposable _disposable;

        private string _startupScript;
        private ReplProcess _replProcess;

        internal sealed class ReplProcess : IDisposable
        {
            private readonly Process _process;
            private readonly IDisposable _disposable;

            public ReplProcess(Process process, IDisposable disposable)
            {
                _process = process;
                _disposable = disposable;
            }

            public void Dispose()
            {
                var workingDirectory = _process.StartInfo.WorkingDirectory;

                _disposable.Dispose();

                _process.StandardInput.WriteLine(QuitLine);
                _process.WaitForExit();
                _process.Dispose();

                SafeDirectoryDelete(workingDirectory);
            }

            public void WriteLine(string script)
            {
                _process.StandardInput.WriteLine(script);
            }

            private static void SafeDirectoryDelete(string workingDirectory)
            {
                try
                {
                    Directory.Delete(workingDirectory, true);
                }
                catch (Exception exn)
                {
                    Debug.WriteLine("Failed to delete directory - " + workingDirectory);
                    Debug.WriteLine("Exception message - " + exn.Message);
                }
            }
        }

        internal sealed class ReplProcessOutput
        {
            public string Output { get; private set; }

            public bool IsError { get; private set; }

            public ReplProcessOutput(string output, bool isError = false)
            {
                Output = output;
                IsError = isError;
            }
        }

        public ReplEngine(string workingDirectory = null, IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? TaskPoolScheduler.Default;

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                _baseWorkingDirectory = workingDirectory.Trim();
            }

            _stateStream = new BehaviorSubject<State>(Repl.State.Unknown);
            _outputStream = new Subject<ReplProcessOutput>();

            _disposable = new CompositeDisposable
            {
                _stateStream,
                _outputStream,
            };
        }

        public IObservable<string> Output { get { return _outputStream.Where(x => !x.IsError).Select(x => x.Output); } }

        public IObservable<string> Error { get { return _outputStream.Where(x => x.IsError).Select(x => x.Output); } }

        public IObservable<State> State { get { return _stateStream.DistinctUntilChanged(); } }

        public IReplEngine Start(string script = null)
        {
            var state = _stateStream.First();
            if (state != Repl.State.Stopped && state != Repl.State.Unknown && state != Repl.State.Faulted)
            {
                return this;
            }

            _stateStream.OnNext(Repl.State.Starting);

            _startupScript = script;
            _replProcess = StartProcess();

            return this;
        }

        public IReplEngine Stop()
        {
            var state = _stateStream.First();
            if (state == Repl.State.Stopping || state == Repl.State.Stopped)
            {
                return this;
            }

            _stateStream.OnNext(Repl.State.Stopping);

            _replProcess.Dispose();

            _replProcess = null;
            _startupScript = null;

            _stateStream.OnNext(Repl.State.Stopped);
            
            return this;
        }

        public IReplEngine Reset()
        {
            var state = _stateStream.First();
            if (state == Repl.State.Stopping || state == Repl.State.Stopped)
            {
                return this;
            }

            _stateStream.OnNext(Repl.State.Stopping);

            _replProcess.Dispose();

            _stateStream.OnNext(Repl.State.Stopped);
            _stateStream.OnNext(Repl.State.Starting);

            _replProcess = StartProcess();

            return this;
        }

        public IReplEngine Execute(string script)
        {
            var state = _stateStream.First();
            if (state != Repl.State.Running && state != Repl.State.Executing)
            {
                return this;
            }

            if (script.EndsWith(LineTermination))
            {
                _stateStream.OnNext(Repl.State.Executing);
            }

            _replProcess.WriteLine(script);

            return this;
        }

        public void Dispose()
        {
            Stop();

            _disposable.Dispose();
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
            .Subscribe(_ => { }, e => _stateStream.OnNext(Repl.State.Faulted));

            return new ReplProcess(process, Disposable.Create(() =>
            {
                tokenSource.Cancel();
                tokenSource.Dispose();

                disposable.Dispose();
            }));
        }

        private IObservable<Unit> ObserveStandardOutput(Process process, CancellationToken cancellationToken)
        {
            return Observable.Start(() =>
            {
                while (true)
                {
                    try
                    {
                        var output = string.Empty;
                        while (true)
                        {
                            var readTask = process.StandardOutput.ReadAsync(cancellationToken);
                            readTask.Wait(cancellationToken);

                            output += (char)readTask.Result;

                            if (output == AwaitingInput)
                            {
                                _outputStream.OnNext(new ReplProcessOutput(output));

                                if (_stateStream.First() == Repl.State.Starting && !string.IsNullOrEmpty(_startupScript))
                                {
                                    _outputStream.OnNext(new ReplProcessOutput(_startupScript));
                                    _outputStream.OnNext(new ReplProcessOutput(Environment.NewLine));

                                    _stateStream.OnNext(Repl.State.Running);

                                    Execute(_startupScript);
                                }
                                else
                                {
                                    _stateStream.OnNext(Repl.State.Running);
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
                        return;
                    }
                }
            }, _scheduler);
        }

        private IObservable<Unit> ObserveStandardErrors(Process process, CancellationToken cancellationToken)
        {
            return Observable.Start(() =>
            {
                while (true)
                {
                    try
                    {
                        var error = string.Empty;
                        while (true)
                        {
                            var readTask = process.StandardError.ReadAsync(cancellationToken);
                            readTask.Wait(cancellationToken);

                            error += (char)readTask.Result;

                            if (error.EndsWith(Environment.NewLine))
                            {
                                _outputStream.OnNext(new ReplProcessOutput(error, true));
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }, _scheduler);
        }

        private static string BuildExecutablePath()
        {
            var currrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currrentDirectory == null)
            {
                throw new Exception("Failed to get currrent executing directory.");
            }

            return Path.Combine(Path.Combine(currrentDirectory, BinaryDirectory), Executable);
        }

        private string BuildWorkingDirectory()
        {
            string workingDirectory;
            if (string.IsNullOrEmpty(_baseWorkingDirectory))
            {
                var currrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (currrentDirectory == null)
                {
                    throw new Exception("Failed to get currrent executing directory.");
                }

                workingDirectory = Path.Combine(currrentDirectory, Guid.NewGuid().ToString());
            }
            else
            {
                workingDirectory = Path.Combine(_baseWorkingDirectory, Guid.NewGuid().ToString());
            }

            Directory.CreateDirectory(workingDirectory);

            return workingDirectory;
        }

        private Process CreateProcess()
        {
            var process = new Process
            {
                StartInfo =
                {
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = BuildWorkingDirectory(),
                    FileName = BuildExecutablePath()
                }
            };

            Debug.WriteLine("Working directory - " + process.StartInfo.WorkingDirectory);
            Debug.WriteLine("File name - " + process.StartInfo.FileName);

            return process;
        }
    }
}