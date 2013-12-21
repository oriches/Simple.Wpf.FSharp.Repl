namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    using System.Text.RegularExpressions;

    public sealed class ReplOuputViewModel : BaseViewModel
    {
        private static readonly Regex IsErrorRegex = new Regex(@"stdin\((\d+),(\d+)\): error FS");

        public string Value { get; private set; }

        public bool IsError { get; private set; }

        public ReplOuputViewModel(string value)
        {
            Value = value;
            IsError = IsErrorRegex.IsMatch(value);
        }
    }
}
