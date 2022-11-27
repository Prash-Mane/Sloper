using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Google.Analytics;
using SloperMobile.Analytics.Interface;
using SloperMobile.iOS.DependancyObjects;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Analytics))]
namespace SloperMobile.iOS.DependancyObjects
{
   public class Analytics : IAnalytics
    {
        private static Gai gaInstance;
        private static ITracker tracker;

        public IAnalytics InitWithId(string analyiticsId)
        {
            gaInstance = Gai.SharedInstance;
            gaInstance.DispatchInterval = 10;
            gaInstance.TrackUncaughtExceptions = true;
            tracker = gaInstance.GetTracker(analyiticsId);

            return this;
        }

        public void TrackScreen(PageName screen)
        {
            tracker.Set(GaiConstants.ScreenName, screen.ToString());
            tracker.Send(DictionaryBuilder.CreateScreenView().Build());
            gaInstance.Dispatch();
        }
    }
}