namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    public sealed class ReplOuputViewModel : BaseViewModel
    {
        public string Value { get; private set; }

        public bool IsError { get; private set; }

        public ReplOuputViewModel(string value, bool isError = false)
        {
            Value = value;
            IsError = isError;
        }
    }
}
