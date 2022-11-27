using System;
using CoreLocation;
using Foundation;
using SloperMobile.iOS;
using Xamarin.Forms;
using UIKit;

[assembly: Dependency(typeof(LocationBGService))]
namespace SloperMobile.iOS
{
    public class LocationBGService : IBgLocationHelper
    {
        void IBgLocationHelper.StartBGUpdates()
        {
            var status = CLLocationManager.Status;
            if (status == CLAuthorizationStatus.AuthorizedAlways || status == CLAuthorizationStatus.NotDetermined)
                return;

            var appName = NSBundle.MainBundle.InfoDictionary["CFBundleDisplayName"];

            //var alert = UIAlertController.Create("", $"Location services are disabled. Please, go to Settings>{appName} and set location to Always", UIAlertControllerStyle.Alert);
            
            var alert = new UIAlertView("", $"Location services are disabled. Please, go to Settings>{appName} and set location to Always", null, "Cancel", "Settings");
            alert.Clicked += (sender, e) => {
                if (e.ButtonIndex == 1)
                {
                    UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(UIApplication.OpenSettingsUrlString));
                }
            };
            alert.Show();

            throw new Exception("Location is not enabled");
        }

        void IBgLocationHelper.StopBGUpdates()
        {
        }
    }
}
