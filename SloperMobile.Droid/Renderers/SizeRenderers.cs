using FFImageLoading.Forms;
using FFImageLoading.Forms.Platform;
using SloperMobile.Droid.Renderers;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using SloperMobile.Droid.Effects;
using Syncfusion.SfChart.XForms.Droid;
using Syncfusion.SfChart.XForms;
using SloperMobile.CustomControls;
using SloperMobile.UserControls.CustomControls;

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
namespace SloperMobile.Droid.Renderers
{
	public class LayoutSizeRenderer : VisualElementRenderer<View>
	{
        public LayoutSizeRenderer(Context context):base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ScrollSizeRenderer : ScrollViewRenderer
    {
        public ScrollSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class FrameSizeRenderer : FrameRenderer
    {
        public FrameSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ImageSizeRenderer : ImageRenderer
    {
        public ImageSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class CachedImageSizeRenderer : CachedImageRenderer
    {
        public CachedImageSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<CachedImage> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ListViewSizeRenderer : ListViewRenderer
    {
        public ListViewSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class ButtonSizeRenderer : ButtonRenderer
    {
        public ButtonSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            Control.SetPadding(0, 0, 0, 0);

            if (!Element.Effects.Any(eff => eff is FontSizeEffectAndroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.FontSizeEffect"));
            if (!Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class BoxSizeRenderer : BoxRenderer
    {
        public BoxSizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class EntrySizeRenderer : EntryRenderer
    {
        public EntrySizeRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
                return;

            //Control.SetPadding(0, 0, 0, 0);

            if (!Element.Effects.Any(eff => eff is FontSizeEffectAndroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.FontSizeEffect"));
            if (!Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
        }
    }

    public class SfChartSizeRenderer : SfChartRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SfChart> e)
        {
            base.OnElementChanged(e);

            if (Element != null && !Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));

            if (Control != null && Element is ChartExt formsView)
            {
                var padding = formsView.ChartPadding;
                var rootLayout = Control.GetChildAt(0) as Android.Widget.RelativeLayout;
                if (rootLayout != null)
                    (rootLayout.LayoutParameters as Android.Widget.FrameLayout.LayoutParams).SetMargins((int)padding.Left, (int)padding.Top, (int)padding.Right, (int)padding.Bottom);
            }
        }
    }
}