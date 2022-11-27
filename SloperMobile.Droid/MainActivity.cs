using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Util;
using Android.Views;
using Plugin.InAppBilling;
using Plugin.Permissions;
using SegmentedControl.FormsPlugin.Android;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.Droid.Services;
using System;
using System.Reflection;
using Xamarin.Facebook;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Serialization;
using XLabs.Serialization.JsonNET;
using FFImageLoading.Forms;
using SloperMobile.Droid.Renderers;
using FFImageLoading.Forms.Platform;
using SloperMobile.ViewModel.GuideBookViewModels;

namespace SloperMobile.Droid
{
	[Activity(Label = AppSetting.APP_LABEL_DROID,
        Icon = "@mipmap/ic_launcher",
        RoundIcon = "@mipmap/ic_launcher_round",
        Theme = "@style/Theme.Splash",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        private const int RC_SIGN_IN = 9001;

        protected override async void OnCreate(Bundle bundle)
        {
            try
            {
				Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(AppSetting.SyncfusionKey);

				//base.Window.RequestFeature(WindowFeatures.ActionBar);
                // Name of the MainActivity theme you had there before.
                // Or you can use global::Android.Resource.Style.ThemeHoloLight

                base.SetTheme(Resource.Style.MainTheme);  
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;
                base.OnCreate(bundle);

                // To Get the device screen width                      [Added by Ravi 25-09-2018]
                App.DeviceScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);

                // Set status bar to the correct themed color
                // Must be done after calling FormsAppCompatActivity.OnCreate()
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    SetFullScreen();
                }

                Window.DecorView.SetBackgroundColor(Android.Graphics.Color.Black);

                RegisterCachedImageRenderer();
                Rg.Plugins.Popup.Popup.Init(this, bundle);
                global::Xamarin.Forms.Forms.Init(this, bundle); //Moved here By Ravi 16-Aug-2017
                global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, bundle); // added by sandy 30-Nov-17
                SegmentedControlRenderer.Init();

                if (PackageName == "com.sloperclimbing.bullinachinashop")
                {
                    FacebookSdk.ApplicationId = AppSetting.BullFBId;
                    FacebookSdk.ApplicationName = AppSetting.BullFBName;
                }
                else
                {
                    FacebookSdk.ApplicationId = AppSetting.SloperClimbingFBId;
                    FacebookSdk.ApplicationName = AppSetting.SloperClimbingFBName;
                }

                FacebookSdk.SdkInitialize(this.ApplicationContext);
                DependencyService.Register<IFacebookManager, FacebookManager>();
                //Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
                Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

				//old code with permission
				//GoogleSignInService.GoogleApiClient = new GoogleApiClient.Builder(this)
				//                                            .AddConnectionCallbacks(this)
				//                                            .AddOnConnectionFailedListener(this)
				//                                            .AddScope(new Scope(Scopes.Email))
				//                                            .AddScope(new Scope(Scopes.Profile))
				//                                            //.AddScope(new Scope(Scopes.PlusLogin))
				//                                            //.AddScope(new Scope(Scopes.PlusMe))
				//                                            .AddApi(PlusClass.API)
				//                                            .Build();

				//google native sign in 
				GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                                                             .RequestIdToken(AppSetting.GooglePlusWebClientId)
                                                             .RequestEmail()
                                                             .Build();

                GoogleSignInService.GoogleApiClient = new GoogleApiClient.Builder(((MainActivity)Forms.Context).ApplicationContext)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                   // .AddApi(PlusClass.API)
                    .AddScope(new Scope(Scopes.Profile))
                    .Build();

                var cv = typeof(Xamarin.Forms.CarouselView);
                var assembly = Assembly.Load(cv.FullName);
                Context context = this.ApplicationContext;
                var version = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;

                //take screen height -40 for top nav and 40 for bottom nav
                Cache.CurrentScreenHeight =
                    (int)((Resources.DisplayMetrics.HeightPixels) / Resources.DisplayMetrics.Density) - 80;
                var container = new SimpleContainer();

                container.Register<IJsonSerializer, JsonSerializer>();
                container.Register<IDevice>(AndroidDevice.CurrentDevice);
                Resolver.ResetResolver();
                Resolver.SetResolver(container.GetResolver());
                //FFImageLoading.Forms.Platform.CachedImageRenderer.Init(false);
               
                //global::Xamarin.Forms.Forms.Init(this, bundle); //Commented By Ravi 16-Aug-2017
                Xamarin.FormsGoogleMaps.Init(this, bundle); // initialize for Xamarin.Forms.GoogleMaps 
                UserDialogs.Init(this);

                LoadApplication(new App(version, this.BaseContext.PackageName));

                if ((int)Build.VERSION.SdkInt >= 23 && CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted) 
                {
                    int RequestLocationId = 0;

                    string[] PermissionsLocation = {
                        Manifest.Permission.AccessCoarseLocation,
                        Manifest.Permission.AccessFineLocation
                    };

                    RequestPermissions(PermissionsLocation, RequestLocationId);
                }
            }
            catch (Exception extension)
            {
				await App.ExceptionSyncronizationManager?.LogException(new DataBase.DataTables.ExceptionTable
				{
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(bundle),
					Exception = extension.Message,
					Page = this.GetType().Name,
					Method = "OnCreate",
					StackTrace = extension.StackTrace,
				});
			}
        }
       
        public void OnConnected(Bundle connectionHint)
        {                            
        }
              
        public void OnConnectionFailed(ConnectionResult result)
        {           
            if (!result.HasResolution)
            {
                // show the localized error dialog.
                GoogleApiAvailability.Instance.GetErrorDialog(this, result.ErrorCode, 0).Show();
                return;
            }
           
            try
            {
                result.StartResolutionForResult(this, RC_SIGN_IN);
            }
            catch (IntentSender.SendIntentException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + System.Environment.NewLine + e.StackTrace);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            var manager = DependencyService.Get<IFacebookManager>();
            if (manager != null)
            {
                (manager as FacebookManager)._callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
            }

            if (requestCode == RC_SIGN_IN)
            {
                if (resultCode == Result.Ok)
                {
                    GoogleSignInService.GoogleApiClient.Connect();
                }
            }

            if (requestCode == 1)
            {
                GoogleSignInResult result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                GoogleSignInService.googleService.OnAuthCompleted(result);
			}

			InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
		}

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                if(grantResults[0] == 0)
                {
                     GoogleSignInService.GoogleApiClient.Connect();
                }
            }
            catch(Exception ex)
			{
				await App.ExceptionSyncronizationManager.LogException(new DataBase.DataTables.ExceptionTable
				{
					Data =
					$"requestCode = {Newtonsoft.Json.JsonConvert.SerializeObject(requestCode)}, permissions = {Newtonsoft.Json.JsonConvert.SerializeObject(permissions)}, grantResults = {Newtonsoft.Json.JsonConvert.SerializeObject(permissions)}",
					Page = this.GetType().Name,
					Method = "OnRequestPermissionsResult",
					StackTrace = ex.StackTrace,
				});
			}
        }

        public override void OnBackPressed()
        {
            if (App.IsAscentSummaryPage)
            {
                return;
            }

            base.OnBackPressed();
        }

        public void OnConnectionSuspended(int cause)
        {
        }

        private void SetFullScreen ()
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                 SystemUiFlags.LayoutFullscreen
                | SystemUiFlags.LayoutStable
                | SystemUiFlags.Immersive);

            Window.SetStatusBarColor(Android.Graphics.Color.Transparent);

            Window.DecorView.SetFitsSystemWindows(true);
        }

        void RegisterCachedImageRenderer()
        {
            //CachedImage.IsRendererInitialized = true;
            var isRendererInitializedProp = typeof(CachedImage).GetProperty("IsRendererInitialized", BindingFlags.Static | BindingFlags.NonPublic);
            isRendererInitializedProp.SetValue(null, true);

            //Assembly assembly = typeof(Image).Assembly;
            //Type type2 = assembly.GetType("Xamarin.Forms.Internals.Registrar") ?? assembly.GetType("Xamarin.Forms.Registrar");
            //PropertyInfo property = type2.GetProperty("Registered", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            //object value = property.GetValue(type2, null);
            //MethodInfo runtimeMethod = value.GetType().GetRuntimeMethod("Register", new Type[2] {
            //    typeof(Type),
            //    typeof(Type)
            //});
            //MethodInfo methodInfo = runtimeMethod;
            //object obj = value;
            //object[] parameters = new Type[2] {
            //    typeof(CachedImage),
            //    typeof(CachedImageSizeRenderer)
            //};
            //methodInfo.Invoke(obj, parameters);
        }

        protected override async void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            var page = intent.GetStringExtra("Navigate");

            if (page == "MyDownloads")
            {
                await App.NotificationClickExecute();
            }
        }

    }
}