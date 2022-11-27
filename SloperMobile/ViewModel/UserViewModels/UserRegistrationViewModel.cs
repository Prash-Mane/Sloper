using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using Prism.Mvvm;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.MasterDetailViewModels;
using Xamarin.Forms;
using System;
using System.Linq;
using SloperMobile.Common.Interfaces;
using SloperMobile.Views.FlyoutPages;

namespace SloperMobile.ViewModel.UserViewModels
{
	public class UserRegistrationViewModel : BaseViewModel
	{
		private readonly IRepository<IssueTable> issueRepository;
		private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<CragExtended> cragRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        IRepository<TrailTable> trailRepository;
        readonly IRepository<GuideBookTable> guidebookRepository;
        private readonly IConnectivity connectivity;
		private readonly IUserDialogs userDialogs;
        private UserInfoTable userInfoObj;

        public UserRegistrationViewModel(
			INavigationService navigationService,
            IRepository<IssueTable> issueRepository,
			IRepository<AppSettingTable> appSettingsRepository,
			IConnectivity connectivity,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<GuideBookTable> guidebookRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper):base(navigationService,exceptionManager,httpHelper)
		{
            this.issueRepository = issueRepository;
            this.cragRepository = cragRepository;
            this.topoRepository = topoRepository;
            //this.routeRepository = routeRepository;
            //this.sectorRepository = sectorRepository;
			this.appSettingsRepository = appSettingsRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
			this.connectivity = connectivity;
			this.userDialogs = userDialogs;
            this.userInfoRepository = userInfoRepository;
            this.guidebookRepository = guidebookRepository;
            RegistrationReq = new RegistrationRequestModel();
			LoginRequestModel = new LoginRequestModel();
			RegistrationCommand = new Command(ExecuteOnRegistration);
			LoginCommand = new Command(async () => await navigationService.NavigateAsync<UserLoginViewModel>());
		}

		private LoginRequestModel _loginRequestModel;
		/// <summary>
		/// /Get or set the login required 
		/// </summary>
		public LoginRequestModel LoginRequestModel
		{
			get { return _loginRequestModel; }
			set { SetProperty(ref _loginRequestModel, value); }
		}
		
		private string confirmPassword;
		/// <summary>
		/// Get or set the confirm Password
		/// </summary>
		public string ConfirmPassword
		{
			get { return confirmPassword; }
			set { SetProperty(ref confirmPassword, value); }
		}

		private RegistrationRequestModel registrationReq;
		/// <summary>
		/// Get or set the Registration Required 
		/// </summary>
		public RegistrationRequestModel RegistrationReq
		{
			get { return registrationReq; }
			set { SetProperty(ref registrationReq, value); }
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

		public Command RegistrationCommand { get; set; }
		public ICommand LoginCommand { get; set; }

        private async void ExecuteOnRegistration()         {
			try
			{
				if (!connectivity.IsConnected)
				{
					//await navigationService.NavigateAsync<NetworkErrorViewModel>();
					await exceptionManager.LogException(new ExceptionTable
					{
						Method = nameof(this.ExecuteOnRegistration),
						Page = this.GetType().Name,
						StackTrace = JsonConvert.SerializeObject(MasterNavigationPage.Instance?.Navigation.NavigationStack),
						Exception = "Network error page"
					});

					return;
				}              var isValidate = await IsRegistrationValidation();             if (!isValidate)
            {                 userDialogs.HideLoading();                 //await userDialogs.AlertAsync(AppConstant.REGISTRATION_FAILURE, "Registration Error", "OK");                 return;             }

            userDialogs.ShowLoading("Registering...");

            RegistrationReq.Email = RegistrationReq.UserName;
            RegistrationReq.DisplayName = $"{RegistrationReq.FirstName} {RegistrationReq.LastName}";
            RegistrationReq.AppID=Convert.ToInt32(AppSetting.APP_ID);
            var regjson = JsonConvert.SerializeObject(RegistrationReq);
            
            var regiserResponse = await httpHelper.PostAsync<RegistrationResponse>(ApiUrls.Url_SloperUser_Register, RegistrationReq, null);
            
            if (!regiserResponse.ValidateResponse() || !regiserResponse.Result.successful)
            {
                userDialogs.HideLoading();
                await userDialogs.AlertAsync(regiserResponse.Result.message, "Registration Error", "OK");
                return;
            }

            LoginRequestModel.u = RegistrationReq.UserName;
            LoginRequestModel.p = RegistrationReq.Password;

            var loginResponse = await httpHelper.PostAsync<LoginResponseModel>(ApiUrls.Url_JwtAuth_login, LoginRequestModel, AppSetting.Base_Url);

            if (!loginResponse.ValidateResponse() 
            || loginResponse.Result.accessToken == null
            || loginResponse.Result.renewalToken == null)
            {
                userDialogs.HideLoading();
                await userDialogs.AlertAsync(AppConstant.LOGIN_FAILURE, "Login Error","OK");
                await navigationService.NavigateAsync<UserLoginViewModel>();
                return;
            }
            else 
            {
                Settings.AccessToken = loginResponse.Result.accessToken;
                Settings.RenewalToken = loginResponse.Result.renewalToken;
                Settings.DisplayName = loginResponse.Result.displayName;
            }                         var climbdaysResponse = await HttpGetClimbdays();
            if (!climbdaysResponse.ValidateResponse())
                return;

            Settings.ClimbingDays = climbdaysResponse.Result[0].climbing_days;
             await userInfoRepository.DeleteAll();
            var userInfoResponse = await HttpGetUserInfo();
            if (userInfoResponse.ValidateResponse()) //userInfoObj != null)
            {
                userInfoObj = userInfoResponse.Result;

                await RemoveDownloadedData();
                
                Settings.UserID = userInfoObj.UserID;
                Settings.AvailableFreeCrags = userInfoObj.NumberOfFreeCrags;
                Settings.FreeCragIds = new List<int>();
                await userInfoRepository.InsertAsync(userInfoObj);
            }
            else { 
                userDialogs.HideLoading();
                await userDialogs.AlertAsync("Registration failed", "Login", "OK");
                return;
            }             await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, HomeViewModel>();             userDialogs.HideLoading();             return;

			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.ExecuteOnRegistration),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(RegistrationReq)
				});
				return;
			}
		}

        private async Task<bool> IsRegistrationValidation()
		{
			if (string.IsNullOrWhiteSpace(RegistrationReq.FirstName))
			{
				await userDialogs.AlertAsync("First Name required, try again.", "Registration Error", "OK");
				return false;
			}
			else if (string.IsNullOrWhiteSpace(RegistrationReq.LastName))
			{
				await userDialogs.AlertAsync("Last Name required, try again.", "Registration Error", "OK");
				return false;
			}
			else if (string.IsNullOrWhiteSpace(RegistrationReq.UserName))
			{
				await userDialogs.AlertAsync("Email Address required, try again.", "Registration Error", "OK");
				return false;
			}
			else if (!GeneralHelper.IsEmailValid(RegistrationReq.UserName.ToLower()))
			{
				await userDialogs.AlertAsync("Please enter valid email address.", "Registration Error", "OK");
				return false;
			}
			else if (string.IsNullOrWhiteSpace(RegistrationReq.Password))
			{
				await userDialogs.AlertAsync("Password required, try again.", "Registration Error", "OK");
				return false;
			}
			else if (RegistrationReq.Password != ConfirmPassword)
			{
				await userDialogs.AlertAsync("Passwords do not match, try again.", "Registration Error", "OK");
				return false;
			}

            if (RegistrationReq.Password.Length < 7) 
            { 
                await userDialogs.AlertAsync("Password should be at least 7 characters long.", "Registration Error", "OK");
                return false;
            }

			return true;
		}

        private async Task<OperationResult<IList<ClimbingDaysModel>>> HttpGetClimbdays() => await httpHelper.GetAsync<IList<ClimbingDaysModel>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppLastUpdateDate, "ascent"));

        private async Task<OperationResult<UserInfoTable>> HttpGetUserInfo() => await httpHelper.GetAsync<UserInfoTable>(ApiUrls.Url_SloperUser_GetUserInfo);

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

        private async Task RemoveDownloadedData()
        {
            await issueRepository.DeleteAll();

            await parkingRepository.DeleteAll();
            await trailRepository.DeleteAll();

            var crags = await cragRepository.GetAsync();
			foreach (var crag in crags)
			{
				crag.Unlocked = false;
			}

			await cragRepository.UpdateAllAsync(crags);
			Settings.ActiveCrag = default(int);	 
            var guidebooks = await guidebookRepository.GetAsync();
            foreach (var guidebook in guidebooks)
                guidebook.Unlocked = false;
            await guidebookRepository.UpdateAllAsync(guidebooks);
            await GeneralHelper.UpdateCragsDownloadedStateAsync();
        }
	}
}
