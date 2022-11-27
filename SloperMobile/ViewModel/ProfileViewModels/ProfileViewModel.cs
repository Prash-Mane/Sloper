using System;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.ViewModel;
using SloperMobile.ViewModel.ProfileViewModels;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;

namespace SloperMobile
{
    public class ProfileViewModel : BaseViewModel
    {
        #region Private Fields

        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private readonly IUserDialogs userDialogs;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<GradeTable> gradeRepository;
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<RouteTable> routeRepository;
        readonly IRepository<UserInfoTable> userInfoRepository;
        readonly IRepository<ProfileFilterTable> profileFilterTable;
        readonly IRepository<ProfileFilterTypeTable> profileFilterTypeTable;
        readonly IRepository<AreaTable> areaRepository;

        private ImageSource backgroundImage;
        private int selectedIndex;
        private int userId;
        private bool isMe;
        private bool isLoaded;
        private DateTime dateForgraph;

        Dictionary<ProfileViews, BaseViewModel> profileVMs = new Dictionary<ProfileViews, BaseViewModel>();

        #endregion

        #region Constructors

        public ProfileViewModel(INavigationService navigationService,
            IRepository<SectorTable> sectorRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<GuideBookTable> guideBookRepository,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper,
            IRepository<TopoTable> topoRepository,
            IRepository<GradeTable> gradeRepository,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<ProfileFilterTable> profileFilterTable,
            IRepository<ProfileFilterTypeTable> profileFilterTypeTable,
            IRepository<AreaTable> areaRepository,
            IRepository<RouteTable> routeRepository)
            : base(navigationService, exceptionManager, httpHelper)
        {
            this.sectorRepository = sectorRepository;
            this.cragRepository = cragRepository;
            this.guideBookRepository = guideBookRepository;
            this.userDialogs = userDialogs;
            this.topoRepository = topoRepository;
            this.sectorRepository = sectorRepository;
            this.gradeRepository = gradeRepository;
            this.appSettingsRepository = appSettingsRepository;
            this.routeRepository = routeRepository;
            this.userDialogs = userDialogs;
            this.userInfoRepository = userInfoRepository;
            this.profileFilterTable = profileFilterTable;
            this.profileFilterTypeTable = profileFilterTypeTable;
            this.areaRepository = areaRepository;

            Offset = Offsets.Footer;
            IsShowFooter = true;

            ProfileRankingViewModel = new ProfileRankingViewModel(
              navigationService, userInfoRepository,
              profileFilterTable, profileFilterTypeTable, areaRepository, exceptionManager, userDialogs, cragRepository, guideBookRepository, httpHelper);

            ProfileSendsViewModel = new ProfileSendsViewModel(navigationService, topoRepository, sectorRepository, gradeRepository, appSettingsRepository, routeRepository, exceptionManager, userDialogs, httpHelper);

            ProfilePointsViewModel = new ProfilePointsViewModel(navigationService, userDialogs, exceptionManager, httpHelper);

            ProfileCalendarViewModel = new ProfileCalendarViewModel(navigationService, exceptionManager, userDialogs, httpHelper);

            ProfileTickListViewModel = new ProfileTickListViewModel(navigationService, sectorRepository, cragRepository, guideBookRepository, exceptionManager, userDialogs, httpHelper);

            profileVMs.Add(ProfileViews.ProfileRanking, ProfileRankingViewModel);
            profileVMs.Add(ProfileViews.ProfileSends, ProfileSendsViewModel);
            profileVMs.Add(ProfileViews.ProfilePoints, ProfilePointsViewModel);
            profileVMs.Add(ProfileViews.ProfileCalendar, ProfileCalendarViewModel);
            profileVMs.Add(ProfileViews.ProfileTickList, ProfileTickListViewModel);
        }

        #endregion

        #region Properties

        public ICommand FilterCommand => new Command(ProfileRankingViewModel.OnFilterIconClick);

        public ICommand SelectedHeaderCommand => new Command<int>(OnSelectedHeaderExecute);

        public ImageSource BackgroundImage
        {
            get => backgroundImage;
            set => SetProperty(ref backgroundImage, value);
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value && value != 0)
                    return;

                profileVMs[(ProfileViews)selectedIndex].OnNavigatedFrom(null);
                SetProperty(ref selectedIndex, value);
                profileVMs[(ProfileViews)selectedIndex].OnNavigatedTo(null);
                SetTabView((ProfileViews)selectedIndex);
                RaisePropertyChanged(nameof(IsFilterVisible));
            }
        }

        public bool IsFilterVisible => ((ProfileViews) selectedIndex == ProfileViews.ProfileRanking) && isMe;

        public ProfileRankingViewModel ProfileRankingViewModel { get; set; }
        public ProfileSendsViewModel ProfileSendsViewModel { get; set; }
        public ProfilePointsViewModel ProfilePointsViewModel { get; set; }
        public ProfileCalendarViewModel ProfileCalendarViewModel { get; set; }
        public ProfileTickListViewModel ProfileTickListViewModel { get; set; }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            profileVMs[(ProfileViews)selectedIndex].OnNavigatedFrom(null);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        #endregion

        #region Public Methods

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;
            isLoaded = true;
           
            var viewType = parameters.GetValue<int>(NavigationParametersConstants.ApplicationActivity);

            if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileId, out int id))
            {
                isMe = id == Settings.UserID;
                userId = id;
                if (isMe)
                    PageHeaderText = Settings.DisplayName;
                else if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileName, out string userName))
                {
                    PageHeaderText = userName;
                }
            }
            else if (userId == 0)
            {
                userId = Settings.UserID;
                isMe = true;
                PageHeaderText = Settings.DisplayName;
            }

            if (parameters.TryGetValue(NavigationParametersConstants.PointsDayParameter, out DateTime dateForgraph))
            {
                this.dateForgraph = dateForgraph;
            }

            BackgroundImage = ImageSource.FromUri(new Uri(ApiUrls.Url_M_ProfileAvatarBig(userId, DateTime.Now)));

            SelectedIndex = viewType;

            await ProfileRankingViewModel.BindFilter();

            IsBackButtonVisible = true;

            GuestHelper.CheckGuest();
        }

        #endregion

        #region Private Methods

        private void SetTabView(ProfileViews view)
        {
            switch (view)
            {
                case ProfileViews.ProfileRanking:
                    PageSubHeaderText = "Rankings";
                    ProfileRankingViewModel.OnPagePrepration(userId);
                    if (!GeneralHelper.IsCragsDownloaded)
                    {
                       // UserDialogs.Instance.Alert("Download a crag to unlock");

                        var sectorPlaceholder = new SectorPlaceholderPopup(this, "Rankings");
                        PopupNavigation.PushAsync(sectorPlaceholder, false);

                        return;
                    }
                    break;
                case ProfileViews.ProfilePoints:
                    PageSubHeaderText = "Points";
                    ProfilePointsViewModel.OnPagePrepration(userId, dateForgraph);
                    break;
                case ProfileViews.ProfileSends:
                    PageSubHeaderText = "Sends";
                    ProfileSendsViewModel.OnPagePrepration(userId);
                    break;
                case ProfileViews.ProfileCalendar:
                    PageSubHeaderText = "Calendar";
                    ProfileCalendarViewModel.OnPagePrepration(userId);
                    break;
                case ProfileViews.ProfileTickList:
                    PageSubHeaderText = "Tick List";
                    ProfileTickListViewModel.OnPagePrepration(userId);
                    break;
                default:
                    PageSubHeaderText = "Rankings";
                    ProfileRankingViewModel.OnPagePrepration(userId);
                    break;
            }
        }

        private void OnSelectedHeaderExecute(int index)
        {
            SelectedIndex = index;
        }


        #endregion
    }
}
