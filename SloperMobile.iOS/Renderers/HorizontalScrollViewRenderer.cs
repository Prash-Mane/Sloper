using SloperMobile.iOS.Renderers;
using SloperMobile.UserControls.CustomControls;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(HorizontalScrollView), typeof(HorizontalScrollViewRenderer))]
namespace SloperMobile.iOS.Renderers
{
    public class HorizontalScrollViewRenderer : ScrollSizeRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            var element = e.NewElement as HorizontalScrollView;
            element?.Render();

            // Don't need these.
            ShowsHorizontalScrollIndicator = false;
        }
    }
}
