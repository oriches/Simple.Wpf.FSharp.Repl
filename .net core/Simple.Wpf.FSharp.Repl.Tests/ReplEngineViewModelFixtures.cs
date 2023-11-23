using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;
using Simple.Wpf.FSharp.Repl.Common.UI.ViewModels;
using Simple.Wpf.FSharp.Repl.UI.ViewModels;

namespace Simple.Wpf.FSharp.Repl.Tests
{
    [TestFixture]
    public sealed class ReplEngineViewModelFixtures
    {
        [SetUp]
        public void SetUp()
        {
            _stateSubject = new Subject<State>();
            _replOutputSubject = new Subject<ReplLineViewModel>();
            _replErrorSubject = new Subject<ReplLineViewModel>();
            _workingDirectory = @"c:\temp\fsharp";

            _process = new Mock<IProcess>();
            _processService = new Mock<IProcessService>(MockBehavior.Strict);

            _scheduler = new TestScheduler();

            _viewModel = new ReplEngineViewModel(_stateSubject, _replOutputSubject, _replErrorSubject,
                _workingDirectory, _processService.Object);
        }

        private Subject<State> _stateSubject;
        private Subject<ReplLineViewModel> _replOutputSubject;
        private Subject<ReplLineViewModel> _replErrorSubject;
        private string _workingDirectory;
        private Mock<IProcessService> _processService;
        private ReplEngineViewModel _viewModel;

        private TestScheduler _scheduler;
        private Mock<IProcess> _process;

        [Test]
        public void clear_disabled_with_no_output()
        {
            // ARRANGE
            // ACT
            var canClear = _viewModel.ClearCommand.CanExecute(null);

            // ASSERT
            Assert.That(canClear, Is.False);
        }

        [Test]
        public void clear_enabled_with_output()
        {
            // ARRANGE
            _replOutputSubject.OnNext(new ReplLineViewModel("line 1"));

            // ACT
            var canClear = _viewModel.ClearCommand.CanExecute(null);

            // ASSERT
            Assert.That(canClear, Is.True);
        }

        [Test]
        public void clears_output()
        {
            // ARRANGE
            _replOutputSubject.OnNext(new ReplLineViewModel("line 1"));
            _replOutputSubject.OnNext(new ReplLineViewModel("line 2"));

            var initialOutputCount = _viewModel.Output.Count();

            // ACT
            _viewModel.ClearCommand.Execute(null);

            // ASSERT
            var finalOutputCount = _viewModel.Output.Count();
            Assert.That(finalOutputCount, Is.EqualTo(0));
            Assert.That(initialOutputCount, Is.Not.EqualTo(finalOutputCount));
        }

        [Test]
        public void execute_is_disabled_when_engine_is_not_running_and_not_executing()
        {
            // ARRANGE
            // ACT
            _stateSubject.OnNext(State.Starting);
            var canWhenRunning = _viewModel.ExecuteCommand.CanExecute(null);

            _stateSubject.OnNext(State.Faulted);
            var canWhenExecuting = _viewModel.ExecuteCommand.CanExecute(null);

            // ASSERT
            Assert.That(canWhenRunning, Is.False);
            Assert.That(canWhenExecuting, Is.False);
        }

        [Test]
        public void execute_is_enabled_when_engine_is_running_or_executing()
        {
            // ARRANGE
            // ACT
            _stateSubject.OnNext(State.Running);
            var canWhenRunning = _viewModel.ExecuteCommand.CanExecute(null);

            _stateSubject.OnNext(State.Executing);
            var canWhenExecuting = _viewModel.ExecuteCommand.CanExecute(null);

            // ASSERT
            Assert.That(canWhenRunning, Is.True);
            Assert.That(canWhenExecuting, Is.True);
        }

        [Test]
        public void opens_working_folder()
        {
            // ARRANGE
            _processService.Setup(x => x.StartWindowsExplorer(It.IsAny<string>())).Returns(_process.Object)
                .Verifiable();

            // ACT
            _viewModel.OpenWorkingFolderCommand.Execute(null);

            // ASSERT
            _processService.Verify(x => x.StartWindowsExplorer(_workingDirectory), Times.Once);
        }

        [Test]
        public void pumps_when_execution_requested()
        {
            // ARRANGE
            _stateSubject.OnNext(State.Running);

            var called = false;
            _viewModel.Execute
                .ObserveOn(_scheduler)
                .Subscribe(x => called = true);

            // ACT
            _viewModel.ExecuteCommand.Execute("line 1");

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // ASSERT
            Assert.That(called, Is.True);
        }

        [Test]
        public void pumps_when_reset_requested()
        {
            // ARRANGE
            _stateSubject.OnNext(State.Running);

            var called = false;
            _viewModel.Reset
                .ObserveOn(_scheduler)
                .Subscribe(x => called = true);

            // ACT
            _viewModel.ResetCommand.Execute(null);

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // ASSERT
            Assert.That(called, Is.True);
        }

        [Test]
        public void reset_is_disabled_when_engine_is_not_running()
        {
            // ARRANGE
            // ACT
            _stateSubject.OnNext(State.Starting);

            // ASSERT
            Assert.That(_viewModel.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void reset_is_enabled_when_engine_is_running()
        {
            // ARRANGE
            // ACT
            _stateSubject.OnNext(State.Running);

            // ASSERT
            Assert.That(_viewModel.ResetCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void when_engine_is_executing_then_state_is_not_null()
        {
            // ARRANGE
            _stateSubject.OnNext(State.Running);

            // ACT
            _stateSubject.OnNext(State.Executing);

            // ASSERT
            Assert.That(_viewModel.State, Is.EqualTo("Executing"));
        }

        [Test]
        public void when_engine_is_not_executing_then_state_is_null()
        {
            // ARRANGE
            _stateSubject.OnNext(State.Executing);

            // ACT
            _stateSubject.OnNext(State.Running);

            // ASSERT
            Assert.That(_viewModel.State, Is.Not.EqualTo("Executing"));
        }

        [Test]
        public void when_repl_error_pumps_then_output_is_updated()
        {
            // ARRANGE
            var initialOutput = _viewModel.Output.ToArray();

            // ACT
            _replErrorSubject.OnNext(new ReplLineViewModel("error 1"));
            _replErrorSubject.OnNext(new ReplLineViewModel("error 2"));
            _replErrorSubject.OnNext(new ReplLineViewModel("error 3"));

            var finalOutput = _viewModel.Output.ToArray();

            // ASSERT
            Assert.That(initialOutput, Is.Empty);
            Assert.That(finalOutput.Count(), Is.EqualTo(3));
            Assert.That(finalOutput.First().Value, Is.EqualTo("error 1"));
            Assert.That(finalOutput.Skip(1).First().Value, Is.EqualTo("error 2"));
            Assert.That(finalOutput.Skip(2).First().Value, Is.EqualTo("error 3"));
        }

        [Test]
        public void when_repl_output_pumps_then_output_is_updated()
        {
            // ARRANGE
            var initialOutput = _viewModel.Output.ToArray();

            // ACT
            _replOutputSubject.OnNext(new ReplLineViewModel("line 1"));
            _replOutputSubject.OnNext(new ReplLineViewModel("line 2"));
            _replOutputSubject.OnNext(new ReplLineViewModel("line 3"));

            var finalOutput = _viewModel.Output.ToArray();

            // ASSERT
            Assert.That(initialOutput, Is.Empty);
            Assert.That(finalOutput.Count(), Is.EqualTo(3));
            Assert.That(finalOutput.First().Value, Is.EqualTo("line 1"));
            Assert.That(finalOutput.Skip(1).First().Value, Is.EqualTo("line 2"));
            Assert.That(finalOutput.Skip(2).First().Value, Is.EqualTo("line 3"));
        }
    }
}