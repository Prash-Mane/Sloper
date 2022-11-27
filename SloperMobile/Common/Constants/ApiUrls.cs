using SloperMobile.Common.Helpers;
using System;
using System.Text;
using Xamarin.Forms;
using SloperMobile.Common.Enumerators;

namespace SloperMobile.Common.Constants
{
    public static class ApiUrls
    {
        //User Registration & Password
        public const string Url_SloperUser_Register = "SloperUser/Register";
        public static string Url_SloperUser_ResetPassword(string email) => $"SloperUser/ResetPassword?email={email}";
        public const string Url_SloperUser_ChangePassword = "SloperUser/ChangePassword";
        public const string Url_SloperUser_UpdatePassword = "SloperUser/UpdatePassword";
        public const string Url_SloperUser_GetUserInfo = "SloperUser/GetUserInfo";
        public const string Url_SloperUser_UpdateUserInfo = "SloperUser/UpdateUserInfo";
        public static string Url_SloperUser_ResetPasswordVerification (string email, string token) => $"SloperUser/ResetPasswordVerification?email={email}&token={token}";

        // Test
        //public const string Url_SloperUser_AssignRole = AppSetting.Base_Url + "SloperPlatform/API/" + AppSetting.API_VERSION + "/SloperUser/AssignUserRole?isapprole={0}&id={1}";
        public static string Url_SloperUser_AssignRole(int? roleId) => $"SloperUser/AssignUserRole?roleid={roleId}";
        public static string Url_SloperUser_RemoveRole(int? roleId) => $"SloperUser/RemoveUserRole?roleid={roleId}";

        //JwtAuth //todo: support those as full, their url prefix should be send to model as parameter
        public const string Url_JwtAuth_login = /*AppSetting.Base_Url + */"JwtAuth/API/mobile/login";
        public const string Url_JwtAuth_extendtoken = /*AppSetting.Base_Url + */"JwtAuth/API/mobile/extendtoken";

        //Splash App Initialization
        public static string Url_M_GetInitialUpdatesByType(string since, string type) 
                    => $"M/GetInitialUpdatesByType?appid={AppSetting.APP_ID}&since={since}&type={type}&initialize=true";

		//Check For Updates
		public static string Url_M_AvailableUpdates(string since)
        {
            var deviceType = Device.RuntimePlatform == Device.Android ? 1 : 2;
            return $"M/AvailableUpdate?appid={AppSetting.APP_ID}&since={since}&devicetype={deviceType}&packagename={Settings.AppPackageName}";
        }
        public static string Url_M_GetUpdatesByType(int cragId, string type, string date = AppConstant.DefaultDate) 
                    => $"M/GetUpdatesByType?appid={AppSetting.APP_ID}&cragid={cragId}&since={date}&type={type}&initialize=true";
        public static string  Url_M_GetCheckForUpdatesByType(string date, string type, bool init = true) 
                    => $"M/GetCheckForUpdatesByType?appid={AppSetting.APP_ID}&since={date}&type={type}&initialize={init}";
        // type : sector or route
        public static string Url_M_GetUpdatesByCragAndType(string cragIds, string date, string type, bool initialize) 
                    => $"M/GetUpdatesByCragAndType?cragids={cragIds}&since={date}&type={type}&initialize={initialize}";

        public static string GetCragById(int id) => $"M/GetCrag?id={id}";
        public static string GetCragImages(int cragId) => $"M/GetCragImages?cragId={cragId}";

        //Check For Updates Consensus
        public const string Url_M_GetConsensusSectors = "M/GetConsensusSectors";
        public const string Url_M_GetConsensusRoutes = "M/GetConsensusRoutes";
        //public static string Url_M_UpdateConsensusData(int routeId) => $"M/UpdateConsensusData?routeId={routeId}"; //not used

        //Check For Updates Images
        public static string Url_TopoImageServer_Get(int sectorId, string since = AppConstant.DefaultDate) => $"TopoImagesServer/Get?sectorId={sectorId}&since={since}";
        public const string Url_M_GetBase64WithRotationImage = "M/GetBase64WithRotationImage";

        //Grades and Technical Grades
        public static string Url_M_GetGradesByAppId = $"M/GetGradesByAppId?appid={AppSetting.APP_ID}";
        public static string Url_M_GetTTechGrades = $"M/GetTTechGrades?appid={AppSetting.APP_ID}";

        //Rankings
        public static string Url_M_GetRankings(string typeId, int cragId, string filter = null, int userId = 0)
        { 
            var url = new StringBuilder($"M/GetRankings?appid={AppSetting.APP_ID}&cragId={cragId}&typeId={typeId}");
            if (!string.IsNullOrEmpty(filter))
                url.Append($"&filter={filter}");
            if (userId != 0)
                url.Append($"&user_id={userId}");
            return url.ToString();
        }
        //public static string Url_M_GetRankingsWithFilter(int cragId, string typeId, string filter) 
        //=> $"M/GetRankings?appid={AppSetting.APP_ID}&cragId={cragId}&typeId={typeId}&filter={filter}";

        //Points
        public static string Url_M_GetPoints(string date = null, int userId = 0)
        {
            var url = new StringBuilder($"M/GetPoints?appid={AppSetting.APP_ID}");
            if (!string.IsNullOrEmpty(date))
                url.Append($"&date_climb={date}");
            if (userId != 0)
                url.Append($"&user_id={userId}");
            return url.ToString();
        }
        //public const string Url_M_GetPointsUser = "M/GetPoints?appid={0}";
        public static string Url_M_GetPointsDaily(int userId = 0) 
                    => userId == 0 ?
                        $"M/GetPointsDaily?appid={AppSetting.APP_ID}"
                        : $"M/GetPointsDaily?appid={AppSetting.APP_ID}&user_id={userId}";

        //Ascents
        public const string Url_Asecnt_CreateAscent = "Ascent/CreateJournalAscent";
        public const string Url_M_GetAscents = "M/GetAscents";
        public const string Url_M_GetAscentDates = "M/GetAscentDates";
        public const string Url_M_GetAscentDetails = "M/GetAscentDetails";
        //public const string Url_M_GetSnapshotImage = "Ascent/GetSnapshotImage?image_id="; //not used
        //public const string Url_M_GetCameraImage = "Ascent/GetCameraImageData?image_id={0}"; //not used
        //public const string Url_M_SaveSnapshotImageWithId = "Ascent/SaveSnapshotImageWithId"; //not used
        //public const string Url_M_SaveRouteImage = "M/SaveRouteImage"; //not used
        //public const string Url_M_UpdateAscentImageData = "Ascent/UpdateAscentImageData"; //not used
        public const string Url_M_SaveImage = "Ascent/SaveImages";

        //Auth
        //public const string Url_M_SaveAuthData = "Authentication/SaveAuthData"; //not used
        //public const string Url_M_GetSavedAuthData = "Authentication/GetAuthDataByUserId?authType={0}"; //not used
        //public const string Url_M_GetAllAuthData = "Authentication/GetAllAuthData"; //not used
        //public const string Url_M_GetDeleteAuthData = "Authentication/DeleteAuth?authType={0}"; //not used
        //public const string Url_M_ShareTwitterImage = "Authentication/ShareTwitterImage"; //not used
        //public const string Url_M_ShareFacebookImage = "Authentication/ShareFacebookImage"; //not used
        //
        public static string Url_Ascent_DeleteAscent(int ascentId) => $"Ascent/DeleteAscent?ascentId={ascentId}";

        //Tick List
        public static string Url_TickList_AddTickList(int routeId) => $"TickList/AddTickList?routeId={routeId}";
        public static string Url_TickList_GetTickList(int userId = 0) 
                    => userId == 0 ? 
                        $"TickList/GetTickList?cragId={AppSetting.APP_ID}" //todo: why does it have app id passed as crag id?
                        : $"TickList/GetTickList?cragId={AppSetting.APP_ID}&user_id={userId}"; 
        public static string Url_TickList_isRoutePresentInTAscent(int routeId) => $"TickList/isRoutePresentInTAscent?routeId={routeId}";
        public static string Url_TickList_isRoutePresentInTickList(int routeId) => $"TickList/isRoutePresentInTickList?routeId={routeId}";
        public static string Url_TickList_DeleteTickList(int routeId) => $"TickList/DeleteTickList?routeId={routeId}";

        //User Crags
        //public const string Url_M_GetUserCragHistory = "M/GetUserCragsHistory"; //not used
        public static string Url_M_AddUserCragHistory(int cragId) => $"M/AddUserCragHistory?crag_id={cragId}";
        public static string Url_M_RemoveUserCragHistory(int selectedCragId) => $"M/RemoveUserCragHistory?crag_id={selectedCragId}";

        //Route Issues
        public static string Url_M_GetIssueData(int type) => $"M/GetIssueData?type={type}";
        public const string Url_M_LogUserIssue = "M/LogUserIssue";
        public static string Url_M_GetRouteIssueData(int cragId, string date) => $"M/GetRouteIssues?crag_id={cragId}&since={date}";

        //Profile Filters
        public const string Url_M_ProfileFilter = "M/GetProfileFilter";
        public const string Url_M_ProfileFilterType = "M/GetProfileFilterType";

        //Jwttoken by social signin                                                                                                                  
        public const string Url_SocialSite_JwtToken = "GetJwtToken/GetSocialJwtToken";
        public const string Url_SocialSite_CheckRegUser = "GetJwtToken/CheckAlreadyRegisterUser"; //not used (has refs in not used VMs)

        public const string Url_M_ExceptionLogger = "M/LogException";

        //purchase api call
        public const string Url_AppPurchase_UpdateUnlockStatus = "AppPurchase/UpdateUnlockStatus";
        public const string Url_AppPurchase_GetCurrentAppProductIds = "AppPurchase/GetProductBySloperId";
        public const string Url_AppPurchase_GetCurrentAppProductsId = "AppPurchase/GetProductsBySloperId";

        //public const string Url_AppPurhcase_VerifyPurchaseReceipt = "AppPurchase/VerifyPurchaseReceipt"; // not used

        public static string Url_AppPurchase_GetSectorRouteCount(int appProductTypeId, int sloperId) 
                    => $"M/GetSectorRouteCount?app_product_type_id={appProductTypeId}&sloperId={sloperId}";

        public static string Url_AppPurchase_GetUnlock = $"M/GetUnlock?appid={AppSetting.APP_ID}";

        //Social Features 
        public static string Url_M_Social_GetMember(int userId) 
                    => $"Social/GetMember?userId={userId}&appId={AppSetting.APP_ID}";
        public const string Url_M_Social_AddFriend = "Social/AddFriend";
        public const string Url_M_Social_RemoveFriend = "Social/RemoveFriend";
        public const string Url_M_Social_Follow = "Social/Follow";
        public const string Url_M_Social_UnFollow = "Social/UnFollow";
        public static string Url_M_Social_GetJournal(int skip, int take) => $"Social/GetJournalItems?skip={skip}&take={take}";
        public static string Url_M_Social_GetJournalEngagement(int journalId, int userId, string flag) => $"Social/GetJournalUsers?journalid={journalId}&userid={userId}&flag={flag}";

        public static string Url_M_Social_LikeAPost(int journalId) => $"Social/LikeAPost?JournalID={journalId}";
        public const string Url_M_Social_PostComment = "Social/PostComment";

        public static string Url_M_Social_CommentList(int journalId, int skip, int take) 
                    => $"Social/CommentList?JournalID={journalId}&skip={skip}&take={take}";
        public static string Url_M_ProfileAvatarThumbnail(int userId, DateTime ts) => $"{AppSetting.Base_Url}SloperPlatform/API/{AppSetting.API_VERSION}/M/GetUserProfileImage?userId={userId}&height=150&width=150&TS={ts.ToString("yyyyMMddHHmmss")}";
        public static string Url_M_ProfileAvatarBig(int userId, DateTime? ts) => $"{AppSetting.Base_Url}SloperPlatform/API/{AppSetting.API_VERSION}/M/GetUserProfileImage?userId={userId}&height=750&width=750&TS={ts?.ToString("yyyyMMddHHmmss")}";

        public static string Url_M_Social_GetFollowers(int userid,int contextuserid) => $"Social/GetFollowers?userid={userid}&contextuserid={contextuserid}";
        public static string Url_M_Social_GetFollowings(int userid, int contextuserid) => $"Social/GetFollowings?userid={userid}&contextuserid={contextuserid}";

        //Top N GuideBooks:
        public static string GetTopNPopularGuidebooks(int topN = 999) => $"M/GetTopNPopularGuidebooks?AppId={AppSetting.APP_ID}&TopN={topN}";
        public static string Url_M_GetTopNNewGuideBooks(int topN = 999) => $"M/GetTopNNewGuideBooks?AppId={AppSetting.APP_ID}&TopN={topN}";

        public const string Url_M_UploadTrailRecord = "M/UploadTrailRecord";
        public static string Url_M_GetTrailRecords(int cragId) => $"M/GetTrailRecords?cragId={cragId}";

        public static string GetWeatherForecast(Metrics unit, float latitude, float longitude) => $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units={unit}&APPID=96bac50ac1d01bc17a7c99a0960b1204";
    }
}