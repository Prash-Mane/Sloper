using SloperMobile.Common.Constants;
using SloperMobile.Droid.Effects;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content.Res;
using System.Threading.Tasks;
using SloperMobile.Common.Helpers;

[assembly: ExportEffect(typeof(ControlSizeEffectDroid), "ControlSizeEffect")]
namespace SloperMobile.Droid.Effects
{
	public class ControlSizeEffectDroid : PlatformEffect
    {
        bool isLoaded;

        protected override async void OnAttached()
        { 
            if (Element == null || isLoaded || IsAttached)
                return;

            isLoaded = true;

            SizeHelper.AdjustSize(Element);
		}

		protected override void OnDetached()
		{
		}
	} 
}