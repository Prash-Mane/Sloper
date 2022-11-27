using System;
using UIKit;
using Acr.Support.iOS;
using Xamarin.Forms;
using SloperMobile.iOS;
using System.Threading.Tasks;

[assembly: Dependency(typeof(AlertsHelper))]
namespace SloperMobile.iOS
{
    public class AlertsHelper : IAlertsHelper
    {
        public async Task CloseAll()
        {
            while (UIApplication.SharedApplication.GetTopViewController() is UIAlertController alertVC)
            {
                await alertVC.DismissViewControllerAsync(false);
                alertVC.Dispose();
                alertVC = null;
            }
        }
    }
}
