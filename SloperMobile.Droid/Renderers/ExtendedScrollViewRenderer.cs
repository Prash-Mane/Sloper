using Android.Content;
using Android.Views;
using SloperMobile.CustomControls;
using SloperMobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedScrollView), typeof(ExtendedScrollViewRenderer))]
namespace SloperMobile.Droid.Renderers
{

    public class ExtendedScrollViewRenderer : ScrollViewRenderer
    {
        public ExtendedScrollViewRenderer(Context context) : base(context)
        {
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    Parent.RequestDisallowInterceptTouchEvent(true);
                break;

                case MotionEventActions.Up:
                    Parent.RequestDisallowInterceptTouchEvent(false);
                break;
            }
            return base.OnTouchEvent(ev); ;
        }
    }
}
