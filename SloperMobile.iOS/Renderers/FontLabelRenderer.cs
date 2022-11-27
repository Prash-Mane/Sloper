using System.ComponentModel;
using SloperMobile.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;
using System.Linq;
using SloperMobile.Effects;
using SloperMobile.iOS.Effects;

[assembly: ExportRenderer(typeof(Label), typeof(FontLabelRenderer))]
namespace SloperMobile.iOS.Renderers
{
	public class FontLabelRenderer : LabelRenderer
	{
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (Element is IFontElement)
            {
                if (!Element.Effects.Any(eff => eff is FontEffectiOS))
                    Element.Effects.Add(Effect.Resolve("SloperMobile.FontSizeEffect"));
                if (!Element.Effects.Any(eff => eff is ControlSizeEffectiOS))
                    Element.Effects.Add(Effect.Resolve("SloperMobile.ControlSizeEffect"));
            }
        }
	}
}