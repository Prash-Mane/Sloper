using System;
using System.Collections.Generic;
using Xamarin.Facebook.AppEvents;
using Android.OS;
using Xamarin.Forms;
using SloperMobile.Droid;

[assembly: Dependency(typeof(EventLogger))]
namespace SloperMobile.Droid
{
    public class EventLogger : IEventLogger
    {
        static AppEventsLogger logger;
        static AppEventsLogger Logger { 
            get 
            {
                if (logger != null)
                    return logger;

                logger = AppEventsLogger.NewLogger(Android.App.Application.Context);
                return logger;
            }
        }

        public void LogEvent(string name)
        {
            Logger.LogEvent(name);
        }

        public void LogEvent(string name, double amount)
        { 
            Logger.LogEvent(name, amount);
        }

        public void LogEvent(string name, Dictionary<string, string> parameters)
        {
            var nativeDict = new Bundle();
            foreach (var kv in parameters)
                nativeDict.PutString(kv.Key, kv.Value);

            Logger.LogEvent(name, nativeDict);
        }
    }
}
