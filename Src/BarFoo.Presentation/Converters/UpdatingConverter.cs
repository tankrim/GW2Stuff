using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BarFoo.Presentation.Converters
{
    public class UpdatingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && b ? "Updating..." : "";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new BindingNotification(new InvalidOperationException("Cannot convert progress string back to bool"), BindingErrorType.Error);
        }
    }

}
