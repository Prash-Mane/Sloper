using Xamarin.Forms;
using SloperMobile.DataBase;
using SloperMobile.Model;
using SloperMobile.DataBase.DataTables;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using System;
using System.Collections.Generic;
using SloperMobile.Model.CragModels;

namespace SloperMobile.Common.Constants
{
    public class Cache
    {
        public static FlyoutPage MasterPage { get; set; }
        public static MapListModel SelctedCurrentSector { get; set; }
        public static int SelectedTopoIndex { get; set; }
        public static int SendBackArrowCount { get; set; }

        public static int CurrentScreenHeight;
        public static bool IsGlobalRouteId { get; set; }
        public static string accessToken { get; set; }
        public static bool isModel { get; set; }
        public static string twitterSocialData { get; set; }
        public static GooglePlusUser googlePlusSocialData { get; set; }			 
        public static string profileImage { get; set; }        
        public static bool IsTapOnSectorImage { get; set; }		
	}
}
