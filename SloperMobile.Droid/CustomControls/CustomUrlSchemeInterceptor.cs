using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using SloperMobile.Droid.Renderers;
using SloperMobile.Common.Constants;

namespace SloperMobile.Droid.CustomControls
{
    [Activity(Label = "CustomUrlSchemeInterceptorActivity", NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //DataSchemes = new[] { AppSetting.GooglePlusAndroidDataSchemes },
    DataPath = "/oauth2redirect")]
    public class CustomUrlSchemeInterceptor : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Convert Android.Net.Url to Uri
            var uri = new Uri(Intent.Data.ToString());

            // Load redirectUrl page            
            //GooglePlusSignInRenderer.googleauth?.OnPageLoaded(uri);

            //closed chrome browser
            //  Finish();            
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}