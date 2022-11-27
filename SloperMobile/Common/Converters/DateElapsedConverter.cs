using System;
using System.Globalization;
using Xamarin.Forms;
using SloperMobile.Common.Helpers;

namespace SloperMobile.Common.Converters
{
 
    public class DateElapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return DateTimeHelper.ConvertDate((DateTime)value);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
