namespace Simple.Wpf.FSharp.Repl.Tests
{
    using NUnit.Framework;
    using UI.ViewModels;

    [TestFixture]
    public class ReplOuputViewModelFixtures
    {
        [Test]
        public void is_errored()
        {
            // ARRANGE
            // ACT
            var viewModel = new ReplLineViewModel("stdin(2,1): error FS0039: The value or constructor 'sssss' is not defined", true);

            // ASSERT
            Assert.That(viewModel.IsError, Is.True);
        }

        [Test]
        public void is_not_errored()
        {
            // ARRANGE
            // ACT
            var viewModel = new ReplLineViewModel("val x : float = 23.0");

            // ASSERT
            Assert.That(viewModel.IsError, Is.False);
        }
    }
}
