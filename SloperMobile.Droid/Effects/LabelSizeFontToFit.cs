using System;
using System.Linq;
using Android.Widget;
using SloperMobile.Droid.Effects;
using SloperMobile.UserControls.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Plugin.DeviceInfo;
using System.ComponentModel;

[assembly: ExportEffect(typeof(LabelSizeFontToFit), "LabelSizeFontToFit")]
namespace SloperMobile.Droid.Effects
{
    public class LabelSizeFontToFit : PlatformEffect
    {
        TextView textView;
        LabelSizeFontToFitEffect effect;

        protected override void OnAttached()
        {
            textView = Control as TextView;
            if (textView == null)
                return;

            effect = (LabelSizeFontToFitEffect)Element.Effects.FirstOrDefault(item => item is LabelSizeFontToFitEffect);

            if (!string.IsNullOrEmpty(textView.Text))
                textView.SetLines(Math.Min(textView.Text.Split().Count(), effect.Lines));
            
            Element.PropertyChanged += OnTextChanged;

            if (CrossDeviceInfo.Current.VersionNumber.Major >= 8)
            {
                textView?.SetAutoSizeTextTypeUniformWithConfiguration(6, (int)(Element as Label).FontSize, 1, 1);
                return;
            }

            textView.LayoutChange += OnLayoutChanged;
        }

        protected override void OnDetached()
        {
            if (textView == null)
                return;

            textView.LayoutChange -= OnLayoutChanged;
            Element.PropertyChanged -= OnTextChanged;
        }


        void OnLayoutChanged(object sender, Android.Views.View.LayoutChangeEventArgs e)
        {
            //todo: adjust to support not fullwidth controls
            var element = Element as Label;
            var lineHeight = 3;
            var charWidth = 0.5;

            if (!string.IsNullOrEmpty(textView.Text))
            {
                var charCount = textView.Text.Length;
                var fontSize = (float)Math.Sqrt(element.Width * element.Height / (charCount * lineHeight * charWidth));
                var maxLength = textView.Text.Split().Max(t => t.Length);
                var coef = 9 / (float)maxLength; //9 is average number of chars that fit in 1 line for fullscreen
                if (coef < 1)
                    fontSize *= coef;

                textView.TextSize = fontSize;
            }
            else
                textView.TextSize = 4f;


        }

        void OnTextChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text" && !string.IsNullOrEmpty(textView.Text))
            {
                textView.SetLines(Math.Min(textView.Text.Split().Count(), effect.Lines));

                if (CrossDeviceInfo.Current.VersionNumber.Major < 8)
                    textView.RequestLayout();
            }
        }
    }
}