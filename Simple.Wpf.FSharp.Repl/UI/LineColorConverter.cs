using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Simple.Wpf.FSharp.Repl.UI.ViewModels;

namespace Simple.Wpf.FSharp.Repl.UI
{
    /// <summary>
    ///     Line color converter used with Simple.Wpf.Terminal control.
    /// </summary>
    public sealed class LineColorConverter : DependencyObject, IValueConverter
    {
        /// <summary>
        ///     Dependency property for the Normal line color.
        /// </summary>
        public static readonly DependencyProperty NormalProperty = DependencyProperty.Register("Normal",
            typeof(Color),
            typeof(LineColorConverter),
            new PropertyMetadata(Colors.Black, OnNormalChanged));

        /// <summary>
        ///     Dependency property for the Error line color.
        /// </summary>
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error",
            typeof(Color),
            typeof(LineColorConverter),
            new PropertyMetadata(Colors.Black, OnErrorChanged));

        private SolidColorBrush _errorBrush;

        private SolidColorBrush _normalBrush;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public LineColorConverter()
        {
            _normalBrush = new SolidColorBrush(Normal);
            _errorBrush = new SolidColorBrush(Error);
        }

        /// <summary>
        ///     Normal line color property.
        /// </summary>
        public Color Normal
        {
            get => (Color)GetValue(NormalProperty);
            set => SetValue(NormalProperty, value);
        }

        /// <summary>
        ///     Error line color property.
        /// </summary>
        public Color Error
        {
            get => (Color)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        /// <summary>
        ///     Converts the line to a specific color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return _normalBrush;

            if (value is ReplLineViewModel)
            {
                var line = (ReplLineViewModel)value;
                return line.IsError ? _errorBrush : _normalBrush;
            }

            return _normalBrush;
        }

        /// <summary>
        ///     Does nothing.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Binding.DoNothing;

        private static void OnNormalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((LineColorConverter)d)._normalBrush = new SolidColorBrush((Color)e.NewValue);

        private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((LineColorConverter)d)._errorBrush = new SolidColorBrush((Color)e.NewValue);
    }
}