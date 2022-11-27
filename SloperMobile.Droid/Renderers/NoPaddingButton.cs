using System;
using SloperMobile.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;

[assembly: ExportRenderer(typeof(Button), typeof(NoPaddingButton))]
namespace SloperMobile.Droid
{
    public class NoPaddingButton : ButtonRenderer
    {
        public NoPaddingButton(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
                Control.SetPadding(0, 0, 0, 0);
        }

    }
}
