namespace Simple.Wpf.FSharp.Repl.UI.ViewModels
{
    /// <summary>
    /// REPL engine line output.
    /// </summary>
    public sealed class ReplLineViewModel : BaseViewModel
    {
        /// <summary>
        /// The value of line output.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Is the output line an error.
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Creates a line ViewModel.
        /// </summary>
        /// <param name="value">The value of the output line.</param>
        /// <param name="isError">Is the output line an error, default is false.</param>
        public ReplLineViewModel(string value, bool isError = false)
        {
            Value = value;
            IsError = isError;
        }
    }
}
