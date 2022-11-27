using Android.Content;
using Android.Text;
using SloperMobile.Droid.Renderers;
using SloperMobile.UserControls.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Linq;

[assembly: ExportRenderer(typeof(RectangleEntry), typeof(RectangleEntryRenderer))]
namespace SloperMobile.Droid.Renderers
{
    public class RectangleEntryRenderer : EntryRenderer
    {
        public RectangleEntryRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Element == null || Control == null)
                return;
               
            Control.SetBackground(null);

            Control.Gravity = Android.Views.GravityFlags.CenterHorizontal;
            Control.TextChanged += (object sender, Android.Text.TextChangedEventArgs args) => {
                Control.Gravity = args.Text.Any() ? Android.Views.GravityFlags.Left : Android.Views.GravityFlags.CenterHorizontal;
            };
        }
    }
}
