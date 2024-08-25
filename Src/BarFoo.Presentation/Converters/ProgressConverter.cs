using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;

using BarFoo.Infrastructure.DTOs;

namespace BarFoo.Presentation.Converters
{
    public class ProgressConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObjectiveDto objectiveDto)
            {
                return $"{objectiveDto.ProgressCurrent}/{objectiveDto.ProgressComplete}";
            }
            return "N/A";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Since we can't meaningfully convert back to an ObjectiveDto,
            // we return a BindingNotification to indicate the error
            return new BindingNotification(new InvalidOperationException("Cannot convert progress string back to ObjectiveDto"), BindingErrorType.Error);
        }
    }

}
