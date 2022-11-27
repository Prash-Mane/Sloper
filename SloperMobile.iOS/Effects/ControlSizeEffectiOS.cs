using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.iOS.Effects;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(ControlSizeEffectiOS), "ControlSizeEffect")]
namespace SloperMobile.iOS.Effects
{
	public class ControlSizeEffectiOS : PlatformEffect
    {
        bool isLoaded;

		protected override void OnAttached()
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