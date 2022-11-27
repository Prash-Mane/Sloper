using SloperMobile.CustomControls;
using System;
using System.Linq;
using Xamarin.Forms;

namespace SloperMobile.UserControls.CustomControls
{
	public class LabelWithShadowEffect : Label
	{
        public LabelWithShadowEffect()
		{
			Effects.Add(new ShadowEffect
			{
				Color = Color.FromHex("#191919"),
				DistanceX = Device.RuntimePlatform == Device.Android ? 4 : 1,
				DistanceY = Device.RuntimePlatform == Device.Android ? 4 : 1,
				Radius = Device.RuntimePlatform == Device.Android ? 7f : 1.3f
			});
		}
	}
}