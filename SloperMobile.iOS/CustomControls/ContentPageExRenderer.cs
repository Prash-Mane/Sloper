using System;
using SloperMobile;
using SloperMobile.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(ContentPageExRenderer))]
namespace SloperMobile.iOS
{
    public class ContentPageExRenderer : PageRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Element is ContentPage
                && UIApplication.SharedApplication.KeyWindow != null
                && UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var safeArea = UIApplication.SharedApplication.KeyWindow.SafeAreaInsets;
                if(SizeHelper.SafeArea == default(Thickness))
                    SizeHelper.SafeArea = new Thickness(safeArea.Left,safeArea.Top,safeArea.Right,safeArea.Bottom);
                var pad = ((ContentPage)Element).Padding;
                ((ContentPage)Element).Padding = new Thickness(
                    safeArea.Left, 
                    pad.Top /*+ safeArea.Top*/, 
                    safeArea.Right, 
                    pad.Bottom + safeArea.Bottom);
            }
        }
    }
}
