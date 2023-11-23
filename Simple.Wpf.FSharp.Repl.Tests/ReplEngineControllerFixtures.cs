using System;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Simple.Wpf.FSharp.Repl.Core;
using Simple.Wpf.FSharp.Repl.Services;
using Simple.Wpf.FSharp.Repl.Tests.Extensions;
using Simple.Wpf.FSharp.Repl.UI.Controllers;
using Simple.Wpf.FSharp.Repl.UI.ViewModels;

namespace Simple.Wpf.FSharp.Repl.Tests
{
    [TestFixture]
    public class ReplEngineControllerFixtures
    {
        [SetUp]
        public void Setup()
        {
            _testScheduler = new TestScheduler();

            _processService = new Mock<IProcessService>(MockBehavior.Strict);

            _errorSubject = new Subject<string>();
            _outputSubject = new Subject<string>();
            _stateSubject = new Subject<State>();

            _replEngine = new Mock<IReplEngine>(MockBehavior.Strict);
            _replEngine.Setup(x => x.Error)
                .Returns(_errorSubject);
            _replEngine.Setup(x => x.Output)
                .Returns(_outputSubject);
            _replEngine.Setup(x => x.State)
                .Returns(_stateSubject);

            _replEngine.Setup(x => x.WorkingDirectory)
                .Returns((string)null);
            _replEngine.Setup(x => x.Start(null))
                .Returns(_replEngine.Object);
        }

        private Mock<IProcessService> _processService;
        private Mock<IReplEngine> _replEngine;
        private TestScheduler _testScheduler;
        private Subject<string> _errorSubject;
        private Subject<string> _outputSubject;
        private Subject<State> _stateSubject;

        [Test]
        public void repl_engine_executes_script_when_controller_execute_is_called()
        {
            // ARRANGE
            const string script = "let x = 345;;";
            _replEngine.Setup(x => x.Execute(script))
                .Returns(_replEngine.Object)
                .Verifiable();
            var controller =
                new ReplEngineController(null, null, _replEngine.Object, _processService.Object, _testScheduler);
            var viewModel = (ReplEngineViewModel)controller.ViewModel;

            // ACT
            controller.Execute(script);

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ASSERT
            _replEngine.Verify(x => x.Execute(script), Times.Once);
        }

        [Test]
        public void repl_engine_executes_when_view_model_execute_is_called()
        {
            // ARRANGE
            var script = @"let x = 123;;";
            _replEngine.Setup(x => x.Execute(script))
                .Returns(_replEngine.Object)
                .Verifiable();
            var controller = new ReplEngineController(null, null, _replEngine.Object, _processService.Object,
                _testScheduler, _testScheduler);
            var viewModel = (ReplEngineViewModel)controller.ViewModel;
            _stateSubject.OnNext(State.Running);

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ACT
            viewModel.ExecuteCommand.Execute(script);

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ASSERT
            _replEngine.Verify(x => x.Execute(script), Times.Once);
        }

        [Test]
        public void repl_engine_generates_error_output_then_view_model_is_updated()
        {
            // ARRANGE
            var controller =
                new ReplEngineController(null, null, _replEngine.Object, _processService.Object, _testScheduler);
            var viewModel = (ReplEngineViewModel)controller.ViewModel;

            // ACT
            _errorSubject.OnNext("error 1");

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ASSERT
            Assert.That(viewModel.Output.Count(), Is.EqualTo(1));
            Assert.That(viewModel.Output.First()
                .Value, Is.EqualTo("error 1"));
            Assert.That(viewModel.Output.First()
                .IsError, Is.True);
        }


        [Test]
        public void repl_engine_generates_output_then_view_model_is_updated()
        {
            // ARRANGE
            var controller =
                new ReplEngineController(null, null, _replEngine.Object, _processService.Object, _testScheduler);
            var viewModel = (ReplEngineViewModel)controller.ViewModel;

            // ACT
            _outputSubject.OnNext("line 1");

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ASSERT
            Assert.That(viewModel.Output.Count(), Is.EqualTo(1));
            Assert.That(viewModel.Output.First()
                .Value, Is.EqualTo("line 1"));
            Assert.That(viewModel.Output.First()
                .IsError, Is.False);
        }

        [Test]
        public void repl_engine_is_only_started_once()
        {
            // ARRANGE
            var controller =
                new ReplEngineController(null, null, _replEngine.Object, _processService.Object, _testScheduler);

            // ACT
            var viewModel1 = controller.ViewModel;
            var viewModel2 = controller.ViewModel;

            // ASSERT
            Assert.That(viewModel1, Is.Not.Null);
            Assert.That(viewModel2, Is.Not.Null);
            _replEngine.Verify(x => x.Start(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void repl_engine_is_started_when_view_model_is_accessed()
        {
            // ARRANGE
            var controller =
                new ReplEngineController(null, null, _replEngine.Object, _processService.Object, _testScheduler);

            // ACT
            var viewModel = controller.ViewModel;

            // ASSERT
            Assert.That(viewModel, Is.Not.Null);
            _replEngine.Verify(x => x.Start(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void repl_engine_resets_when_view_model_reset_is_called()
        {
            // ARRANGE
            _replEngine.Setup(x => x.Reset())
                .Returns(_replEngine.Object)
                .Verifiable();
            var controller = new ReplEngineController(null, null, _replEngine.Object, _processService.Object,
                _testScheduler, _testScheduler);
            var viewModel = (ReplEngineViewModel)controller.ViewModel;
            _stateSubject.OnNext(State.Running);

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ACT
            viewModel.ResetCommand.Execute(null);

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1));

            // ASSERT
            _replEngine.Verify(x => x.Reset(), Times.Once);
        }
    }
}