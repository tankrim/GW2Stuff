using System.Globalization;

using Avalonia.Data.Converters;

using BarFoo.Core.DTOs;

namespace BarFoo.Presentation.Converters
{
    public class ProgressPercentageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObjectiveDto objectiveDto)
            {
                if (objectiveDto.ProgressComplete == 0)
                    return 0.0;
                return (double)objectiveDto.ProgressCurrent / objectiveDto.ProgressComplete * 100;
            }
            return 0.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
}