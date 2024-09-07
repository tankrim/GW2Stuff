using System.Globalization;

using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BarFoo.Presentation.Converters;

public class NotificationTypeToClassConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is NotificationType type
            ? type switch
            {
                NotificationType.Error => "Error",
                NotificationType.Warning => "Warning",
                NotificationType.Success => "Success",
                _ => string.Empty
            }
            : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new InvalidOperationException("Cannot convert class back to NotificationType"), BindingErrorType.Error);
    }
}
