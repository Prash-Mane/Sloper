using Acr.UserDialogs;
using Autofac;
using Plugin.Connectivity;
using Plugin.InAppBilling;
using Plugin.Media;
using Prism.Autofac;
using Prism.Ioc;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.Services;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.ViewModel;
using SloperMobile.ViewModel.AscentViewModels;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.ViewModel.MyViewModels;
using SloperMobile.ViewModel.ProfileViewModels;
using SloperMobile.ViewModel.ReportedIssueViewModels;
using SloperMobile.ViewModel.SectorViewModels;
using SloperMobile.ViewModel.SocialModels;
using SloperMobile.ViewModel.SubscriptionViewModels;
using SloperMobile.ViewModel.UserViewModels;
using SloperMobile.Views;
using SloperMobile.Views.AscentPages;
using SloperMobile.Views.CragPages;
using SloperMobile.Views.FlyoutPages;
using SloperMobile.Views.MyPages;
using SloperMobile.Views.ReportedIssuePages;
using SloperMobile.Views.SectorPages;
using SloperMobile.Views.SocialPages;
using SloperMobile.Views.SubscriptionPages;
using SloperMobile.Views.UserPages;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using FFImageLoading;
using System.Net.Http;
using System.Linq;
using SloperMobile.Views.GuideBooks;
using SloperMobile.ViewModel.GuideBookViewModels;
using SloperMobile.ViewModel.WelcomeOverviewViewModels;

using Plugin.Geolocator;
using Rg.Plugins.Popup.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

using SloperMobile.Views.WelcomeOverviewPages;

namespace SloperMobile
{
    public partial class App : PrismApplication
    {
        private IRepository<AppSettingTable> appSettingsRepository;
        private IRepository<SectorTable> sectorRepository;
        private IRepository<AscentTypeTable> ascentTypeRepository;
        private CancellationTokenSource cancellation;
        public static byte[] CroppedImage;
        public static bool IsForeground { get; set; }
        public static int DeviceScreenWidth;
        public App(string version, string bundle)
        {
            InitializeComponent();
            MainPage = new ContentPage();
            Settings.Version = version;
            Settings.AppPackageName = bundle;
            cancellation = new CancellationTokenSource();
        }

        public static bool WasHttpFailed { get; set; } //why do we need this?
        public static bool IsNavigating { get; set; }
        public static bool IsAscentSummaryPage { get; set; }
        public static ICheckForUpdateService CheckForUpdateService { get; private set; }
        public static IExceptionSynchronizationManager ExceptionSyncronizationManager { get; private set; }
        public static IContainerProvider ContainerProvider { get; set; }
        public static INavigationService Navigation { get; set; }

        protected override void OnInitialized()
        {
            OnInitializedAsync();
        }

        protected override void OnStart()
        {
            base.OnStart();

            AppCenter.Start($"ios={AppSetting.AppCenteriOS};android={AppSetting.AppCenterDroid}", typeof(Analytics), typeof(Crashes));

            //Crashes.SendingErrorReport += (sender, e) => { 
            //    Debug.WriteLine("Sending report"); 
            //};
            //Crashes.SentErrorReport += (sender, e) => {
            //    Debug.WriteLine("Sent report");
            //};
            //Crashes.FailedToSendErrorReport += (sender, e) => {
            //    Debug.WriteLine("Failed report");
            //};
            //Crashes.ShouldAwaitUserConfirmation = () => false;
            //Crashes.ShouldProcessErrorReport = (e) => true;
            //Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
        }

        async Task OnInitializedAsync()
        {
            Navigation = NavigationService;
            Settings.MapSelectedCrag = 0;
            IsForeground = true;

            await InitializeApp();
            ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpClient = new HttpClient(new AuthenticatedHttpImageClientHandler())
            });

            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            await VacuumDB();
            // Handle when your app starts
            await SubscribeToCheckForUpdates();
        }

        private async System.Threading.Tasks.Task SubscribeToCheckForUpdates()
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    return;
                }

                var isAppinitialized = await appSettingsRepository.ExecuteScalarAsync<int>("Select IS_INITIALIZED From APP_SETTING");
                if (isAppinitialized <= 0 || string.IsNullOrEmpty(Settings.AccessToken) || Settings.UserID == 0)
                {
                    return;
                }

                var userId = Settings.UserID;
                var userInfoRepository = Container.GetContainer().Resolve<IRepository<UserInfoTable>>();
                var user = await userInfoRepository.GetAsync(userId);
                if (user == null)
                    return;


                await CheckForUpdateService.RunCheckForUpdates();
            }
            catch (Exception exception)
            {
                //need to implement network availability.						   
                //await Navigation.NavigateAsync<NetworkErrorViewModel>();
                return;
            }
        }

        protected override void OnSleep()
        {
            IsForeground = false;
        }

        protected override void OnResume()
        {
            IsForeground = true;

            if (!InAppPurchaseService.IsPurchaseInProgress)
                SubscribeToCheckForUpdates();
        }

        async Task InitializeApp()
        {
            try
            {
                appSettingsRepository = Container.GetContainer().Resolve<IRepository<AppSettingTable>>();
                sectorRepository = Container.GetContainer().Resolve<IRepository<SectorTable>>();
                ascentTypeRepository = Container.GetContainer().Resolve<IRepository<AscentTypeTable>>();
                Container.GetContainer().Resolve<IRepository<TempAscentTable>>();
                Container.GetContainer().Resolve<IRepository<TempIssueTable>>();
                Container.GetContainer().Resolve<IRepository<TempRouteImageTable>>();
                Container.GetContainer().Resolve<IRepository<ExceptionTable>>();
                Container.GetContainer().Resolve<IRepository<UserTrailRecordsTable>>();
                ExceptionSyncronizationManager = Container.GetContainer().Resolve<IExceptionSynchronizationManager>();
                CheckForUpdateService = Container.GetContainer().Resolve<ICheckForUpdateService>();
                Container.GetContainer().Resolve<ISynchronizationManager>();
                Container.GetContainer().Resolve<IDownloadManager>();
                ContainerProvider = Container;
                await GeneralHelper.UpdateCragsDownloadedStateAsync();
                await SetRootPage();
                CrossGeolocator.Current.DesiredAccuracy = 1;
            }
            catch (Exception ex) //Some types may not be registered yet. Wait till RegisterTypes completes
            {
                await InitializeApp();
            }
        }

        async Task SetRootPage()
        {
            var IsAppinitialized = await appSettingsRepository.ExecuteScalarAsync<int>("Select [IS_INITIALIZED] From [APP_SETTING]");
            if (IsAppinitialized > 0)
            {
                await LoadInitializedPage();
            }
            else
            {
                var url = NavigationExtention.CreateNavigationUrl(typeof(MasterNavigationViewModel), typeof(SplashViewModel));
                await NavigationService.NavigateAsync(url);

                var typesCount = await ascentTypeRepository.CountAsync();
                if (typesCount == 0)
                {
                    var ascent_types = new[] {
                            new AscentTypeTable { ascent_type_description = "Onsight" },
                            new AscentTypeTable { ascent_type_description = "Flash" },
                            new AscentTypeTable { ascent_type_description = "Redpoint" },
                            new AscentTypeTable { ascent_type_description = "Repeat" },
                            new AscentTypeTable { ascent_type_description = "One hang" },
                            new AscentTypeTable { ascent_type_description = "Project" } };
                    await ascentTypeRepository.InsertAllAsync(ascent_types);
                }
            }
        }

        public async Task LoadInitializedPage()
        {
            if (!string.IsNullOrEmpty(Settings.AccessToken)
                && !string.IsNullOrEmpty(Settings.RenewalToken)
                && Settings.UserID != default(int))
            {
                var userId = Settings.UserID;
                var userInfoRepository = Container.GetContainer().Resolve<IRepository<UserInfoTable>>();
                var user = await userInfoRepository.GetAsync(userId);

                if (user != null)
                {
                    Container.GetContainer().Resolve<IHttpHelper>().CheckTokenAsync();
                    await LoadLoggedInPage();
                    return;
                }
            }

            var url = NavigationExtention.CreateNavigationUrl(typeof(MasterNavigationViewModel), typeof(UserLoginViewModel));
            await NavigationService.NavigateAsync(url);
        }

        public async Task LoadLoggedInPage()
        {
            var cragRepository = Container.GetContainer().Resolve<IRepository<CragExtended>>();
            var downloadedCrags = await cragRepository.GetAsync(c => c.is_downloaded);
            if (!downloadedCrags.Any())
            {
                var url = NavigationExtention.CreateNavigationUrl(typeof(MasterNavigationViewModel), typeof(OverviewSlidesViewModel));
                await NavigationService.NavigateAsync(url);
            }
            else
                await NavigationService.NavigateFromMenuAsync<HomeViewModel>();

        }

        public static async Task NotificationClickExecute()
        {
            if (PopupNavigation.PopupStack.Any())
                return;

            await Navigation.NavigateFromMenuAsync<ManageDownloadsViewModel>(animated: false);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Registering Repositories
            containerRegistry.RegisterSingleton<IRepository<TempIssueTable>, Repository<TempIssueTable>>();
            containerRegistry.RegisterSingleton<IRepository<TempAscentTable>, Repository<TempAscentTable>>();
            containerRegistry.Register<IRepository<AppSettingTable>, Repository<AppSettingTable>>();
            containerRegistry.Register<IRepository<AreaTable>, Repository<AreaTable>>();
            containerRegistry.Register<IRepository<RouteTable>, Repository<RouteTable>>();
            containerRegistry.Register<IRepository<SectorTable>, Repository<SectorTable>>();
            containerRegistry.Register<IRepository<CragExtended>, Repository<CragExtended>>();
            containerRegistry.Register<IRepository<CragSectorMapTable>, Repository<CragSectorMapTable>>();
            containerRegistry.Register<IRepository<MapTable>, Repository<MapTable>>();
            containerRegistry.Register<IRepository<TopoTable>, Repository<TopoTable>>();
            containerRegistry.Register<IRepository<AscentTypeTable>, Repository<AscentTypeTable>>();
            containerRegistry.Register<IRepository<GradeTable>, Repository<GradeTable>>();
            containerRegistry.Register<IRepository<TechGradeTable>, Repository<TechGradeTable>>();
            containerRegistry.Register<IRepository<BucketTable>, Repository<BucketTable>>();
            containerRegistry.Register<IRepository<CragImageTable>, Repository<CragImageTable>>();
            containerRegistry.Register<IRepository<UserCragTable>, Repository<UserCragTable>>();
            containerRegistry.Register<IRepository<UserInfoTable>, Repository<UserInfoTable>>();
            containerRegistry.Register<IRepository<ProfileFilterTable>, Repository<ProfileFilterTable>>();
            containerRegistry.Register<IRepository<ProfileFilterTypeTable>, Repository<ProfileFilterTypeTable>>();
            containerRegistry.Register<IRepository<IssueCategoryTable>, Repository<IssueCategoryTable>>();
            containerRegistry.Register<IRepository<IssueCategoryIssueTypeLinkTable>, Repository<IssueCategoryIssueTypeLinkTable>>();
            containerRegistry.Register<IRepository<IssueTable>, Repository<IssueTable>>();
            containerRegistry.Register<IRepository<IssueTypeDetailTable>, Repository<IssueTypeDetailTable>>();
            containerRegistry.Register<IRepository<IssueTypeTable>, Repository<IssueTypeTable>>();
            containerRegistry.Register<IRepository<IssueTypeDetailLinkTable>, Repository<IssueTypeDetailLinkTable>>();
            containerRegistry.RegisterSingleton<IRepository<ExceptionTable>, Repository<ExceptionTable>>();
            containerRegistry.Register<IRepository<AppProductTable>, Repository<AppProductTable>>();
            containerRegistry.Register<IRepository<GuideBookTable>, Repository<GuideBookTable>>();
            containerRegistry.Register<IRepository<ReceiptTable>, Repository<ReceiptTable>>();
            containerRegistry.Register<IRepository<ParkingTable>, Repository<ParkingTable>>();
            containerRegistry.Register<IRepository<TrailTable>, Repository<TrailTable>>();
            containerRegistry.Register<IRepository<UserTrailRecordsTable>, Repository<UserTrailRecordsTable>>();
            containerRegistry.Register<IRepository<UserLocationTable>, Repository<UserLocationTable>>();
            containerRegistry.RegisterSingleton<IRepository<TempRouteImageTable>, Repository<TempRouteImageTable>>();

            DependencyService.Register<IPurchaseValidation>();
            containerRegistry.RegisterInstance(CrossConnectivity.Current);
            containerRegistry.RegisterInstance(UserDialogs.Instance);
            containerRegistry.RegisterInstance(CrossMedia.Current);
            containerRegistry.RegisterInstance<IContainerRegistry>(containerRegistry);
            containerRegistry.RegisterInstance(CrossInAppBilling.Current);
            containerRegistry.Register<IDownloadCragService, DownloadCragService>();
            containerRegistry.Register<IHttpHelper, HttpHelper>();
            containerRegistry.Register<ICheckForUpdateService, CheckForUpdateService>();
            containerRegistry.Register<ICameraService, CameraService>();
            containerRegistry.RegisterSingleton<ISynchronizationManager, SynchronizationManager>();
            containerRegistry.RegisterSingleton<IDownloadManager, DownloadManager>();
            containerRegistry.RegisterSingleton<IRemoveCragManager, RemoveCragManager>();
            containerRegistry.RegisterSingleton<IExceptionSynchronizationManager, ExceptionsSynchronizationManager>();
            containerRegistry.Register<IGetImageBytes, ImageBytesService>();
            containerRegistry.Register<IInAppPurchase, InAppPurchaseService>();
            containerRegistry.RegisterSingleton<IPurchasedCheckService, PurchasedCheckService>();


            //Registering ViewModelsForNavigation
            containerRegistry.Register<Object, ProfileRankingViewModel>("ProfileRankingViewModel");
            containerRegistry.RegisterTypeForViewModelNavigation<AscentProcessPage, AscentProcessViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<AscentSummaryPage, AscentSummaryViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<CheckForUpdatesPage, CheckForUpdatesViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<CragDetailsPage, CragDetailsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<CragSectorsPage, CragSectorsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<CragSectorMapDetailPage, CragSectorMapDetailViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<GoogleMapPinPage, GoogleMapPinsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<HomePage, HomeViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MainFlyoutPage, MainMasterDetailViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MasterNavigationPage, MasterNavigationViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MyChangePasswordPage, MyChangePasswordViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MyPreferencesPage, MyPreferencesViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MyProfilePage, MyProfileViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<NewsPage, NewsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ProfilePage, ProfileViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ReportedIssueImagePage, ReportedIssueImageViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ReportedIssueListPage, ReportedIssueListViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ReportedIssueSummaryPage, ReportedIssueSummaryViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ReportIssuePage, ReportIssueViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SectorRoutesPage, SectorRoutesViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SectorTopoDetailsPage, SectorTopoDetailsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SectorToposPage, SectorToposViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SplashPage, SplashViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<TermsPage, TermsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<UserLoginPage, UserLoginViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<UserRegistrationPage, UserRegistrationViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<UserResetPasswordPage, UserResetPasswordViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<UserPasscodePage, UserPasscodeViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<UserChangePasswordPage, UserChangePasswordViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<FacebookSignIn, FacebookSignInViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MicrosoftLiveSignIn, MicrosoftLiveSignInViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<TwitterSignIn, TwitterSignInViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<GooglePlusSignIn, GooglePlusSignInViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<UnlockPage, UnlockViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SubscriptionPage, SubscriptionViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<MemberProfilePage, MemberProfileViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<JournalFeedPage, JournalFeedViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<JournalCommentPage, JournalCommentViewModel>();
            //containerRegistry.RegisterTypeForViewModelNavigation<JournalEngagedUsers, JournalEngagedUsersViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<GuideBookPage, GuideBookViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<GuideBookDetailPage, GuideBookDetailViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ManageDownloadsPage, ManageDownloadsViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<ApproachMapPage, ApproachMapViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<POISelectionPage, POISelectionViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SectorMapListPage, SectorMapListViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<SearchPage, SearchViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<PremiumSubscriptionPage, PremiumSubscriptionViewModel>();
            containerRegistry.RegisterTypeForViewModelNavigation<OverviewSlidesPage, OverviewSlidesViewModel>();
        }

        public static Action<bool> ChangeActivityIndicator { get; set; }
        public static bool AreSectorPages { get; set; }
        public static Action<bool> ChangeMenuPresenter { get; set; }

        private async Task VacuumDB()
        {
            //we can also consider moving this logic to Repository.Remove() if we want to
            var connection = DependencyService.Get<ISQLite>().GetAsyncConnection();

            if (connection != null)
            {
                var freePages = await connection.ExecuteScalarAsync<int>("PRAGMA freelist_count");
                var pageSize = 4096;//await connection.ExecuteScalarAsync<int>("PRAGMA page_size");
                var limit = 1024 * 1024 * 50; //50 MB
                if (freePages * 4096 > limit)
                {
                    UserDialogs.Instance.Toast("Cleaning the Database", TimeSpan.FromDays(1)); 
                    await connection.ExecuteAsync("VACUUM;");
                    UserDialogs.Instance.Toast("Cleaning Database finished");
                }
            }
        }

        void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = GeneralHelper.GetInnerException(e.Exception);
            ExceptionSyncronizationManager.LogException(new ExceptionTable { 
                Page = nameof(App),
                Method = nameof(OnUnobservedTaskException),
                Exception = ex.Message,
                StackTrace = ex.StackTrace
            });
        }
    }
}
