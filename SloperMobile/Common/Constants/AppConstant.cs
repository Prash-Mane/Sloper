namespace SloperMobile.Common.Constants
{
    public class AppConstant
    {
		public const string DefaultDate = "20160101000000";
		public const string DateParseString = "yyyyMMdd";
        public const string ExtendedDateParseString = "yyyyMMddHHmmss";

		public const string GradeRequestType = "grade";
		public const string CragRequestType = "crag";
		public const string RouteRequestType = "route";
		public const string SectorRequestType = "sector";
	    public const string MapRequestType = "map";
	    public const string MapRegionRequestType = "mapregion";
        public const string ParkingRequestType = "parking";
        public const string TrailRequestType = "trail";
        public const string AppIdRequestType = "appid";
		public const string SinceRequestType = "since";
		public const string DeviceTypeRequestType = "devicetype";
		public const string PackageNameRequestType = "packagename";

		public const string EmptyJsonArray = "[]";

	    public const string Payload = "apppayload";
		public const string IndoorAppType = "indoor";
		public const string DefaultSloperIndoorPortraiteImageString = "default_sloper_indoor_portrait";
		public const string DefaultSloperOutdoorPortraitImageString = "default_sloper_outdoor_portrait";

		public const string EMPTY_STRING = "";
        public const string NETWORK_FAILURE = "No Network Connection found! Please try again.";
        public const string CANCELLED = "Cancelled";
        public const string LOGIN_FAILURE = "Login Failed! Please try again.";
        public const string CHANGEPASSWORD_FAILURE = "Password Change Request Failed! Please try again.";
        public const string UPDATE_FAILURE_MSG = "Your app data is up to date.";

        public const string SPF_USER_DISPLAYNAME = "displayName";
        public const string SPF_ACCESSTOKEN = "accessToken";
        public const string SPF_RENEWALTOKEN = "renewalToken";

        //MapRoutePage
        public const string RouteType_climbing_Angle = "climbing angle";
        public const string RouteType_hold_Type = "hold type";
        public const string RouteType_Route_Style = "route style";

        //Images For RouteType Angle
        public const string RouteType_Angle_Slab_1 = "angle_1_slab";
        public const string RouteType_Angle_Vertical_2 = "angle_2_vertical";
        public const string RouteType_Angle_Overhanging_4 = "angle_4_overhanging";
        public const string RouteType_Angle_Roof_8 = "angle_8_roof";

        //Images for Hold Type 
        public const string RouteType_Hold_Slopers_1 = "hold_type_1_slopers";
        public const string RouteType_Hold_Crimp_2 = "hold_type_2_crimps";
        public const string RouteType_Hold_Jungs_4 = "hold_type_4_jugs";
        public const string RouteType_Hold_Pockets_8 = "hold_type_8_pockets";

        //Images for Route Style
        public const string RouteType_Route_Style_Technical_1 = "route_style_1_technical";
        public const string RouteType_Route_Style_Sequential_2 = "route_style_2_sequential";
        public const string RouteType_Route_Style_Powerful_4 = "route_style_4_powerful";
        public const string RouteType_Route_Style_Sustained_8 = "route_style_8_sustained";
        public const string RouteType_Route_Style_One_Move_16 = "route_style_16_one_move";

	    public const string AppUnlockedText = "App is unlocked";
	    public const string CragGroupText = "Crags:";
	    public const string GuideBookGroupText = "GuideBooks:";

		/// <summary>
		/// The email address
		/// </summary>
		public const string emailRegex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
       @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        public const float defaultIOSPageHeight = 667f; //based on iPhone 8, 7, 6s, 6
        public const float defaultIOSPageWidth = 375f;

        public const float defaultDroidPageHeight = 640f; //based on Nexus 6P
        public const float defaultDroidPageWidth = 411f;
        public const int GradientHeaderHeight = 140;
    }
}