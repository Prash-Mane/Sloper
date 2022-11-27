using System;
using System.Collections.Generic;
using SkiaSharp;
using SloperMobile.Common.Enumerators;
using Syncfusion.XForms.TabView;
using Xamarin.Forms;
using SelectionChangedEventArgs = Syncfusion.XForms.TabView.SelectionChangedEventArgs;

namespace SloperMobile
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            BindingContextChanged += ProfilePage_BindingContextChanged;

            //need provide better approach
            var coef = SizeHelper.GetResizeCoeficientsAsync().Result;
            var yOffset = SizeHelper.StatusBarHeight + (40 * coef.y); //40 is HeaderUC HeightRequest
            TabView.Margin = new Thickness(10, yOffset, 10, 0);
        }

        void ProfilePage_BindingContextChanged(object sender, EventArgs e)
        {
            var ProfileViewModel = (ProfileViewModel)BindingContext;

            if (ProfileViewModel == null)
                return;

            Ranking.BindingContext = ProfileViewModel.ProfileRankingViewModel;
            Points.BindingContext = ProfileViewModel.ProfilePointsViewModel;
            Sends.BindingContext = ProfileViewModel.ProfileSendsViewModel;
            Calendar.BindingContext = ProfileViewModel.ProfileCalendarViewModel;
            TickList.BindingContext = ProfileViewModel.ProfileTickListViewModel;

        }
        protected override void OnDisappearing()
        {
            BindingContextChanged -= ProfilePage_BindingContextChanged;

            base.OnDisappearing();

        }


        void Handle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
            var view = (SfTabView)sender;

            foreach (var i in view.Items)
            {
                var element = (ProfileHeader)i.HeaderContent;
                element.IsSelected = false;
            }

            var currentHeader = (ProfileHeader)view.Items[e.Index].HeaderContent;
            currentHeader.IsSelected = true;
        }

        void DrawRadialGradientHandle(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            float widthCoff = 1.40f;
            
            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateRadialGradient(
                                new SKPoint(info.Rect.MidX, info.Rect.MidY),
                                (int)(info.Rect.Width/ widthCoff),
                                new SKColor[] { new SKColor(0, 0, 0, 165), SKColors.Black },
                                null,
                                SKShaderTileMode.Clamp);

                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}
