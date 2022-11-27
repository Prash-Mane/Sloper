using System;
using System.Collections.Generic;
using Facebook.CoreKit;
using Foundation;
using SloperMobile.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(EventLogger))]
namespace SloperMobile.iOS
{
    public class EventLogger : IEventLogger
    {
        public void LogEvent(string name)
        {
            AppEvents.LogEvent(name);
        }

        public void LogEvent(string name, double amount)
        {
            AppEvents.LogEvent(name, amount);
        }

        public void LogEvent(string name, Dictionary<string, string> parameters)
        {
            var nativeDict = new NSMutableDictionary();
            foreach (var kv in parameters)
                nativeDict.Add(new NSString(kv.Key), new NSString(kv.Value));

            AppEvents.LogEvent(name, nativeDict);
        }
    }
}
