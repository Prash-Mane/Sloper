using System;
using Android.Support.V4.App;
using Android.App;
using Xamarin.Forms;
using Android.OS;
using Android.Content.Res;
using Android.Content;
using Android.Runtime;

[assembly: Dependency(typeof(SloperMobile.Droid.NotificationService))]
namespace SloperMobile.Droid
{
    //[Service]
    public class NotificationService : /*Service,*/ INotificationService
    {
        public const string BGServicesChannelId = "com.sloperclimbing.sloperoutdoor.channel-bg";
        public const string BGServicesTitle = "On-Going Services";

        NotificationCompat.Builder builder = new NotificationCompat.Builder(Android.App.Application.Context, BGServicesChannelId);
        NotificationManager notificationManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.NotificationService) as NotificationManager;
        bool isRunning;

        public NotificationTypes NotificationType { get; set; }

        public NotificationService()
        {
            CreateNotificationChannel();
        }

        public void UpdateProgress(string title, float progress, string contentText = null)
        {
            builder.SetSmallIcon(Resource.Drawable.sloper_basic_50w);
            builder.SetContentTitle(title);
            builder.SetSubText(contentText);
            builder.SetProgress(100, (int)progress, false);
            builder.SetOngoing(true);
            builder.SetShowWhen(false);
            builder.SetVibrate(null);

            if (!isRunning)
            {
                isRunning = true;
                // Check if device is running Android 8.0 or higher and if so, use the newer StartForegroundService() method
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Android.App.Application.Context.StartForegroundService(new Intent(Android.App.Application.Context, GetServiceType()));
                }
                else // For older versions, use the traditional StartService() method
                {
                    Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, GetServiceType()));
                }
            }

            notificationManager.Notify((int)NotificationType, builder.Build());
        }

        public void EndProgress()
        {
            isRunning = false;
            notificationManager.Cancel((int)NotificationType);

            Android.App.Application.Context.StopService(new Intent(Android.App.Application.Context, GetServiceType()));
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(BGServicesChannelId, BGServicesTitle, NotificationImportance.Low);
            //channel.EnableVibration(false);
            //channel.SetVibrationPattern(null);
            channel.LockscreenVisibility = NotificationVisibility.Public;
            notificationManager.CreateNotificationChannel(channel);

            SetNotificationClick();
            
        }
      
        void SetNotificationClick()
        {
            var context = Android.App.Application.Context;

            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);

            intent.PutExtra("Navigate", "MyDownloads");

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            builder.SetContentIntent(pendingIntent);
        }

        Type GetServiceType()
        {
            switch (NotificationType)
            {
                case NotificationTypes.OverlayProgress:
                    return typeof(OverlayProgressService);
                case NotificationTypes.DMProgress:
                    return typeof(DMProgressService);
                default:
                    return null;
            }
        }

        //public override IBinder OnBind(Intent intent)
        //{
        //    return null;
        //}

        //[return: GeneratedEnum]
        //public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        //{
        //    var notification = new NotificationCompat.Builder(this, Droid.NotificationService.BGServicesChannelId).Build();
        //                           //  .SetContentTitle(Droid.NotificationService.BGServicesTitle)
        //                           // .SetContentText(Resources.GetString(Resource.String.status_bar_notification_info_overflow))
        //                           //.SetSmallIcon(Resource.Drawable.sloper_basic_50w)
        //                           //.SetOngoing(true)
        //                           //.Build();

        //    // Check if device is running Android 8.0 or higher and call StartForeground() if so
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //    {
        //        StartForeground((int)1, notification);
        //    }

        //    //return base.OnStartCommand(intent, flags, startId);
        //    return StartCommandResult.Sticky;
        //}
    }

    [Service]
    class OverlayProgressService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var notification = new NotificationCompat.Builder(this, Droid.NotificationService.BGServicesChannelId).Build();
            // Check if device is running Android 8.0 or higher and call StartForeground() if so
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForeground((int)NotificationTypes.OverlayProgress, notification);
            }

            //return base.OnStartCommand(intent, flags, startId);
            return StartCommandResult.Sticky;
        }
    }

    [Service]
    class DMProgressService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var notification = new NotificationCompat.Builder(this, Droid.NotificationService.BGServicesChannelId).Build();
            // Check if device is running Android 8.0 or higher and call StartForeground() if so
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForeground((int)NotificationTypes.DMProgress, notification);
            }

            //return base.OnStartCommand(intent, flags, startId);
            return StartCommandResult.Sticky;
        }
    }
}
