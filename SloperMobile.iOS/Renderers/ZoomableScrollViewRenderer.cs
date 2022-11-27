using System.Threading.Tasks;
using SloperMobile.iOS.Renderers;
using SloperMobile.ViewModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ZoomableScrollView), typeof(ZoomableScrollViewRenderer))]

namespace SloperMobile.iOS.Renderers
{
    public class ZoomableScrollViewRenderer : ScrollViewRenderer
    {
        private ZoomableScrollViewRenderer This {
            get{
                return this;
            }
        }

        protected async override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            if (e.NewElement == null)
                return;

            if (e.OldElement == null)
            {
                var zsv = Element as ZoomableScrollView;
	            zsv.ScaleFactor = 1;
				this.MinimumZoomScale = 1;
                this.MaximumZoomScale = 5;
                this.ViewForZoomingInScrollView +=
                    (UIScrollView sv) =>
                    {
                        var view = this.Subviews[0];//.Subviews[0];
                        
                        return view;
                    };

                this.ZoomingEnded += (sender, eventArgs) => {
                    if(zsv == null){
                        return;
                    }

                    zsv.ScaleFactor = (float)eventArgs.AtScale;
                    zsv.IsScalingUp = true;
                    zsv.IsScalingDown = true;
                    zsv.RescaleOniOS?.Invoke();
                };

                await Task.Delay(1000);
                if (zsv != null) {
                    zsv.IsScalingUp = true;
                    zsv.IsScalingDown = true;
                    zsv.RescaleOniOS?.Invoke();
                }

                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    Bounces = false;
                }
            }
        }
    }
}