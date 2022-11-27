using Newtonsoft.Json;
using SloperMobile.CustomControls;
using SloperMobile.Droid.Effects;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("SloperMobile")]
[assembly: ExportEffect(typeof(LabelShadowEffect), "LabelShadowEffect")]
namespace SloperMobile.Droid.Effects
{
    public class LabelShadowEffect : PlatformEffect
    {
        protected override async void OnAttached()
        {
            try
            {
                var control = Control as Android.Widget.TextView;
                var effect = (ShadowEffect)Element.Effects.FirstOrDefault(e => e is ShadowEffect);
                if (effect != null)
                {
                    float radius = effect.Radius;
                    float distanceX = effect.DistanceX;
                    float distanceY = effect.DistanceY;
                    Android.Graphics.Color color = effect.Color.ToAndroid();
                    control.SetShadowLayer(radius, distanceX, distanceY, color);
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

        protected override void OnDetached()
        {
        }
    }
}