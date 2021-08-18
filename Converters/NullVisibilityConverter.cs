using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mp3ToM4b.Converters
{
    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = value == null ? Visibility.Hidden : Visibility.Visible;
            if (bool.TryParse(parameter?.ToString(), out bool param))
            {
                if (!param)
                {
                    visibility = value == null ? Visibility.Visible : Visibility.Hidden;
                }
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}