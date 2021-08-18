using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mp3ToM4b.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool b)
            {
                flag = b;
            }

            var param = true;
            if (bool.TryParse(parameter?.ToString(), out var result))
            {
                param = result;
            }

            return (flag == param ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}