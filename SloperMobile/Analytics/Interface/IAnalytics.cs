using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Analytics.Interface
{
    public enum PageName { Login, Main }

    public interface IAnalytics
    {
        IAnalytics InitWithId(string analyticsId);
        void TrackScreen(PageName pageName);
    }
}
