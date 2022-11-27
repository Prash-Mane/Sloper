using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.SocialPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MemberProfilePage : ContentPage
    {
        bool isLoaded;

        public MemberProfilePage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (isLoaded)
                return;
            isLoaded = true;
                    
            if (BindingContext is BaseViewModel vm)
            {
                vm.IsMenuVisible = Navigation.NavigationStack.Count == 1;
                vm.IsBackButtonVisible = !vm.IsMenuVisible;
            }
        }

        //private void DrawLinearGradient(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        //{
        //    e.Surface.Canvas.Clear();

        //    using (var paint = new SKPaint())
        //    {
        //        paint.Shader = SKShader.CreateLinearGradient(
        //                        new SKPoint(e.Info.Rect.MidX, 0),
        //                         new SKPoint(e.Info.Rect.MidX, e.Info.Rect.Height),
        //                        new[] { SKColors.Black.WithAlpha(0), SKColors.Black },
        //                        new[] { 0f, 1f },
        //                        SKShaderTileMode.Clamp);

        //        e.Surface.Canvas.DrawRect(e.Info.Rect, paint);
        //    }
        //}
    }
}
