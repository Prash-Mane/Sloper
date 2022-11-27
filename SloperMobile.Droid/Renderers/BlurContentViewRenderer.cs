using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.EightbitLab.BlurViewBinding;
using SloperMobile.CustomControls;
using SloperMobile.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BlurContentView), typeof(BlurContentViewRenderer))]
namespace SloperMobile.Droid
{
    public class BlurContentViewRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<BlurContentView, FrameLayout>
    {
        public BlurContentViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BlurContentView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                //var context = Context;
                //var activity = context as Activity;
                //var rootView = (ViewGroup)activity.Window.DecorView.FindViewById(Android.Resource.Id.Content);
                //var windowBackground = activity.Window.DecorView.Background;
                //var blurView = new BlurView(context);

                //blurView.SetOverlayColor(Android.Graphics.Color.ParseColor("#99000000"));

                //blurView.SetupWith(rootView)
                //.WindowBackground(windowBackground)
                //.BlurAlgorithm(new RenderScriptBlur(context))
                //.BlurRadius(10f);

                //SetNativeControl(blurView);


            }
            if (Element != null)
                Element.BackgroundColor = Color.FromHex("#E6000000");
        }
    }
}
