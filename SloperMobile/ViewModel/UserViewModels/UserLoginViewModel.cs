using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.PurchaseModels;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.Views.FlyoutPages;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.UserViewModels
{
	public class UserLoginViewModel : BaseViewModel
    {														   
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<IssueTable> issueRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<TrailTable> trailRepository;
        readonly IRepository<GuideBookTable> guidebookRepository;
        private readonly IUserDialogs userDialogs;
		private readonly IPurchasedCheckService purchasedCheckService;
		private IFacebookManager facebookManager;
		private UserInfoTable userInfoObj;

		public UserLoginViewModel(
            INavigationService navigationService,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<IssueTable> issueRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IRepository<GuideBookTable> guidebookRepository,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper,
            IPurchasedCheckService purchasedCheckService) : base(navigationService, httpHelper)
        {												
            this.appSettingsRepository = appSettingsRepository;
            this.userInfoRepository = userInfoRepository;
            this.topoRepository = topoRepository;
            this.cragRepository = cragRepository;
            this.issueRepository = issueRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
            this.guidebookRepository = guidebookRepository;
			this.userDialogs = userDialogs;
			this.purchasedCheckService = purchasedCheckService;
            LoginCommand = new Command<string>(ExecuteOnLogin);
			TapFBCommand = new Command(TapOnFBImage);
			TapWLCommand = new Command(TapOnWIImage);
			TapTWCommand = new Command(TapOnTWImage);
			TapGPCommand = new Command(TapOnGOImage);
			LoginRequestModel = new LoginRequestModel();
            facebookManager = DependencyService.Get<IFacebookManager>();
		}

        private LoginRequestModel _loginRequestModel;
        /// <summary>
        /// /Get or set the login required 
        /// </summary>
        public LoginRequestModel LoginRequestModel
        {
            get { return _loginRequestModel; }
            set { _loginRequestModel = value; RaisePropertyChanged(); }
        }

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

        public Command<string> LoginCommand { get; set; }
		public Command TapFBCommand { get; set; }
		public Command TapWLCommand { get; set; }
		public Command TapTWCommand { get; set; }
		public Command TapGPCommand { get; set; }

		public ICommand SignUpCommand
        {
            get
            {
                return new Command(async () => await navigationService.NavigateAsync<UserRegistrationViewModel>(null, null, true));
            }
        }

        public ICommand ForgetPasswordCommand
        {
            get
            {
                return new Command(async () => await navigationService.NavigateAsync<UserResetPasswordViewModel>(null, null, true));
            }
        }

        public ICommand LoginGuest => new Command(() => OnLoginGuestAsync());

		private async void ExecuteOnLogin(string param)
		{
			try
			{
				//if (!CrossConnectivity.Current.IsConnected)
				//{
					//await navigationService.NavigateAsync<NetworkErrorViewModel>();
					//await exceptionManager.LogException(new ExceptionTable
					//{
					//	Method = nameof(this.ExecuteOnLogin),
					//	Page = this.GetType().Name,
					//	StackTrace = JsonConvert.SerializeObject(MasterNavigationPage.Instance?.Navigation.NavigationStack),
					//	Exception = "Network error page"
					//});

					//return;
				//}

                //todo: handle fb login here
                if (param == "FB")
                {

                }

                if (param == "Guest")
                {
                    LoginRequestModel.u = AppSetting.Guest_UserId;
                    LoginRequestModel.p = AppSetting.Guest_UserPassword;
                }

                LoginRequestModel.u = Regex.Replace(LoginRequestModel.u, @"\s+", "");

                if (string.IsNullOrWhiteSpace(LoginRequestModel.u) || string.IsNullOrWhiteSpace(LoginRequestModel.p))
                {
                    await userDialogs.AlertAsync("Enter both Email and Password.", "Login Error", "OK");
                    return;
                }

                if (param != "Guest")
                {
                    //check lowercase username, as they are stored in the database all lowercase.
                    if (!GeneralHelper.IsEmailValid(_loginRequestModel.u.ToLower()))
                    {
                        await userDialogs.AlertAsync("Invalid email address.", "Login Error", "OK");
                        return;
                    }
                }

                userDialogs.ShowLoading("Authenticating...");
                //httpClinetHelper.ChangeTokens(ApiUrls.Url_JwtAuth_login, string.Empty);
                var loginjson = JsonConvert.SerializeObject(LoginRequestModel);
                //var response = await httpClinetHelper.Login<LoginResponseModel>(loginjson);

                var response = await httpHelper.PostAsync<LoginResponseModel>(ApiUrls.Url_JwtAuth_login, LoginRequestModel, AppSetting.Base_Url);

                if (!response.ValidateResponse(false))
                {
                    userDialogs.HideLoading();
                    userDialogs.AlertAsync(AppConstant.LOGIN_FAILURE, "Login", "OK");
                    return;
                }

                var result = response.Result;
                //need?
                if (result.accessToken == null
                || result.renewalToken == null
                || result.accessToken == string.Empty
                || result.renewalToken == string.Empty)
                {
                    userDialogs.HideLoading();
                    await userDialogs.AlertAsync(AppConstant.LOGIN_FAILURE, "Login", "OK");
                    return;
                }

                Settings.AccessToken = result.accessToken;
                Settings.RenewalToken = result.renewalToken;
                Settings.DisplayName = result.displayName;

                var climbdaysResponse = await httpHelper.GetAsync<IList<ClimbingDaysModel>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppLastUpdateDate, "ascent"));
                if (!climbdaysResponse.ValidateResponse())
                    return;

                Settings.ClimbingDays = Convert.ToInt32(climbdaysResponse.Result[0].climbing_days);

                //userInfoObj = await HttpGetUserInfo();

                var userInfoResponse = await httpHelper.GetAsync<UserInfoTable>(ApiUrls.Url_SloperUser_GetUserInfo);

                if (!userInfoResponse.ValidateResponse())
                {
                    userDialogs.HideLoading();
                    await userDialogs.AlertAsync(AppConstant.LOGIN_FAILURE, "Login", "OK");
                    return;
                }

                userInfoObj = userInfoResponse.Result;

                if (Settings.UserID != userInfoObj.UserID)
                {
                    await RemoveDownloadedData();
                }

                await GeneralHelper.UpdateCragsDownloadedStateAsync();
                Settings.UserID = userInfoObj.UserID;

                await userInfoRepository.DeleteAll();
                await userInfoRepository.InsertOrReplaceAsync(userInfoObj);
                await purchasedCheckService.UpdateStateAsync();

                //Calculating the number of free crags left - max number of free crags minus count of free downloaded
                Settings.AvailableFreeCrags = userInfoObj.NumberOfFreeCrags - Settings.FreeCragIds.Count;

                await ((App)Application.Current).LoadLoggedInPage();
            }
            catch (Exception ex)
            {
                //IsRunningTasks = false;
                exceptionManager.LogException(new ExceptionTable { 
                    Method = nameof(ExecuteOnLogin),
                    Page = GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = JsonConvert.SerializeObject(userInfoObj)
                });
                userDialogs.HideLoading();
                await userDialogs.AlertAsync("Incorrect username/password. Please try again.", "Login Failure", "OK");
                return;
            }
            finally
            {
                userDialogs.HideLoading();
            }
		}

		private async void OnGoogleLoginComplete(GooglePlusUser googlePlusUser, string message)
		{
			if (googlePlusUser != null)
			{                
                Settings.GPLogIn = true;
                //why?
				//Cache.accessToken = "any text";
				Cache.googlePlusSocialData = googlePlusUser;
				Goto("GooglePlus");
			}
			else
			{
                userDialogs.HideLoading();
				await Application.Current.MainPage.DisplayAlert("Login Canceled", "", "Ok");
				return;
			}
        }

        async Task OnLoginGuestAsync()
        {
            userDialogs.ShowLoading();
            var guestUser = GuestHelper.GetGuestUser();

            Settings.AccessToken = "Guest";
            Settings.UserID = guestUser.UserID;
            Settings.DisplayName = guestUser.DisplayName;
            Settings.ClimbingDays = 0;

            await RemoveDownloadedData();
            await GeneralHelper.UpdateCragsDownloadedStateAsync();

            await userInfoRepository.DeleteAll();
            await userInfoRepository.InsertOrReplaceAsync(guestUser);
            await RemovePurchased();

            Settings.AvailableFreeCrags = 0;

            ((App)Application.Current).LoadLoggedInPage();
            userDialogs.HideLoading();
        }

        private async void Goto(string pageName)
		{
			switch (pageName)
			{
				case "Facebook":
					await navigationService.NavigateAsync<FacebookSignInViewModel>();
					break;
				case "GooglePlus":
					await navigationService.NavigateAsync<GooglePlusSignInViewModel>();
					break;
				default:
					break;
			}
		}

		private async void TapOnWIImage()
		{
			await navigationService.NavigateAsync<MicrosoftLiveSignInViewModel>();
		}

		private async void TapOnTWImage()
		{
			//Cache.navigationService = navigationService;
			await navigationService.NavigateAsync<TwitterSignInViewModel>();
		}

		private async void TapOnGOImage()
		{
			//Cache.navigationService = navigationService;
			Xamarin.Forms.DependencyService.Get<IGoogleSignIn>().Login(OnGoogleLoginComplete,userDialogs);
		}

		private async void TapOnFBImage()
		{
			//Cache.navigationService = navigationService;
			facebookManager.Login(OnLoginComplete,userDialogs);
		}

		private async void OnLoginComplete(FacebookUser facebookUser, string message)
		{
			if (facebookUser != null)
			{               
                Settings.FBLogIn = true;
				Cache.accessToken = facebookUser.Token;
				Goto("Facebook");
			}
			else
			{
                userDialogs.HideLoading();
				await Application.Current.MainPage.DisplayAlert("Login Error", message, "Ok");
				return;
			}
		}

        private async Task RemoveDownloadedData()
        {
            await issueRepository.DeleteAll();

            await parkingRepository.DeleteAll();
            await trailRepository.DeleteAll();


            Settings.ActiveCrag = default(int);
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            lastUpdate = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");
        }

        async Task RemovePurchased()
        {
            var crags = await cragRepository.GetAsync();
            foreach (var crag in crags)
                crag.Unlocked = false;
            await cragRepository.UpdateAllAsync(crags);

            var guidebooks = await guidebookRepository.GetAsync();
            foreach (var guidebook in guidebooks)
                guidebook.Unlocked = false;
            await guidebookRepository.UpdateAllAsync(guidebooks);
        }
    }
}
