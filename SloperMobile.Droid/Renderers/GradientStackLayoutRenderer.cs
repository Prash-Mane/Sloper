using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using SloperMobile.Droid.Renderers;
using SloperMobile.UserControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Linq;

[assembly: ExportRenderer(typeof(GradientStackLayout), typeof(GradientStackLayoutRenderer))]
namespace SloperMobile.Droid.Renderers
{
    public class GradientStackLayoutRenderer : VisualElementRenderer<GradientStackLayout>
    {
        //bool isLoaded;

        public GradientStackLayoutRenderer(Context context) : base(context) { }

        //protected override void OnElementChanged(ElementChangedEventArgs<GradientStackLayout> e)
        //{
        //    base.OnElementChanged(e);

        //    if (Element.HasFadeBackground)
        //    { 
        //        SetGradient(); 
        //    }
        //}

        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);

        //    if (e.PropertyName == GradientStackLayout.HasFadeBackgroundProperty.PropertyName)
        //    {
        //        if (Element.HasFadeBackground)
        //            SetGradient();
        //        else
        //            this.SetBackgroundColor(Color.Transparent.ToAndroid());
        //    }
        //}

        //private void SetGradient()
        //{
        //    var gradientDrawable = new GradientDrawable(
        //            GradientDrawable.Orientation.TopBottom,
        //        new int[] { Color.Blue.ToAndroid(), Color.Green.ToAndroid(), Color.Red.ToAndroid() });

        //    gradientDrawable.SetGradientType(GradientType.LinearGradient);

        //    this.SetBackground(gradientDrawable);
        //}

        protected override void DispatchDraw(global::Android.Graphics.Canvas canvas)
        {
            //if (isLoaded) 
            //{
            //    base.DispatchDraw(canvas);
            //    isLoaded = true;
            //    return;
            //}


            if (!Element.HasFadeBackground || Element.Colors == null || Element.Colors.Count < 2)
                return;

            var colors = Element.Colors.Select(c => (int)c.Color.ToAndroid()).ToArray();
            var positions = Element.Colors.Select(c => c.Position).ToArray();

            #region for Vertical Gradient
            var gradient = new Android.Graphics.LinearGradient(0, 0, 0, Height,
            #endregion

            //#region for Horizontal Gradient
            //var gradient = new Android.Graphics.LinearGradient(0, 0, Width, 0,
            //#endregion
                        colors,
                        positions,
                        Android.Graphics.Shader.TileMode.Clamp);

            var paint = new Android.Graphics.Paint()
            {
                Dither = true,
            };
            paint.SetShader(gradient);
            canvas.DrawPaint(paint);
            SetLayerType(Android.Views.LayerType.Hardware, paint);

            base.DispatchDraw(canvas);
        }
    }
}