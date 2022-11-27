using System;
using System.Linq;
using Foundation;
using SloperMobile.iOS.Effects;
using SloperMobile.UserControls.CustomControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Diagnostics;
using System.ComponentModel;

[assembly: ExportEffect(typeof(LabelSizeFontToFit), "LabelSizeFontToFit")]
namespace SloperMobile.iOS.Effects
{
	[Preserve(AllMembers = true)]
	public class LabelSizeFontToFit : PlatformEffect
	{
        UILabel label;
        LabelSizeFontToFitEffect effect;

		protected override void OnAttached()
		{
			label = Control as UILabel;
			if (label == null)
				return;
            
            effect = (LabelSizeFontToFitEffect)Element.Effects.FirstOrDefault(item => item is LabelSizeFontToFitEffect);
			label.AdjustsFontSizeToFitWidth = true;
            label.LineBreakMode = UILineBreakMode.Clip;

            if (!string.IsNullOrEmpty(label.Text))
                label.Lines = Math.Min(label.Text.Split().Count(), effect.Lines);

            Element.PropertyChanged += OnTextChanged;
		}

		protected override void OnDetached()
		{
            Element.PropertyChanged -= OnTextChanged;
		}

        void OnTextChanged(object sender, PropertyChangedEventArgs e) 
        { 
            if (e.PropertyName == "Text" && !string.IsNullOrEmpty(label.Text))
            {
                label.Lines = Math.Min(label.Text.Split().Count(), effect.Lines);
            }
        }
	}
}