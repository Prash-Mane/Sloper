using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.ViewModel.UserViewModels;
using Xamarin.Forms;

namespace SloperMobile.ViewModel
{
    //why do we have it? It has nothing to display. This should be moved to UserLoginVM
    public class FacebookSignInViewModel : BaseViewModel
    {															
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<IssueTable> issueRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<TrailTable> trailRepository;
        readonly IPurchasedCheckService purchasedCheckService;
        private readonly IUserDialogs userDialogs;
        private LoginRequestModel loginReq;
        private UserInfoTable userInfoObj;

        private string lastUpdate;

        public FacebookSignInViewModel(
            INavigationService navigationService,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<TopoTable> topoRepository,            
            IRepository<CragExtended> cragRepository,
            IRepository<IssueTable> issueRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IPurchasedCheckService purchasedCheckService,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {												 
            this.appSettingsRepository = appSettingsRepository;            
            this.userInfoRepository = userInfoRepository;
            this.topoRepository = topoRepository;
            this.cragRepository = cragRepository;
            this.issueRepository = issueRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
            this.purchasedCheckService = purchasedCheckService;
            this.userDialogs = userDialogs;
            LoginRequestModel = new LoginRequestModel();
        }
        #region Properties
        /// <summary>
        /// /Get or set the login required 
        /// </summary>
        public LoginRequestModel LoginRequestModel
        {
            get { return loginReq; }
            set { loginReq = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Returns app's last updated date.
        /// </summary>
        public string AppLastUpdateDate
        {
            get
            {
                if (string.IsNullOrEmpty(lastUpdate))
                {
                    lastUpdate = "20160101000000";
                }
                //if the user has an old version of the app, add HH:mm:ss to the string
                if (lastUpdate.Length == 8)
                {
                    lastUpdate = lastUpdate + "000000";
                }
                return lastUpdate;
            }
        }
        #endregion

        #region Methods
        public async void GetJwtToken(string token)
        {
            SocialUserData userData = new SocialUserData();
            RegUserData regData = new RegUserData();
            FbProfileData fbProfileData = new FbProfileData();
            if (!string.IsNullOrEmpty(token))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://graph.facebook.com");

                    HttpResponseMessage _res = client.GetAsync($"me?fields=id,name,email,first_name,last_name,link,birthday,gender,locale,timezone,updated_time&access_token={Cache.accessToken}").Result;

                    _res.EnsureSuccessStatusCode();
                    string result = _res.Content.ReadAsStringAsync().Result;

                    userData = JsonConvert.DeserializeObject<SocialUserData>(result);
                    userData.siteUrl = AppSetting.Base_Url;
                    userData.accountName = "Facebook";

                    //get profile image
                    HttpResponseMessage _res1 = client.GetAsync($"me?fields=picture.height(720).width(720)&access_token={Cache.accessToken}").Result;

                    _res1.EnsureSuccessStatusCode();
                    string fbresult = _res1.Content.ReadAsStringAsync().Result;
                    fbProfileData = JsonConvert.DeserializeObject<FbProfileData>(fbresult);
                    if (!fbProfileData.picture.data.is_silhouette)
                        userData.picture = fbProfileData.picture.data.url;
                    else
                        userData.picture = string.Empty;
                }
            }

            //workaround, coz our server returns error if gender is null
            if (userData.gender == null)
                userData.gender = "";
                
            var response = await httpHelper.PostAsync<IList<LoginResponseModel>>(ApiUrls.Url_SocialSite_JwtToken, userData);//.FirstOrDefault();

            if (response.ValidateResponse())
            {
                var result = response.Result.FirstOrDefault();

                Settings.AccessToken = result.accessToken;
                Settings.RenewalToken = result.renewalToken;
                Settings.DisplayName = result.displayName;
                GotoHomePage();
                return;
            }
            else
            {
                Cache.accessToken = string.Empty;
				userDialogs.HideLoading();
				await userDialogs.AlertAsync( AppConstant.LOGIN_FAILURE, "Login", "OK");
                await navigationService.ResetNavigation<UserLoginViewModel>();
                userDialogs.HideLoading();
                return;
            }
        }

        public async void GotoHomePage()
        {
            if (Settings.AccessToken != null)
            {
                var climbdays = await HttpGetClimbdays();
                if (!climbdays.ValidateResponse())
                    return;

                Settings.ClimbingDays = Convert.ToInt32(climbdays.Result[0].climbing_days);

                var userInfoResponse = await HttpGetUserInfo();
                if (userInfoResponse.ValidateResponse())
                {
                    userInfoObj = userInfoResponse.Result;
                    if(Settings.UserID != userInfoObj.UserID)
                    {
                        await RemoveDownloadedData();
                    }

                    Settings.UserID = userInfoObj.UserID;
                    await userInfoRepository.DeleteAll();
                    await userInfoRepository.InsertOrReplaceAsync(userInfoObj);
                    await purchasedCheckService.UpdateStateAsync();

                    //Calculating the number of free crags left - max number of free crags minus count of free downloaded
                    Settings.AvailableFreeCrags = userInfoObj.NumberOfFreeCrags - Settings.FreeCragIds.Count;
                }
                else //todo: what should we display here? we should not let in
                    return;

                await ((App)Application.Current).LoadLoggedInPage();
                UserDialogs.Instance.HideLoading();
                return;                                                                
            }
            else
            {
                userDialogs.HideLoading();
            }
        }

        private async Task RemoveDownloadedData()
        {
            await issueRepository.DeleteAll();

            await parkingRepository.DeleteAll();
            await trailRepository.DeleteAll();

            Settings.ActiveCrag = default(int);
            await GeneralHelper.UpdateCragsDownloadedStateAsync();
        }

        private async Task<OperationResult<IList<ClimbingDaysModel>>> HttpGetClimbdays() =>
            await httpHelper.GetAsync<IList<ClimbingDaysModel>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppLastUpdateDate, "ascent"));

        private async Task<OperationResult<UserInfoTable>> HttpGetUserInfo() =>
            await httpHelper.GetAsync<UserInfoTable>(ApiUrls.Url_SloperUser_GetUserInfo);

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (!string.IsNullOrEmpty(Cache.accessToken))
            {
                GetJwtToken(Cache.accessToken);
            }
        }

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            lastUpdate = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");
        }
        #endregion
    }
}
