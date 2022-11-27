using System;
using System.Globalization;
using Xamarin.Forms;

namespace SloperMobile
{
    public class ExpandImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return ImageSource.FromFile("arrowUp.png");
            else
                return ImageSource.FromFile("arrowDown.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
