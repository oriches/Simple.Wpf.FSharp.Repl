namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    public sealed class ReplLineViewModel : BaseViewModel
    {
        public string Value { get; private set; }

        public bool IsError { get; private set; }

        public ReplLineViewModel(string value, bool isError = false)
        {
            Value = value;
            IsError = isError;
        }
    }
}
