using FFImageLoading.Forms;
using FFImageLoading.Forms.Platform;
using SloperMobile.iOS.Effects;
using SloperMobile.iOS.Renderers;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Syncfusion.SfChart.XForms.iOS.Renderers;
using Syncfusion.SfChart.XForms;
using SloperMobile.CustomControls;
using System;

[assembly: ExportRenderer(typeof(View), typeof(LayoutSizeRenderer))]
[assembly: ExportRenderer(typeof(ScrollView), typeof(ScrollSizeRenderer))]
[assembly: ExportRenderer(typeof(Frame), typeof(FrameSizeRenderer))]
[assembly: ExportRenderer(typeof(Image), typeof(ImageSizeRenderer))]
[assembly: ExportRenderer(typeof(CachedImage), typeof(CachedImageSizeRenderer))]
[assembly: ExportRenderer(typeof(ListView), typeof(ListViewSizeRenderer))]
[assembly: ExportRenderer(typeof(Button), typeof(ButtonSizeRenderer))]
[assembly: ExportRenderer(typeof(BoxView), typeof(BoxSizeRenderer))]
[assembly: ExportRenderer(typeof(Entry), typeof(EntrySizeRenderer))]
[assembly: ExportRenderer(typeof(ChartExt), typeof(SfChartSizeRenderer))]
namespace SloperMobile.iOS.Renderers
{
	public class LayoutSizeRenderer : VisualElementRenderer<View>
	{
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ScrollSizeRenderer : ScrollViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0) && NativeView is UIScrollView scrollView)
                scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
        }
    }

    public class FrameSizeRenderer :  FrameRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ImageSizeRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class CachedImageSizeRenderer : CachedImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<CachedImage> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ListViewSizeRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0) && Control != null)
                Control.ContentInsetAdjustmentBehavior = UIKit.UIScrollViewContentInsetAdjustmentBehavior.Never;
        }
    }

    public class ButtonSizeRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            if (!Element.Effects.Any(eff => eff is FontEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.FontSizeEffect"));
            if (!Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class BoxSizeRenderer : BoxRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class EntrySizeRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            if (!Element.Effects.Any(eff => eff is FontEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.FontSizeEffect"));
            if (!Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class SfChartSizeRenderer : SfChartRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SfChart> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));

            if (Control != null && Element is ChartExt formsView)
            {
                var padding = formsView.ChartPadding;
                Control.EdgeInsets = new UIEdgeInsets((nfloat)padding.Top, (nfloat)padding.Left, (nfloat)padding.Bottom, (nfloat)padding.Right);
            }
        }
    }
}