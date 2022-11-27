using CoreAnimation;
using CoreGraphics;
using SloperMobile.iOS.Renderers;
using SloperMobile.UserControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Linq;
using System.ComponentModel;
using UIKit;
using Foundation;

[assembly: ExportRenderer(typeof(GradientStackLayout), typeof(GradientStackLayoutRenderer))]
namespace SloperMobile.iOS.Renderers
{
    public class GradientStackLayoutRenderer : VisualElementRenderer<GradientStackLayout>
    {
        CAGradientLayer gradientLayer;

        protected override void OnElementChanged(ElementChangedEventArgs<GradientStackLayout> e)
        {
            base.OnElementChanged(e);

            SetGradient();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            if (gradientLayer != null)
                gradientLayer.Frame = rect ;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (gradientLayer != null)
            {
                if (e.PropertyName == GradientStackLayout.HasFadeBackgroundProperty.PropertyName)
                    gradientLayer.Hidden = !Element.HasFadeBackground;
                else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName ||
                         e.PropertyName == VisualElement.HeightProperty.PropertyName)
                    SetNeedsDisplay();
                else if (e.PropertyName == GradientStackLayout.ColorsProperty.PropertyName)
                    SetGradient();
            }
        }

        void SetGradient() 
        {
            if (Element == null)
                return;

            if (gradientLayer != null)
            {
                gradientLayer.RemoveFromSuperLayer();
                gradientLayer.Dispose();
                gradientLayer = null;
            }

            if (Element.Colors?.Any() ?? false)
            {
                var colors = Element.Colors.Select(c => c.Color == Color.Transparent ? UIColor.Clear.CGColor : c.Color.ToCGColor()).ToArray();
                var positions = Element.Colors.Select(c => new NSNumber(c.Position)).ToArray();

                gradientLayer = new CAGradientLayer
                {
                    Colors = colors,
                    Hidden = !Element.HasFadeBackground,
                    Locations = positions,
                    Frame = this.Bounds
                };

                NativeView.Layer.InsertSublayer(gradientLayer, 0);
            }
        }
    }
}
