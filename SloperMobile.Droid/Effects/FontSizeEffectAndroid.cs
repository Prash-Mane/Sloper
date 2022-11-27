using System;
using SloperMobile.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(FontSizeEffectAndroid), "FontSizeEffect")]
namespace SloperMobile.Droid.Effects
{
    public class FontSizeEffectAndroid : PlatformEffect
    {
        bool isLoaded;

        protected override async void OnAttached()
        {
            if (isLoaded || !(Element is IFontElement fontElement))
                return;

            isLoaded = true;

            await SizeHelper.AdjustFont(fontElement);
        }

        protected override void OnDetached()
        {
        }
    } 
}