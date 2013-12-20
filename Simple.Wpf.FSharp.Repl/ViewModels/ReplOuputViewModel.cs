namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    using System.Text.RegularExpressions;

    public sealed class ReplOuputViewModel
    {
        private static readonly Regex IsErrorRegex = new Regex(@"stdin\((\d+),(\d+)\): error FS");

        public string Output { get; private set; }

        public bool IsError { get; private set; }

        public ReplOuputViewModel(string output)
        {
            Output = output;
            IsError = IsErrorRegex.IsMatch(output);
        }
    }
}
