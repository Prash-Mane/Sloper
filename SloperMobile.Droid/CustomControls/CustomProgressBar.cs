using SloperMobile.Droid.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ProgressBar), typeof(CustomProgressBar))]
namespace SloperMobile.Droid.CustomControls
{
    public class CustomProgressBar : ProgressBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ProgressBar> e)
        {
            base.OnElementChanged(e);

            Control.ProgressDrawable.SetColorFilter(Android.Graphics.Color.Rgb(255, 244, 0), Android.Graphics.PorterDuff.Mode.SrcIn);
            Control.ScaleY = 2; //Change the height of progressbar
        }
    }
}