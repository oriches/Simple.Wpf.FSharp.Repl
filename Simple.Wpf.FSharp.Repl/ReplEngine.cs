namespace Simple.Wpf.FSharp.Repl
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading;
    using Extensions;

    public sealed class ReplEngine : IReplEngine
    {
        private readonly IScheduler _scheduler;
        private const string FSharpDirectory = @"FSharp";
        private const string FSharpExecutable = @"fsi.exe";
        private const string FSharpQuitLine = "#quit;;";
        private const string FSharpScriptTermination = ";;";
        private const string FSharpAwaitingInput = "> ";

        private readonly string _baseWorkingDirectory;
        private readonly Subject<string> _outputStream;
        private readonly BehaviorSubject<State> _stateStream;

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

                _process.StandardInput.WriteLine(FSharpQuitLine);
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

        public ReplEngine(string workingDirectory = null, IScheduler scheduler = null)
        {
            _stateStream = new BehaviorSubject<State>(Repl.State.Stopped);
            _scheduler = scheduler ?? TaskPoolScheduler.Default;

            _outputStream = new Subject<string>();

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                _baseWorkingDirectory = workingDirectory.Trim();
            }
        }

        public IObservable<string> Output { get { return _outputStream; } }

        public IObservable<State> State { get { return _stateStream.DistinctUntilChanged(); } }

        public IReplEngine Start(string script = null)
        {
            if (_stateStream.First() != Repl.State.Stopped)
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

            if (script.EndsWith(FSharpScriptTermination))
            {
                _stateStream.OnNext(Repl.State.Executing);
            }

            _replProcess.WriteLine(script);

            return this;
        }

        public void Dispose()
        {
            Stop();
        }

        private ReplProcess StartProcess()
        {
            var process = CreateProcess();
            process.Start();

            var tokenSource = new CancellationTokenSource();

            var disposable = Observable.Start(() =>
            {
                while (true)
                {
                    try
                    {
                        var input = string.Empty;
                        while (true)
                        {
                            var readTask = process.StandardOutput.ReadAsync(tokenSource.Token);
                            readTask.Wait(tokenSource.Token);

                            input += (char)readTask.Result;

                            if (input == FSharpAwaitingInput)
                            {
                                _outputStream.OnNext(input);

                                if (_stateStream.First() == Repl.State.Starting && !string.IsNullOrEmpty(_startupScript))
                                {
                                    _outputStream.OnNext(_startupScript);
                                    _outputStream.OnNext(Environment.NewLine);

                                    _stateStream.OnNext(Repl.State.Running);

                                    Execute(_startupScript);
                                }
                                else
                                {
                                    _stateStream.OnNext(Repl.State.Running);
                                }

                                break;
                            }

                            if (input.EndsWith(Environment.NewLine))
                            {
                                _outputStream.OnNext(input);
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }, _scheduler)
            .Subscribe(_ => { }, e => _stateStream.OnNext(Repl.State.Faulted));

            return new ReplProcess(process, Disposable.Create(() =>
            {
                tokenSource.Cancel();
                disposable.Dispose();
            }));
        }

        private static string BuildExecutablePath()
        {
            var currrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currrentDirectory == null)
            {
                throw new Exception("Failed to get currrent executing directory.");
            }

            return Path.Combine(Path.Combine(currrentDirectory, FSharpDirectory), FSharpExecutable);
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
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = BuildWorkingDirectory(),
                    FileName = BuildExecutablePath()
                }
            };

            Debug.WriteLine("**************************");
            Debug.WriteLine("WorkingDirectory - " + process.StartInfo.WorkingDirectory);
            Debug.WriteLine("FileName - " + process.StartInfo.FileName);
            Debug.WriteLine("**************************");

            return process;
        }
    }
}