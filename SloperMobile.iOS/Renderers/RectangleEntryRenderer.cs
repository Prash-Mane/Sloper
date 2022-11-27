using System;
using Foundation;
using SloperMobile.iOS.Renderers;
using SloperMobile.UserControls.CustomControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RectangleEntry), typeof(RectangleEntryRenderer))]
namespace SloperMobile.iOS.Renderers
{

    public class RectangleEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null || !(Element is RectangleEntry element))
                return;

            Control.BorderStyle = UITextBorderStyle.Bezel;
            Control.AttributedPlaceholder = new NSAttributedString(Control.Placeholder, UIFont.SystemFontOfSize((nfloat)element.FontSize), foregroundColor: Element.PlaceholderColor.ToUIColor(), paragraphStyle: new NSMutableParagraphStyle { Alignment = UITextAlignment.Center });
        }
    }
}
