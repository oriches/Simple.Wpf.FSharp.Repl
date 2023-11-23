using System.ComponentModel;

namespace Simple.Wpf.FSharp.Repl.Common.UI.ViewModels
{
    /// <summary>
    ///     Abstract base view model.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}