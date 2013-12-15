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

    public sealed class ReplEngine : IReplEngine
    {
        private readonly IScheduler _scheduler;
        private const string FSharpDirectory = @"FSharp";
        private const string FSharpExecutable = @"fsi.exe";
        private const string FSharpEndofLineCharacter = ";";
        private const string FSharpQuitLine = "#quit;;";
        
        private readonly string _baseWorkingDirectory;
        private readonly Subject<string> _outputStream;

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
            _scheduler = scheduler ?? TaskPoolScheduler.Default;

            _outputStream = new Subject<string>();

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                _baseWorkingDirectory = workingDirectory.Trim();
            }
        }

        public IObservable<string> Output { get { return _outputStream; } }  

        public IReplEngine Start(string script = null)
        {
            if (_replProcess != null)
            {
                return this;
            }

            _startupScript = CleanScript(script);
            _replProcess = StartProcess();

            return this;
        }

        public IReplEngine Stop()
        {
            if (_replProcess == null)
            {
                return this;
            }

            _replProcess.Dispose();
            _replProcess = null;

            _startupScript = null;

            return this;
        }

        public IReplEngine Reset()
        {
            if (_replProcess == null)
            {
                return this;
            }

           _replProcess.Dispose();
           _replProcess = StartProcess();

            return this;
        }
        
        public IReplEngine Execute(string script)
        {
            if (_replProcess == null)
            {
                return this;
            }

            if (string.IsNullOrWhiteSpace(script))
            {
                return this;
            }

            var cleanedScript = CleanScript(script);
            _replProcess.WriteLine(cleanedScript);

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
                if (!string.IsNullOrEmpty(_startupScript))
                {
                    process.StandardInput.WriteLine(_startupScript);
                }

                while (true)
                {
                    try
                    {
                        var readLineTask = process.StandardOutput.ReadLineAsync(tokenSource.Token);
                        readLineTask.Wait(tokenSource.Token);

                        var line = readLineTask.Result;
                        _outputStream.OnNext(line);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }, _scheduler)
            .Subscribe();

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

            Debug.WriteLine("WorkingDirectory - " + process.StartInfo.WorkingDirectory);
            Debug.WriteLine("FileName - " + process.StartInfo.FileName);

            return process;
        }

        private static string CleanScript(string script)
        {
            var cleanedScript = script;
            if (!string.IsNullOrWhiteSpace(script))
            {
                cleanedScript = script.Trim();

                const string endofline = FSharpEndofLineCharacter + FSharpEndofLineCharacter;
                if (cleanedScript.EndsWith(endofline))
                {
                    // do nothing, already terminated...
                }
                else if (cleanedScript.EndsWith(FSharpEndofLineCharacter))
                {
                    cleanedScript += FSharpEndofLineCharacter;
                }
                else
                {
                    cleanedScript += endofline;
                }
            }

            return cleanedScript;
        }
    }
}