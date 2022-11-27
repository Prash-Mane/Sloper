using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.UserViewModels;
using SloperMobile.Common.Interfaces;

namespace SloperMobile.ViewModel
{    
    public class MicrosoftLiveSignInViewModel : BaseViewModel
    {
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IUserDialogs userDialogs;
        private LoginRequestModel loginReq;
        private UserInfoTable userInfoObj;

        public MicrosoftLiveSignInViewModel(
            INavigationService navigationService,
            IRepository<SectorTable> sectorRepository,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {
            this.sectorRepository = sectorRepository;
            this.appSettingsRepository = appSettingsRepository;
            this.userDialogs = userDialogs;
            this.userInfoRepository = userInfoRepository;
            LoginRequestModel = new LoginRequestModel();
        }

        #region Properties
        private string lastUpdate;
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
        /// <summary>
        /// /Get or set the login required 
        /// </summary>
        public LoginRequestModel LoginRequestModel
        {
            get { return loginReq; }
            set { loginReq = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Methods
        public async void GetJwtToken(string token)
        {
            RegUserData regData = new RegUserData();
            SocialUserData userData = new SocialUserData();
            if (!string.IsNullOrEmpty(token))
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                HttpResponseMessage _response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/");
                string result = await _response.Content.ReadAsStringAsync();
                userData = JsonConvert.DeserializeObject<SocialUserData>(result);
                userData.siteUrl = AppSetting.Base_Url;
                userData.accountName = "MicroSoftLive";

                ////get profile image     
                //HttpClient _client = new HttpClient();
                //HttpResponseMessage _imgresponse = await _client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");                
                //string _result = await _imgresponse.Content.ReadAsStringAsync();
                //var stream = await _imgresponse.Content.ReadAsStreamAsync();                
                //byte[] bytes = new byte[stream.Length];
                //stream.Read(bytes, 0, (int)stream.Length);
                //string base64String = Convert.ToBase64String(bytes);                           

            }
            //check user already used or not 
            regData.email = userData.email;
            regData.account = "MicroSoftLive";

            var checkResponse = await httpHelper.PostAsync<bool>(ApiUrls.Url_SocialSite_CheckRegUser, regData);

            if (checkResponse.ValidateResponse() && checkResponse.Result)
            {
                userDialogs.ShowLoading("Authenticating...");

                var tokenResponse = await httpHelper.PostAsync<IList<LoginResponseModel>>(ApiUrls.Url_SocialSite_JwtToken, userData);
                if (tokenResponse.ValidateResponse())
                {
                    var result = tokenResponse.Result;

                    Settings.AccessToken = result[0].accessToken;
                    Settings.RenewalToken = result[0].renewalToken;
                    Settings.DisplayName = result[0].displayName;
                    GotoHomePage();
                }
                else
                {
                    Cache.accessToken = string.Empty;
                    await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, UserLoginViewModel>();                   
                    userDialogs.HideLoading();
                    return;
                }
            }
            else
            {
                Cache.accessToken = string.Empty;
                await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, UserLoginViewModel>();
                await Application.Current.MainPage.DisplayAlert("Login Error", "An account with the same email already exists", "Ok");
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
                    Settings.UserID = userInfoObj.UserID;
                }
                var sectorsCount = await sectorRepository.CountAsync();
                if (sectorsCount == 0)
                {
                    await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, GoogleMapPinsViewModel>();
                    return;
                }

                await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, HomeViewModel>();
                DisposeObject();
                userDialogs.HideLoading();
                return;
            }

        }

        private async Task<OperationResult<IList<ClimbingDaysModel>>> HttpGetClimbdays() => 
            await httpHelper.GetAsync<IList<ClimbingDaysModel>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppLastUpdateDate, "ascent"));

        private async Task<OperationResult<UserInfoTable>> HttpGetUserInfo() => 
            await httpHelper.GetAsync<UserInfoTable>(ApiUrls.Url_SloperUser_GetUserInfo);

        private void DisposeObject()
        {
            LoginRequestModel = new LoginRequestModel();
        }

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

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            lastUpdate = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");
        }
        #endregion
    }
}
