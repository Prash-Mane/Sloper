using System;
using System.Globalization;
using Xamarin.Forms;

namespace SloperMobile.Common.Converters
{
	public class BoolInvertConverter : IValueConverter
	{
		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}
}
