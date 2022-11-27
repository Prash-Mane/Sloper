using Acr.UserDialogs;
using Plugin.Connectivity;
using Prism.Mvvm;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.ViewModel.GuideBookViewModels;
using SloperMobile.ViewModel.MyViewModels;
using SloperMobile.ViewModel.ProfileViewModels;
using SloperMobile.ViewModel.SocialModels;
using SloperMobile.Views.FlyoutPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using SloperMobile.UserControls;
using SloperMobile.ViewModel.UserViewModels;
using System.Linq;
using Prism.Ioc;
using Microsoft.AppCenter.Analytics;
using System.Diagnostics;
using Prism;

namespace SloperMobile.ViewModel
{
    //TODO: Review. Why does base vm have logic for specific pages. Shouldn't that logic be contained in those pages? Like colors for points, calendar, etc
	public class BaseViewModel : BindableBase, INavigationAware, IDestructible
	{
		protected readonly INavigationService navigationService;
		protected readonly IExceptionSynchronizationManager exceptionManager;
        protected readonly IHttpHelper httpHelper;

		private bool isRunningTasks;
        private bool isBackButtonVisible;
        private bool isMenuVisible;
        private Offsets offset = Offsets.None;

        private string pageHeaderText;
		private string pageSubHeaderText;
		private string headerTitleText;
        IEventLogger eventLogger { get => DependencyService.Get<IEventLogger>(); }
        private bool hasFade;
        private int gradientHeaderHeight = AppConstant.GradientHeaderHeight;
        Stopwatch timer;
        bool isActive;
       

		public BaseViewModel(
			INavigationService navigationService,
			IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper = null)
		{
			this.navigationService = navigationService;
            if (exceptionManager == null)
                exceptionManager = (App.Current as App).Container.Resolve<IExceptionSynchronizationManager>();
			else
                this.exceptionManager = exceptionManager;
            
            this.httpHelper = httpHelper;
            System.Diagnostics.Debug.WriteLine(this.GetType());

            var dict = new Dictionary<string, string>() { { "pageName", GetType().Name } };
            eventLogger.LogEvent("PageLoaded", dict);
            Analytics.TrackEvent("PageLoaded", dict);

            GradientHeaderHeight = AppConstant.GradientHeaderHeight;

            CheckAuthState();
        }

		public BaseViewModel(INavigationService navigationService, 
                             IHttpHelper httpHelper = null) : this(navigationService, null, httpHelper)
		{
        }

        public bool IsBackButtonVisible
		{
			get { return isBackButtonVisible; }
			set
			{
				SetProperty(ref isBackButtonVisible, value);
                RaisePropertyChanged(nameof(IsShowHeader));
			}
		}

        public bool IsMenuVisible
        {
            get { return isMenuVisible; }
            set
            {
                SetProperty(ref isMenuVisible, value);
                RaisePropertyChanged(nameof(IsShowHeader));
			}
        }

        public Offsets Offset
        {
            get => offset;
            set => SetProperty(ref offset, value);
        }

        /// <summary>
        /// /Get or set the IsRunningTasks
        /// </summary> //why do we need it? remove
        public bool IsRunningTasks
		{
			get { return isRunningTasks; }
			set
			{
				SetProperty(ref isRunningTasks, value);
			}
		}

        bool isShowFooter;
        public virtual bool IsShowFooter 
        {
            get => isShowFooter;
            set => SetProperty(ref isShowFooter, value); 
        }

        public virtual bool IsShowHeader => IsMenuVisible || IsBackButtonVisible;

		public string PageHeaderText
		{
			get { return pageHeaderText?.ToUpper(); }
			set
			{
				SetProperty(ref pageHeaderText, value);
			}
		}

		public string PageSubHeaderText
		{
			get { return pageSubHeaderText; }
			set
			{
				SetProperty(ref pageSubHeaderText, value);
			}
		}

		public string HeaderTitleText
		{
			get { return headerTitleText.ToUpper(); }
			set
			{
				SetProperty(ref headerTitleText, value);
			}
		}

		public Action DisplayMessage;

        public bool HasFade
        {
            get => hasFade;
            set => SetProperty(ref hasFade, value);
        }

        List<GradientColor> headerColors = new List<GradientColor> { 
            new GradientColor { Color = Color.FromHex("#cc000000"), Position = 0.1f },
            new GradientColor { Color = Color.Transparent, Position = 0.9f }
        };
        public List<GradientColor> HeaderColors
        {
            get => headerColors;
            set => SetProperty(ref headerColors, value);
        }

        public int GradientHeaderHeight
        {
            get => gradientHeaderHeight;
            set => SetProperty(ref gradientHeaderHeight, value);
        }

        public ICommand FooterCommand
		{
            get { return new Command<string>((page) => OnNavigation(page)); }
		}

        protected virtual async void OnNavigation(string param, NavigationParameters parameters = null)
		{
			try
			{
                if (string.IsNullOrEmpty(param))
                {
                    App.IsNavigating = false;
                    return;
                }

                if(App.IsNavigating) { //what's that for??
                    await Task.Delay(1000).ConfigureAwait(false);
                    return;
                }

                App.IsNavigating = true;
				var pageType = (ApplicationActivity) Enum.Parse(typeof(ApplicationActivity), param);
                await PageNavigation(pageType, parameters);
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigation),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(param)
				});
			}
		}

		public ICommand BackCommand
		{
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        await navigationService.GoBackAsync();
                    }
                    catch (Exception ex)
                    {
                        await exceptionManager.LogException(new ExceptionTable
                        {
                            Method = nameof(this.BackCommand),
                            Page = this.GetType().Name,
                            StackTrace = ex.StackTrace,
                            Exception = ex.Message,
                        });
                    }
                });
            }
		}

        public ICommand MenuCommand => new Command(() =>
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Cache.MasterPage.IsPresented = true;
            });
        });

        public ICommand GBsCommand => new Command(OnGBsCommand);

        public Action<string> OnConditionAddView;

		protected async Task PageNavigation(ApplicationActivity page, NavigationParameters parameters = null)
		{
			try
			{
                App.ChangeMenuPresenter(true);

                if (PopupNavigation.PopupStack.Count > 0)
                    PopupNavigation.PopAllAsync();

                if (page == ApplicationActivity.CragSectorsPage
                    && !GeneralHelper.IsCragsDownloaded) 
                {
                    var sectorPlaceholder = new SectorPlaceholderPopup(this, "Sectors");

                    await PopupNavigation.PushAsync(sectorPlaceholder, true);

                    App.IsNavigating = false;
                    return;
                }

                if (!CrossConnectivity.Current.IsConnected && (

                      page == ApplicationActivity.MyProfilePage
					 //|| page == ApplicationActivity.ProfilePage
					 || page == ApplicationActivity.MemberProfilePage
                     || page == ApplicationActivity.JournalFeedPage))
				{																				  

					Device.BeginInvokeOnMainThread(() =>
					{									   
						UserDialogs.Instance.HideLoading();
                        UserDialogs.Instance.AlertAsync(page + " Requires an Internet Connection.", "Network Error", "Continue");
					});
                    //App.ChangeMenuPresenter(true);
                    App.IsNavigating = false;
					return;
				}

				switch (page)
				{
                    //case ApplicationActivity.ProfilePage:
                        //parameters = new NavigationParameters
                        //    {
                        //        { NavigationParametersConstants.ApplicationActivity, ProfileViews.ProfileSends }
                        //    };
                        //await navigationService.NavigateAsync<ProfileViewModel>(parameters);
                        //break;
                    case ApplicationActivity.MyProfilePage:
                        if (MasterNavigationPage.Instance?.Navigation.NavigationStack.Count == 1)
                            await navigationService.NavigateFromMenuAsync<MyProfileViewModel>();
                        else
                            await navigationService.NavigateAsync<MyProfileViewModel>();
                        break;

                    case ApplicationActivity.NewsPage:
						await navigationService.NavigateFromMenuAsync(typeof(NewsViewModel));
						break;

                    case ApplicationActivity.MemberProfilePage:
                        if (parameters == null)
                            parameters = new NavigationParameters();
                        if (parameters.Count == 0)
                        {
                            parameters.Add(NavigationParametersConstants.MemberProfileId, Settings.UserID);
                            parameters.Add(NavigationParametersConstants.MemberProfileName, Settings.DisplayName);
                        }
                        await navigationService.NavigateFromMenuAsync(typeof(MemberProfileViewModel), parameters);
                        break;

					case ApplicationActivity.JournalFeedPage:
                        await navigationService.NavigateFromMenuAsync(typeof(JournalFeedViewModel));
						break;

					case ApplicationActivity.HomePage:
						await navigationService.NavigateFromMenuAsync(typeof(HomeViewModel));	   
						break;

					case ApplicationActivity.CragSectorsPage:
						await navigationService.NavigateFromMenuAsync(typeof(CragSectorsViewModel));
						break;

					case ApplicationActivity.GoogleMapPinsPage:
						await navigationService.NavigateFromMenuAsync(typeof(GoogleMapPinsViewModel));
						break;

                    case ApplicationActivity.GuideBookPage:
                        await navigationService.NavigateFromMenuAsync(typeof(GuideBookViewModel));
                        break;
                }

                App.IsNavigating = false;
            }
			catch (Exception ex)
			{																				
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.PageNavigation),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(page)
				});

				await navigationService.NavigateFromMenuAsync(typeof(HomeViewModel));
                App.IsNavigating = false;
            }
		}

        async void OnGBsCommand(object sender)
        {
            await navigationService.NavigateFromMenuAsync<GuideBookViewModel>();

            if (PopupNavigation.PopupStack.Count > 0)
                PopupNavigation.PopAllAsync();
        }

        async Task CheckAuthState()
        {
            if (string.IsNullOrEmpty(Settings.AccessToken))
            {
                var notLoggedPages = new[] {
                            typeof(UserLoginViewModel),
                            typeof(UserRegistrationViewModel),
                            typeof(FacebookSignInViewModel),
                            typeof(GooglePlusSignInViewModel),
                            typeof(SplashViewModel),
                            typeof(TermsViewModel),
                            typeof(UserResetPasswordViewModel),
                            typeof(UserPasscodeViewModel),
                            typeof(UserChangePasswordViewModel)
                        };
                var currentType = GetType();
                if (!notLoggedPages.Contains(currentType))
                {
                    await Task.Delay(400);
                    navigationService.ResetNavigation<UserLoginViewModel>();
                }
            }
        }

        public virtual void OnNavigatedFrom(NavigationParameters parameters)
        {
            try
            {
                isActive = false;
                if (timer == null)
                    return;
                var dict = new Dictionary<string, string>() {
                    { "Visible Time", GetTimeRange() }
                };
                var vmName = GetType().Name.TruncateVMName();
                Analytics.TrackEvent(vmName, dict);

                try
                {
					//not 100% sure if that's needed
                    if (parameters?.GetNavigationMode() == NavigationMode.Back)
                    {
                        var prevVM = navigationService.GetPreLastVM().TruncateVMName();
                        dict = new Dictionary<string, string> { { "Nav: Loaded From", $"(Back) {vmName}" } }; //can also be renamed to Loaded From?
                        Analytics.TrackEvent(prevVM, dict);
                        dict = new Dictionary<string, string> { { "Nav: Went To", $"(Back) {prevVM}" } };
                        Analytics.TrackEvent(vmName, dict);
                    }
                }
                catch (ArgumentNullException) //is be thrown when NavigationMode is not available
                { }
            }
            catch (Exception ex)
            {
                exceptionManager.LogException(new ExceptionTable
                {
                    Method = $"base.{nameof(this.OnNavigatedFrom)}",
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message
                });
            }
        }

        public virtual void OnNavigatedTo(NavigationParameters parameters)
        {
            isActive = true;
            timer = Stopwatch.StartNew();
        }

        public virtual void OnNavigatingTo(NavigationParameters parameters)
        {
            try
            {
                if (parameters?.GetNavigationMode() == NavigationMode.New)
                {
                    var vmName = GetType().Name.TruncateVMName();
                    var prevVM = navigationService.GetPreLastVM().TruncateVMName();
                    var dict = new Dictionary<string, string> { { "Nav: Loaded From", prevVM } };
                    Analytics.TrackEvent(vmName, dict);
                    dict = new Dictionary<string, string> { { "Nav: Went To", vmName } };
                    Analytics.TrackEvent(prevVM, dict);
                }
            }
            catch (ArgumentNullException) //is be thrown when NavigationMode is not available
            { }
            catch (Exception ex)
            {
                exceptionManager.LogException(new ExceptionTable {
                    Method = $"base.{nameof(this.OnNavigatingTo)}",
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message
                });
            }
        }

        public virtual void Destroy() //for cases when whole navigation stack is reset and NavigatedFrom was not called
        {
            if (timer == null)
                return;

            if (isActive)
            {
                var dict = new Dictionary<string, string>() {
                    { "Visible Time", GetTimeRange() }
                };
                var vmName = GetType().Name.TruncateVMName();
                Analytics.TrackEvent(vmName, dict);
            }
        }

        string GetTimeRange()
        {
            var time = timer.Elapsed;
            if (time < TimeSpan.FromSeconds(2))
                return "<2 sec";
            if (time < TimeSpan.FromSeconds(5))
                return "2-5 sec";
            if (time < TimeSpan.FromSeconds(10))
                return "5-10 sec";
            if (time < TimeSpan.FromSeconds(20))
                return "10-20 sec";
            if (time < TimeSpan.FromSeconds(35))
                return "20-35 sec";
            if (time < TimeSpan.FromMinutes(1))
                return "35 sec - 1 min";
            if (time < TimeSpan.FromMinutes(2))
                return "1-2 min";
            if (time < TimeSpan.FromMinutes(3))
                return "2-3 min";
            if (time < TimeSpan.FromMinutes(5))
                return "3-5 min";
            else
                return ">5 min";
        }
    }
}
