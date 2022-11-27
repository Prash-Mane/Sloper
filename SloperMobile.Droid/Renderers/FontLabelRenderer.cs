using System.ComponentModel;
using System.Linq;
using Android.Content;
using SloperMobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using SloperMobile.Droid.Effects;

[assembly: ExportRenderer(typeof(Label), typeof(FontLabelRenderer))]
namespace SloperMobile.Droid.Renderers
{
	public class FontLabelRenderer : LabelRenderer
	{
        public FontLabelRenderer(Context context) : base(context)
		{ }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (Element is IFontElement)
            {
                if (!Element.Effects.Any(eff => eff is FontSizeEffectAndroid))
                    Element.Effects.Add(Effect.Resolve("SloperMobile.FontSizeEffect"));
                if (!Element.Effects.Any(eff => eff is ControlSizeEffectDroid))
                    Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
            }
        }
	}
}