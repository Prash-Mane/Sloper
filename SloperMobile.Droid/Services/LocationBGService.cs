using System;

using Xamarin.Forms;
using SloperMobile.Droid;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Locations;
using Android.Support.V4.App;

//workaround to trigger CrossGeolocator updates while in BG
[assembly: Dependency(typeof(LocationBGService))]
namespace SloperMobile.Droid
{
    [Service]
    public class LocationBGService : Service, ILocationListener, IBgLocationHelper
    {
        LocationManager LocMgr = Android.App.Application.Context.GetSystemService("location") as LocationManager;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            // Check if device is running Android 8.0 or higher and call StartForeground() if so
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notification = new NotificationCompat.Builder(this, Droid.NotificationService.BGServicesChannelId)
                                     .SetContentTitle(Droid.NotificationService.BGServicesTitle)
                                    .SetContentText(Resources.GetString(Resource.String.status_bar_notification_info_overflow))
                                   .SetSmallIcon(Resource.Drawable.sloper_basic_50w)
                                   .SetOngoing(true)
                                   .Build();

                var notificationManager =
                    GetSystemService(NotificationService) as NotificationManager;

                var chan = new NotificationChannel(Droid.NotificationService.BGServicesChannelId, Droid.NotificationService.BGServicesTitle, NotificationImportance.Min);

                notificationManager.CreateNotificationChannel(chan);

                StartForeground(123, notification);
            }

            //we can set different location criteria based on requirements for our app -
            //for example, we might want to preserve power, or get extreme accuracy
            var locationCriteria = new Criteria();

            locationCriteria.Accuracy = Accuracy.Fine;
            locationCriteria.PowerRequirement = Power.NoRequirement;

            // get provider: GPS, Network, etc.
            var locationProvider = LocMgr.GetBestProvider(locationCriteria, true);

            // Get an initial fix on location
            LocMgr.RequestLocationUpdates(locationProvider, 5000, 0, this);

            //return base.OnStartCommand(intent, flags, startId);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            LocMgr.RemoveUpdates(this);
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }

        public void OnLocationChanged(Location location)
        {
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void StartBGUpdates()
        {
            // Check if device is running Android 8.0 or higher and if so, use the newer StartForegroundService() method
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Android.App.Application.Context.StartForegroundService(new Intent(Android.App.Application.Context, typeof(LocationBGService)));
            }
            else // For older versions, use the traditional StartService() method
            {
                Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, typeof(LocationBGService)));
            }
        }

        public void StopBGUpdates()
        {
            Android.App.Application.Context.StopService(new Intent(Android.App.Application.Context, typeof(LocationBGService)));
        }
    }
}