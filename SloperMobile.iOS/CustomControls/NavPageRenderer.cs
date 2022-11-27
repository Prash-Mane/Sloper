using System;
using System.Threading.Tasks;
using SloperMobile;
using SloperMobile.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer))]
namespace SloperMobile.iOS
{
    public class NavigationPageRenderer : NavigationRenderer
    {
        bool isLoaded;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (isLoaded)
                return;

            //for some reason, nav controller is changing status bar while loaded. This hack will set it back before appearing
            isLoaded = true;
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
        }
    }
}
