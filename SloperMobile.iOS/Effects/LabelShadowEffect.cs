using CoreGraphics;
using Newtonsoft.Json;
using SloperMobile.CustomControls;
using SloperMobile.iOS;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("SloperMobile")]
[assembly: ExportEffect(typeof(LabelShadowEffect), "LabelShadowEffect")]
namespace SloperMobile.iOS
{
    public class LabelShadowEffect : PlatformEffect
	{
        protected override async void OnAttached()
        {
            try
            {
                var effect = (ShadowEffect)Element.Effects.FirstOrDefault(e => e is ShadowEffect);
                if (effect != null)
                {
                    //Control.Layer.CornerRadius = effect.Radius;
                    Control.Layer.ShadowColor = effect.Color.ToCGColor();
                    Control.Layer.ShadowOffset = new CGSize(effect.DistanceX, effect.DistanceY);
                    Control.Layer.ShadowOpacity = 1.0f;
                    Control.Layer.ShadowRadius = effect.Radius;
                }
            }
            catch (Exception exception)
            {
                await App.ExceptionSyncronizationManager.LogException(new DataBase.DataTables.ExceptionTable
                {
                    Data = JsonConvert.SerializeObject(exception.Data),
                    Exception = exception.Message,
                    Method = nameof(this.OnAttached),
                    StackTrace = exception.StackTrace,
                    Page = nameof(this.Element)
                });
            }
        }

		protected override void OnDetached ()
		{
		}
	}
}
