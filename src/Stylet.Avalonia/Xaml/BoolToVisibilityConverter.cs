using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Stylet.Avalonia.Xaml
{
    /// <summary>
    /// Turn a boolean value into a Visibility
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Don't agree with prefixing static method calls with the class name")]
    public class BoolToVisibilityConverter : AvaloniaObject, IValueConverter
    {
        /// <summary>
        /// Singleton instance of this converter. Usage e.g. Converter="{x:Static s:BoolToVisibilityConverter.Instance}"
        /// </summary>
        public static readonly BoolToVisibilityConverter Instance;

        /// <summary>
        /// Singleton instance of this converter, which provides Visibility.Hidden when input is truthsy.
        /// </summary>
        public static readonly BoolToVisibilityConverter InverseInstance;

        static BoolToVisibilityConverter()
        {
            Instance = new BoolToVisibilityConverter();

            // Need to set this in static ctor, otherwise property setter fails (dp is null)
            InverseInstance = new BoolToVisibilityConverter();
            InverseInstance.TrueVisibility = false;
            InverseInstance.FalseVisibility = true;
        }

        /// <summary>
        /// Gets or sets the visibility to use if value is true
        /// </summary>
        public bool TrueVisibility
        {
            get { return (bool)this.GetValue(TrueVisibilityProperty); }
            set { this.SetValue(TrueVisibilityProperty, value); }
        }

        /// <summary>
        /// Property specifying the visibility to return when the parameter is true
        /// </summary>
        public static readonly AvaloniaProperty TrueVisibilityProperty =
            AvaloniaProperty.Register<BoolToVisibilityConverter, bool>("TrueVisibility", true);

        /// <summary>
        /// Gets or sets the visibility to use if value is false
        /// </summary>
        public bool FalseVisibility
        {
            get { return (bool)this.GetValue(FalseVisibilityProperty); }
            set { this.SetValue(FalseVisibilityProperty, value); }
        }

        /// <summary>
        /// Property specifying the visibility to return when the parameter is false
        /// </summary>
        public static readonly AvaloniaProperty FalseVisibilityProperty =
            AvaloniaProperty.Register<BoolToVisibilityConverter, bool>("FalseVisibility", false);

        /// <summary>
        /// Perform the conversion
        /// </summary>
        /// <param name="value">value as produced by source binding</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="culture">culture information</param>
        /// <returns>Converted value</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;
            if (value == null)
            {
                result = false;
            }
            else if (value is bool)
            {
                result = (bool)value;
            }
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            else if (value is IEnumerable)
            {
                result = ((IEnumerable)value).GetEnumerator().MoveNext();
            }
            else if (!(value is ValueType))
            {
                result = true; // Non-null non-enumerable reference type = true
            }
            else 
            {
                // Value types from here on in

                // This fails if an int can't be converted to it, or for many other reasons
                // Easiest is just to try it and see
                try
                {
                    result = !value.Equals(System.Convert.ChangeType(0, value.GetType()));
                }
                catch
                {
                    result = true; // Not null, didn't meet any other falsy behaviour
                }
            }

            return result ? this.TrueVisibility : this.FalseVisibility;
        }

        /// <summary>
        /// Perform the inverse conversion. Only valid if the value is bool
        /// </summary>
        /// <param name="value">value, as produced by target</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="culture">culture information</param>
        /// <returns>Converted back value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new ArgumentException("Can't ConvertBack on BoolToVisibilityConverter when TargetType is not bool");

            if (!(value is bool))
                return null;

            var vis = (bool)value;

            if (vis == this.TrueVisibility)
                return true;
            if (vis == this.FalseVisibility)
                return false;
            return null;
        }
    }
}
