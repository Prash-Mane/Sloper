using System;
using System.Globalization;
using Syncfusion.DataSource.Extensions;
using Xamarin.Forms;
using System.Linq;
using SloperMobile.Model.PurchaseModels;

namespace SloperMobile
{
    public class PurchaseHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GroupResult model)
                return model.Items != null && model.Level == 4 && model.Count > 0 && model.Items.ToList<PurchaseCrag>().First().ShowGuidebookDownload;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
