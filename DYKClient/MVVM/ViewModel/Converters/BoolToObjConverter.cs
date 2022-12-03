using System;
using System.Globalization;
using System.Windows.Data;

namespace DYKClient.MVVM.ViewModel.Converters
{
    public class BoolToObjConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Gotowy";
            }
            else
            {
                return "Nie gotowy";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value).Equals(true);
        }
    }
}
