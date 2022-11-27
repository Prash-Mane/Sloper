using System.Linq;
using SloperMobile.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CarouselPage), typeof(CarouselExPageRenderer))]
namespace SloperMobile.iOS
{
    public class CarouselExPageRenderer : CarouselPageRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                View.Subviews.OfType<UIScrollView>().Single().ContentInsetAdjustmentBehavior =
                        UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
        }
    }
}
