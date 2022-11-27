using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SloperMobile.Common.Extentions;
using Xamarin.Forms;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.UserViewModels;
using SloperMobile.Common.Interfaces;

namespace SloperMobile.ViewModel
{
    public class GooglePlusSignInViewModel : BaseViewModel
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

        public GooglePlusSignInViewModel(
            INavigationService navigationService,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<IssueTable> issueRepository,
            IPurchasedCheckService purchasedCheckService,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
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
        }
        #region Properties
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
        public LoginRequestModel LoginRequestModel
        {
            get { return loginReq; }
            set { loginReq = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Methods
        public async void GetJwtToken()
        {
            RegUserData regData = new RegUserData();
            SocialUserData userData = new SocialUserData();
            if (Cache.googlePlusSocialData != null)
            {
                userData.first_name = Cache.googlePlusSocialData.FirstName;
                userData.last_name = Cache.googlePlusSocialData.LastName;
                userData.displayName = Cache.googlePlusSocialData.DisplayName;
                userData.gender = Cache.googlePlusSocialData.Gender;
                userData.id = Cache.googlePlusSocialData.Id;
                userData.email = Cache.googlePlusSocialData.Email;
                userData.picture = Cache.googlePlusSocialData.Picture;
                if(!string.IsNullOrEmpty(userData.picture)){
                    userData.picture += "?sz=720";                       
                }
            }

            userData.siteUrl = AppSetting.Base_Url;
            userData.accountName = "GooglePlus";

            var responseList = await httpHelper.PostAsync<IList<LoginResponseModel>>(ApiUrls.Url_SocialSite_JwtToken, userData);

            if (responseList.ValidateResponse())
            {
                var result = responseList.Result.FirstOrDefault();

                if (!string.IsNullOrEmpty(result.accessToken) && !string.IsNullOrEmpty(result.renewalToken))
                {
                    Settings.AccessToken = result.accessToken;
                    Settings.RenewalToken = result.renewalToken;
                    Settings.DisplayName = result.displayName;

                    GotoHomePage();
                    return;
                }
            }
            else
            {
                Cache.accessToken = string.Empty;
				userDialogs.HideLoading();
				await userDialogs.AlertAsync(AppConstant.LOGIN_FAILURE, "Login", "OK");
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
                    if (Settings.UserID != userInfoObj.UserID)
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

        private async Task<OperationResult<IList<ClimbingDaysModel>>> HttpGetClimbdays()
        {
            //httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_M_GetInitialUpdatesByType, Common.Constants.AppSetting.APP_ID, AppLastUpdateDate, "ascent", true), Settings.AccessToken);
            var area_response = await httpHelper.GetAsync<IList<ClimbingDaysModel>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppLastUpdateDate, "ascent"));
            return area_response;
        }

        private async Task<OperationResult<UserInfoTable>> HttpGetUserInfo()
        {
            // httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_GetUserInfo, Settings.AccessToken);
            var response = await httpHelper.GetAsync<UserInfoTable>(ApiUrls.Url_SloperUser_GetUserInfo);
            return response;
        }

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
            //if (!string.IsNullOrEmpty(Cache.accessToken))
            //{
                GetJwtToken();
            //}
        }

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            lastUpdate = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");
        }
        #endregion
    }
}
