using System;
using System.Globalization;
using Xamarin.Forms;

namespace SloperMobile
{
    public class GroupPaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var lvl = (int)value;
                var leftPad = (lvl - 1) * 20 + 5;
                return new Thickness(leftPad, 0, 0, 0);
            }
            catch {
                return new Thickness(5, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
