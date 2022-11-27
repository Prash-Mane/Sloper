using System.Reflection;
using SloperMobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(FlyoutPage), typeof(CustomMasterDetailRenderer))]
namespace SloperMobile.Droid.Renderers
{
    public class CustomMasterDetailRenderer : FlyoutPageRenderer
    {
        public override void AddView(Android.Views.View child)
        {
            child.GetType().GetRuntimeProperty("TopPadding").SetValue(child, 0);
            var padding = child.GetType().GetRuntimeProperty("TopPadding").GetValue(child);
            base.AddView(child);
        }
    }
}