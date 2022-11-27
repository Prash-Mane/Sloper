using Facebook.CoreKit;
using FFImageLoading.Forms.Touch;
using Foundation;
using Google.SignIn;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.iOS.Services;
using Syncfusion.ListView.XForms.iOS;
using Syncfusion.SfCalendar.XForms.iOS;
using Syncfusion.SfChart.XForms.iOS.Renderers;
using Syncfusion.SfGauge.XForms.iOS;
using Syncfusion.SfRating.XForms.iOS;
using System;
using System.Reflection;
using UIKit;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Serialization;
using XLabs.Serialization.JsonNET;
//using MonoTouch.Fabric;
using Syncfusion.SfPullToRefresh.XForms.iOS;
using SegmentedControl.FormsPlugin.iOS;
using CoreLocation;

using Syncfusion.XForms.iOS.TabView;

namespace SloperMobile.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        //iOSCheckUpdatesTaskService updateRunningTask;
        public override UIWindow Window
        {
            get;
            set;
        }

        CLLocationManager locManager;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            try
            {
				Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(AppSetting.SyncfusionKey);

                InitComponents();

                SetDependencies();

                //---------- status bar customization
                var statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
				var bundle = NSBundle.MainBundle.BundleIdentifier;

                statusBar.BackgroundColor = UIColor.Clear;
                SizeHelper.StatusBarHeight = Math.Min((float)statusBar.Bounds.Height, 28); //for iPhone X height is 44, but it doesn't look good, has much free space. So, we lower it to 28 by hardcode

                //-----------

                Cache.CurrentScreenHeight = (int)((int)(UIScreen.MainScreen.Bounds.Height * (int)UIScreen.MainScreen.Scale) * 2);

                var version = NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleShortVersionString")].ToString();

                LoadApplication(new App(version, bundle));

                // To Get the device screen width                      [Added by Ravi 25-09-2018]
                App.DeviceScreenWidth = (int)UIScreen.MainScreen.Bounds.Width;
               
                if (NSBundle.MainBundle.BundleIdentifier == "com.sloperclimbing.bullinachinashop")
                {
                    //fb
                    Settings.AppID = AppSetting.BullFBId;
                    Settings.DisplayName = AppSetting.BullFBName;
                    //google
                    SignIn.SharedInstance.ClientID = AppSetting.GooglePlusBullIOS;
                }
                else { 
                    Settings.AppID = AppSetting.SloperClimbingFBId;
                    Settings.DisplayName = AppSetting.SloperClimbingFBName;
                    SignIn.SharedInstance.ClientID = AppSetting.GooglePlusOutdoorIOS;
                }

                AppEvents.ActivateApp();

                EnableLocationServices();

                return base.FinishedLaunching(app, options);
            }
            catch (Exception ex)
            {
                App.ExceptionSyncronizationManager.LogException(new DataBase.DataTables.ExceptionTable
                {
                    Data = $"options = {Newtonsoft.Json.JsonConvert.SerializeObject(options)}",
                    Page = this.GetType().Name,
                    Method = "FinishedLaunching",
                    StackTrace = ex.StackTrace,
                });
            }

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            // Convert iOS NSUrl to C#/netxf/BCL System.Uri - common API
            var uri_netfx = new Uri(url.AbsoluteString);            

            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }
        // For iOS 9 or newer
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var openUrlOptions = new UIApplicationOpenUrlOptions(options);
            if(url.AbsoluteString.Contains("fb"))
                return ApplicationDelegate.SharedInstance.OpenUrl(app, url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
            else
            return SignIn.SharedInstance.HandleUrl(url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
        }

        private void InitComponents()
        {
            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Auth.Presenters.XamarinIOS.AuthenticationConfiguration.Init(); // added by sandy 30-Nov-17
            global::Xamarin.Forms.Forms.Init();
            Xamarin.FormsGoogleMaps.Init(AppSetting.GoogleApiKey_iOS); // initialize for Xamarin.Forms.GoogleMaps
            CachedImageRenderer.Init();
            SegmentedControlRenderer.Init();

            // --- SF controls init ---
            SfListViewRenderer.Init();
            SfPullToRefreshRenderer.Init();
            SfTabViewRenderer.Init();

            new SfCalendarRenderer();
            new SfChartRenderer();
            new SfGaugeRenderer();
            new SfRatingRenderer();
            // ------------------------
        }

        private void SetDependencies()
        {
            var container = new SimpleContainer();
            var cv = typeof(Xamarin.Forms.CarouselView);
            var assembly = Assembly.Load(cv.FullName);

            //native facebook initialization
            DependencyService.Register<IFacebookManager, FacebookManager>();

            //native google initialization
            DependencyService.Register<IGoogleSignIn, GoogleSignInService>();

            container.Register<IJsonSerializer, JsonSerializer>();
            container.Register<IDevice>(AppleDevice.CurrentDevice);
           
            Resolver.SetResolver(container.GetResolver());
        }

        void EnableLocationServices() {
            if (CLLocationManager.Status != CLAuthorizationStatus.AuthorizedAlways) {
                locManager = new CLLocationManager();
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    locManager.RequestAlwaysAuthorization();
                }

                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    locManager.AllowsBackgroundLocationUpdates = true;
                }
            }
        }
    }
}
