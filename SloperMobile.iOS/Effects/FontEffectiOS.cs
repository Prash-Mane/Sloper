using SloperMobile.iOS.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XLabs.Platform.Device;
using Xamarin.Forms.Internals;
using System;
using SloperMobile.Common.Helpers;

[assembly: ExportEffect(typeof(FontEffectiOS), "FontSizeEffect")]
namespace SloperMobile.iOS.Effects
{
    public class FontEffectiOS : PlatformEffect
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