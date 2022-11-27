namespace SloperMobile.Common.Constants
{
    public static class AppSetting
    {
		// development
		//public const string Root_Url = "http://sloper.beta.slicksystems.ca:8080/";

		//public const string Root_Url = "http://sloper.clone.slicksystems.ca/";

        // live staging
        //public const string Root_Url = "http://sloperlivestaging.slicksystems.ca/";

        // live
        public const string Root_Url = "http://www.sloperclimbing.com/";

        //debug
        //public const string Root_Url = "http://sloper.rostislav.slicksystems.ca:8080/";

        public static string Base_Url => $"{Root_Url}DesktopModules/";

        public const string API_VERSION = "v190201";
		public const string APP_ID = "2";
		public const string APP_TYPE = "outdoor";
		public const string APP_UOM = "metric";
        public static string APP_DBNAME = "sloper-20190201-" + APP_ID + ".db3";
		public static string FiveStarAscentCheck = "10";

		//Company
		public const string APP_TITLE = "SLOPER CLIMBING";
        public const string APP_COMPANY = "Sloper";
        public const string APP_LABEL_DROID = "Sloper";

        //Facebook - pcmd @ hotmail.com
        //551458735229831 = steve, 263779774184527 = jeff
        //TODO: we need to migrate to a single fb userid between the 2 apps, as if we move to Jeffs
        //      it creates a new FB user in our system, as the FBID returned is new
        public const string SloperClimbingFBId = "551458735229831";

        public const string BullFBId = "217169602233063";//"200134067387271";
        public const string SloperClimbingFBName = "Sloper Climbing";
        public const string BullFBName = "Bull in a China Shop";


        //Google+ - jeff.moore @ sloperclimbing.com google
        public const string GooglePlusWebClientId = "912426591256-mpga19980akkt4o4d87uq616fu1avk7e.apps.googleusercontent.com";
        //public const string GooglePlusAndroidLive = "912426591256-7v2qiaa78l8qna26alvu80scglost6ek.apps.googleusercontent.com";
        //public const string GooglePlusBullDroidDebug = "912426591256-1csnas4euh1qsdtv4g9ees2f39epfg5j.apps.googleusercontent.com";
        public const string GooglePlusBullIOS = "912426591256-7roepvg349rfjrnr7ihkj155ukkfvg61.apps.googleusercontent.com";
        public const string GooglePlusOutdoorIOS = "912426591256-lv6lto9p20ifg12bhfrrsgag5t9g9dop.apps.googleusercontent.com";

        //HockeyApp
        //public const string HockeyAppId_Droid = "c528525a7d654946a5692d82f19a8228";
        //public const string HockeyAppId_iOS = "12da4aa9ca2044228df1cca19fd7675d";

        //Google Map API Key Steve
        public const string GoogleApiKey_Droid = "AIzaSyC45C1WQGYjmePYECElEWl4k-Zr3YDQQdU";
        public const string GoogleApiKey_iOS = "AIzaSyDFYT8I-UVDNHYru4Cvold-bZJE3M9UWas";

        //Guest DNN Login
        public const string Guest_UserId = "bull.guest@sloperclimbing.com";
        public const string Guest_UserPassword = "G'7/[LdYbcC4K^Jr";

		//Syncfusion Key
		public const string SyncfusionKey = "NDM1M0AzMTM2MmUzMjJlMzBqV09mWHc2eHptRTRabzRic082SGc5bDdhUTZ2OXR1QmFzR2lHc0pTc1dVPQ==";


        public const string AppCenterDroid = "7f4a57b5-3455-4e3a-bf9d-0a7d45e7d9e0";
        public const string AppCenteriOS = "51f531c5-ab89-457a-a5b7-31a9edeca878";
	}
}
