namespace Simple.Wpf.FSharp.Repl.Views
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    public sealed class Terminal : RichTextBox
    {
        public event EventHandler LineChanged;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof (IEnumerable),
            typeof (Terminal),
            new PropertyMetadata(default(IEnumerable), OnItemsSourceChanged));

        public static readonly DependencyProperty DisplayPathProperty = DependencyProperty.Register("DisplayPath",
            typeof (string),
            typeof (Terminal),
            new PropertyMetadata(default(string), OnDisplayPathChanged));

        public static readonly DependencyProperty IsErrorPathProperty = DependencyProperty.Register("IsErrorPath",
            typeof(string),
            typeof(Terminal),
            new PropertyMetadata(default(string), OnIsErrorPathChanged));

        public static readonly DependencyProperty LineProperty = DependencyProperty.Register("Line",
            typeof(string),
            typeof(Terminal),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty PromptProperty = DependencyProperty.Register("Prompt",
            typeof(string),
            typeof(Terminal),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ErrorColorProperty = DependencyProperty.Register("ErrorColor",
           typeof(Brush),
           typeof(Terminal),
           new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        private readonly Paragraph _paragraph;
        private readonly SerialDisposable _collectionDisposable;

        private PropertyInfo _displayPathProperty;
        private PropertyInfo _isErrorPathProperty;

        private Run _promptInline;
       
        public Terminal()
        {
            _paragraph = new Paragraph();
            _paragraph.Margin = new Thickness(0);
            _paragraph.LineHeight = 10;
            
            _collectionDisposable = new SerialDisposable();

            Document = new FlowDocument(_paragraph);

            TextChanged += (s, e) => ScrollToEnd();

            DataObject.AddPastingHandler(this, PasteCommand);
            DataObject.AddCopyingHandler(this, CopyCommand);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string DisplayPath
        {
            get { return (string) GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }

        public string IsErrorPath
        {
            get { return (string)GetValue(IsErrorPathProperty); }
            set { SetValue(IsErrorPathProperty, value); }
        }

        public string Line
        {
            get { return (string)GetValue(LineProperty); }
            set { SetValue(LineProperty, value); }
        }

        public string Prompt
        {
            get { return (string)GetValue(PromptProperty); }
            set { SetValue(PromptProperty, value); }
        }

        public Brush ErrorColor
        {
            get { return (Brush)GetValue(ErrorColorProperty); }
            set { SetValue(ErrorColorProperty, value); }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Enter)
            {
                HandleEnterKey();
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.Back)
            {
                var promptEnd = _promptInline.ContentEnd;

                var textPointer = GetTextPointer(promptEnd, LogicalDirection.Forward);
                if (textPointer == null)
                {
                    if (CaretPosition.CompareTo(promptEnd) == 0)
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    if (CaretPosition.CompareTo(textPointer) == 0)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue)
            {
                return;
            }

            var terminal = ((Terminal) d);
            if (args.NewValue is INotifyCollectionChanged)
            {
                terminal.ObserveChanges((IEnumerable)args.NewValue);
            }
            else
            {
                terminal.ReplaceValues((IEnumerable)args.NewValue);
            }
        }

        private static void OnDisplayPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var terminal = ((Terminal)d);
            terminal._displayPathProperty = null;
        }

        private static void OnIsErrorPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var terminal = ((Terminal)d);
            terminal._isErrorPathProperty = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _promptInline = new Run(Prompt);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _collectionDisposable.Disposable = Disposable.Empty;
        }

        private void CopyCommand(object sender, DataObjectCopyingEventArgs args)
        {
            if (!string.IsNullOrEmpty(Selection.Text))
            {
                args.DataObject.SetData(typeof(string), Selection.Text);
            }

            args.Handled = true;
        }

        private void PasteCommand(object sender, DataObjectPastingEventArgs args)
        {
            var text = (string)args.DataObject.GetData(typeof(string));

            if (!string.IsNullOrEmpty(text))
            {
                CaretPosition = CaretPosition.DocumentEnd;

                var inline = new Run(text);
                _paragraph.Inlines.Add(inline);

                CaretPosition = CaretPosition.DocumentEnd;
            }

            args.CancelCommand();
            args.Handled = true;
        }

        private void ObserveChanges(IEnumerable values)
        {
            var notifyChanged = (INotifyCollectionChanged) values;

            _collectionDisposable.Disposable = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => notifyChanged.CollectionChanged += h, h => notifyChanged.CollectionChanged -= h)
                .Subscribe(x =>
                    {
                        _paragraph.Inlines.Remove(_promptInline);

                        if (x.EventArgs.Action == NotifyCollectionChangedAction.Add)
                        {
                            AddOutputs(x.EventArgs.NewItems.Cast<object>());
                        }
                        else
                        {
                            ReplaceValues(values);
                        }

                        _paragraph.Inlines.Add(_promptInline);
                        CaretPosition = CaretPosition.DocumentEnd;
                    });
        }

        private void ReplaceValues(IEnumerable outputs)
        {
            _paragraph.Inlines.Clear();
            AddOutputs(ConvertToEnumerable(outputs));

            _paragraph.Inlines.Add(_promptInline);
            CaretPosition = CaretPosition.DocumentEnd;
        }

        private void AddOutputs(IEnumerable outputs)
        {
            foreach (var output in outputs.Cast<object>())
            {
                var value = ExtractValue(output);
                var isError = ExtractIsError(output);

                var inline = new Run(value);
                if (isError)
                {
                    inline.Foreground = ErrorColor;
                }

                _paragraph.Inlines.Add(inline);
            }
        }

        private static IEnumerable<object> ConvertToEnumerable(object values)
        {
            try
            {
                return values == null ? Enumerable.Empty<object>() : ((IEnumerable)values).Cast<object>();
            }
            catch (Exception)
            {
                return Enumerable.Empty<object>();
            }   
        }

        private static TextPointer GetTextPointer(TextPointer textPointer, LogicalDirection direction)
        {
            var currentTextPointer = textPointer;
            while (currentTextPointer != null)
            {
                var nextPointer = currentTextPointer.GetNextContextPosition(direction);
                if (nextPointer == null)
                {
                    return null;
                }

                if (nextPointer.GetPointerContext(direction) == TextPointerContext.Text)
                {
                    return nextPointer;
                }

                currentTextPointer = nextPointer;
            }

            return null;
        }

        private string ExtractValue(object output)
        {
            var displayPath = DisplayPath;
            if (displayPath == null)
            {
                return output == null ? string.Empty : output.ToString();
            }

            if (_displayPathProperty == null)
            {
                _displayPathProperty = output.GetType().GetProperty(displayPath);
            }

            var value = _displayPathProperty.GetValue(output, null);
            return value == null ? string.Empty : value.ToString();
        }

        private bool ExtractIsError(object output)
        {
            var isErrorPath = IsErrorPath;
            if (isErrorPath == null)
            {
                return false;
            }

            if (_isErrorPathProperty == null)
            {
                _isErrorPathProperty = output.GetType().GetProperty(isErrorPath);
            }

            var value = _isErrorPathProperty.GetValue(output, null);
            return (bool)value;
        }
        
        private void HandleEnterKey()
        {
            var inlineList = _paragraph.Inlines.ToList();
            var promptIndex = inlineList.IndexOf(_promptInline);

            var line = inlineList.Where((x, i) => i > promptIndex)
                .Cast<Run>()
                .Select(x => x.Text)
                .Aggregate(string.Empty, (current, part) => current + part);

            foreach (var inline in inlineList.Where((x, i) => i > promptIndex))
            {
                _paragraph.Inlines.Remove(inline);
            }

            Line = line;
            
            CaretPosition = CaretPosition.DocumentEnd;
            
            OnLineEntered();
        }
        
        private void OnLineEntered()
        {
            var handler = LineChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}