using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.GuideBookModels;
using SloperMobile.Model.ResponseModels;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.UserControls.ReportIssue;
using SloperMobile.ViewModel.AscentViewModels;
using SloperMobile.ViewModel.ReportedIssueViewModels;
using SloperMobile.ViewModel.SubscriptionViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using XLabs.Platform.Device;

namespace SloperMobile.ViewModel.SectorViewModels
{
    public class SectorTopoDetailsViewModel : BaseViewModel
    {
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<IssueTable> issueRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<CragImageTable> cragImageRepository;
        private readonly IRepository<CragExtended> cragExtendedRepository;
        private readonly IRepository<AppProductTable> appProductRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private readonly IUserDialogs userDialogs;

        private ImageSource currentImage;
        private ImageSource imageBackgroundSource;
        private bool isImageBackgroundVisible;
        private TopoImageResponseModel topoImg = null;
        private int routeId;
        private CragImageTable cragImage;
        private bool isUnlocked;
        private bool isNavigatedFromSectorImage;
        public string singleRouteImageBytes;
        private bool isNoSingleRouteDrawn = false;
        private bool singleRoute;
        private long guideBookId;
        private bool isAndroidScrollVisible;
        private bool isIOSScrollVisible;
        private bool isUnlockVisible = true;
        private ObservableCollection<ImageSource> topoDetailIcons_54;
        private ObservableCollection<ImageSource> topoDetailIcons_20;
        GuideBookTable guideBook;

        public Action InvalidateSuface { get; set; }
        public Command TickListCommand { get; set; }
        public Command<string> SendCommand { get; set; }
        public Command HideRoutePopupLgCommand { get; set; }
        public Command ShowRoutePopupLgCommand { get; set; }
        public Command HideSmallPopupCommand { get; set; }
        public Command IssueCommand { get; set; }
        public Command OnWarningCommand { get; set; }
        public ICommand NavigateToUnlockPageCommand { get; set; }
        public Command TermsCommand { get; set; }

        public SectorTopoDetailsViewModel(
            INavigationService navigationService,
            IRepository<TopoTable> topoRepository,
            IRepository<SectorTable> sectorRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<CragImageTable> cragImageRepository,
            IRepository<IssueTable> issueRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<CragExtended> cragExtendedRepository,
            IRepository<AppProductTable> appProductRepository,
            IRepository<GuideBookTable> guideBookRepository,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper)
            : base(navigationService, exceptionManager, httpHelper)
        {
            this.topoRepository = topoRepository;
            this.sectorRepository = sectorRepository;
            this.routeRepository = routeRepository;
            this.cragImageRepository = cragImageRepository;
            this.issueRepository = issueRepository;
            this.userDialogs = userDialogs;
            this.userInfoRepository = userInfoRepository;
            this.cragExtendedRepository = cragExtendedRepository;
            this.appProductRepository = appProductRepository;
            this.guideBookRepository = guideBookRepository;
            TickListCommand = new Command(ExecuteOnTickList);
            SendCommand = new Command<string>(ExecuteOnSends);
            HideRoutePopupLgCommand = new Command(ExecuteOnHideRoutePopupLgCommand);
            ShowRoutePopupLgCommand = new Command(ExecuteOnShowRoutePopupLgCommand);
            HideSmallPopupCommand = new Command(ExecuteOnHideSmallPopupCommand);
            IssueCommand = new Command(ExecuteOnIssueCommand);
            OnWarningCommand = new Command(ExecuteOnWarning);
            NavigateToUnlockPageCommand = new Command(NavigateToUnlockPage);
            TermsCommand = new Command(OnTermsPressed);
            IsAndroidScrollVisible = true;
            IsIOSScrollVisible = true;

            IsBackButtonVisible = true;
            Offset = Common.Enumerators.Offsets.Header;
        }

        public bool IsAndroidScrollVisible
        {
            get
            {
                return isAndroidScrollVisible;
            }
            set
            {
                SetProperty(ref isAndroidScrollVisible, value);
            }
        }

        public bool IsNavigatedFromSectorImage
        {
            get
            {
                return isNavigatedFromSectorImage;
            }
            set
            {
                SetProperty(ref isNavigatedFromSectorImage, value);
            }
        }

        public bool IsIOSScrollVisible
        {
            get
            {
                return isIOSScrollVisible;
            }
            set
            {
                SetProperty(ref isIOSScrollVisible, value);
            }
        }

        public bool IsImageBackgroundVisible
        {
            get
            {
                return isImageBackgroundVisible;
            }
            set { SetProperty(ref isImageBackgroundVisible, value); }
        }

        public ImageSource ImageBackgroundSource
        {
            get
            {
                return imageBackgroundSource;
            }
            set { SetProperty(ref imageBackgroundSource, value); }
        }

        public bool IsNavigatingToTopos
        {
            get { return isNavigatingToTopos; }
            set
            {
                isNavigatingToTopos = value;
                RaisePropertyChanged();
            }
        }

        public ImageSource CurrentSectorImage
        {
            get { return currentImage; }
            set { currentImage = value; RaisePropertyChanged(); }
        }

        private bool displayRoutePopupSm;
        public bool DisplayRoutePopupSm
        {
            get { return displayRoutePopupSm; }
            set { SetProperty(ref displayRoutePopupSm, value); }
        }

        private bool displayRoutePopupLg;
        public bool DisplayRoutePopupLg
        {
            get { return displayRoutePopupLg; }
            set { displayRoutePopupLg = value; RaisePropertyChanged(); }
        }

        bool hideSwipeUp = true;
        public bool IsHideSwipeUp
        {
            get { return hideSwipeUp; }
            set { hideSwipeUp = value; RaisePropertyChanged(); }
        }

        private string _sectorName;
        public string SectorName
        {
            get { return _sectorName; }
            set { _sectorName = value; RaisePropertyChanged(); }
        }

        private int cragName;
        public int CragName
        {
            get { return cragName; }
            set { cragName = value; RaisePropertyChanged(); }
        }

        private string stateName;
        public string StateName
        {
            get { return stateName; }
            set { stateName = value; RaisePropertyChanged(); }
        }

        private string routeName = "";
        public string RouteName
        {
            get { return routeName; }
            set { routeName = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(ShowRouteName)); }
        }

        public bool ShowRouteName
        {
            get => !string.IsNullOrEmpty(RouteName);
        }

        private string techGrade;
        public string TechGrade
        {
            get { return techGrade; }
            set { techGrade = value; RaisePropertyChanged(); }
        }

        private string routeInfo = "";
        public string RouteInfo
        {
            get { return routeInfo; }
            set { routeInfo = value; RaisePropertyChanged(); }
        }

        private double rating;
        public double Rating
        {
            get { return rating; }
            set { rating = value; RaisePropertyChanged("Rating"); }
        }

        private string routeTypeTop;

        public string RouteTypeTop
        {
            get { return routeTypeTop; }
            set { routeTypeTop = value; RaisePropertyChanged(); }
        }

        private int? curr_routeid;
        public int? CurrentRouteID
        {
            get { return curr_routeid; }
            set
            {
                curr_routeid = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<MapRouteImageModel> routeImageList;
        private bool isNavigatingToTopos;

        public ObservableCollection<MapRouteImageModel> RouteImageList
        {
            get { return routeImageList; }
            set { routeImageList = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<ImageSource> TopoDetailIcons_20
        {
            get { return topoDetailIcons_20; }
            set { SetProperty(ref topoDetailIcons_20, value); }
        }

        public ObservableCollection<ImageSource> TopoDetailIcons_54
        {
            get { return topoDetailIcons_54; }
            set { SetProperty(ref topoDetailIcons_54, value); }
        }


        private string setWidth = "";
        public string SetWidth
        {
            get { return setWidth; }
            set { setWidth = value; RaisePropertyChanged(); }
        }

        private string showTickListName = "";
        public string ShowTickListName
        {
            get { return showTickListName; }
            set { showTickListName = value; RaisePropertyChanged(); }
        }

        private string firstAscentInfo = String.Empty;
        public string FirstAscentInfo
        {
            get { return firstAscentInfo; }
            set { firstAscentInfo = value; RaisePropertyChanged(); }
        }

        private string equipperInfo = String.Empty;
        public string EquipperInfo
        {
            get { return equipperInfo; }
            set { equipperInfo = value; RaisePropertyChanged(); }
        }

        private bool isFAVisible;
        public bool IsFAVisible
        {
            get { return isFAVisible; }
            set { isFAVisible = value; RaisePropertyChanged(); }
        }

        private bool isEquippedVisible;
        public bool IsEquippedVisible
        {
            get { return isEquippedVisible; }
            set { isEquippedVisible = value; RaisePropertyChanged(); }
        }

        private string boltsAndRouteLength = String.Empty;
        public string BoltsAndRouteLength
        {
            get { return boltsAndRouteLength; }
            set { boltsAndRouteLength = value; RaisePropertyChanged(); }
        }

        private bool isdisplaywarningicon;
        public bool IsDisplayWarningIcon
        {
            get { return isdisplaywarningicon; }
            set { isdisplaywarningicon = value; RaisePropertyChanged(); }
        }

        private string route_issuenotice = String.Empty;
        private CragExtended cragData;

        public string RouteNotice
        {
            get { return route_issuenotice; }
            set { route_issuenotice = value; RaisePropertyChanged(); }
        }

        public MapListModel CurrentSector { get; set; }



        public bool IsUnlockVisible
        {
            get { return isUnlockVisible; }
            set { SetProperty(ref isUnlockVisible, value); }
        }

        private GridLength _popuptopHeight;

        public GridLength PopupTopHeight
        {
            get { return _popuptopHeight; }
            set { SetProperty(ref _popuptopHeight, value); }
        }

        private GridLength _popupbottomHeight;

        public GridLength PopupBottomHeight
        {
            get { return _popupbottomHeight; }
            set { SetProperty(ref _popupbottomHeight, value); }
        }


        private GuideBook _route_GB;
        public GuideBook RouteGuideBook
        {
            get { return _route_GB ?? (_route_GB = new GuideBook()); }
            set
            {
                SetProperty(ref _route_GB, value);
            }
        }

        private Thickness _premiumMargin;

        public Thickness PremiumBoxMargin
        {
            get { return _premiumMargin; }
            set { SetProperty(ref _premiumMargin, value); }
        }


        private Thickness _popScrollMargin;

        public Thickness ScrollPopupMargin
        {
            get { return _popScrollMargin; }
            set { SetProperty(ref _popScrollMargin, value); }
        }

        public ICommand ShowWrapTextcommand => new Command(ShowWrapTextPopup);

        private async void ShowWrapTextPopup()
        {
            await PopupNavigation.PushAsync(new ShowWrapTextPopoup("First Ascent", FirstAscentInfo), true);
        }

        public new ICommand BackCommand
        {
            get
            {
                return new Command(OnBackPressed);
            }
        }

        private async void OnBackPressed()
        {
            var navigationParameters = new NavigationParameters
            {
                { NavigationParametersConstants.IsUnlockedParameter, isUnlocked }
            };

            await navigationService.GoBackAsync(navigationParameters);
        }


        private async void OnTermsPressed()
        {
            await navigationService.NavigateAsync<TermsViewModel>();
        }

        private async void ShowHideTickListButton(int CurrentRouteID)
        {
            ShowTickListName = "+ TICK LIST";
            if (!Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                return;
            }
        }

        private async void ExecuteOnTickList(object obj)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                userDialogs.ShowLoading("Saving...");
            });

            var ascentResponse = await httpHelper.GetAsync<IList<RouteResponseModel>>(ApiUrls.Url_TickList_isRoutePresentInTAscent((int)CurrentRouteID));

            if (ascentResponse.ValidateResponse(false))
            {
                if (ascentResponse.Result.Any() && ascentResponse.Result[0].isRoutePresent == true)
                {
                    userDialogs.Toast(new ToastConfig("")
                    {
                        Message = " Route already climbed, not added to Tick List!",
                        BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                        MessageTextColor = System.Drawing.Color.White,
                        Duration = TimeSpan.FromSeconds(3)
                    });
                }
                else
                {
                    var tickListResponse = await httpHelper.GetAsync<IList<RouteResponseModel>>(ApiUrls.Url_TickList_isRoutePresentInTickList((int)CurrentRouteID));

                    if (tickListResponse.ValidateResponse(false))
                    {
                        //if the route is not on our tick list, add it
                        if (tickListResponse.Result.Any() && tickListResponse.Result[0].isRoutePresent == false)
                        {
                            var addTickListResponse = await httpHelper.GetAsync<string>(ApiUrls.Url_TickList_AddTickList((int)CurrentRouteID));

                            userDialogs.Toast(new ToastConfig("")
                            {
                                Message = " Route added to Tick List!",
                                BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                                MessageTextColor = System.Drawing.Color.White,
                                Duration = TimeSpan.FromSeconds(3)
                            });
                        }
                        //if the route is on our tick list
                        else
                        {
                            //HttpClientHelper httpClinetHelper = new ApiHandler(string.Format(ApiUrls.Url_TickList_DeleteTickList, Convert.ToInt64(CurrentRouteID)), Settings.AccessToken);
                            //var tickList_response = await httpClinetHelper.Get<string>();
                            userDialogs.Toast(new ToastConfig("")
                            {
                                Message = " Route already exists in your Tick List!",
                                BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                                MessageTextColor = System.Drawing.Color.White,
                                Duration = TimeSpan.FromSeconds(3)
                            });
                        }

                    }
                    else
                    {
                        //Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.ShowError("Tick List requires an internet connection."));
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            userDialogs.HideLoading();
                            userDialogs.AlertAsync("Tick List Requires an Internet Connection.", "Network Error", "Continue");
                        });
                        return;
                    }
                }
            }
            else
            {
                //userDialogs.HideLoading();
                //Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.ShowError("Tick List requires an internet connection."));
                Device.BeginInvokeOnMainThread(() =>
                {
                    userDialogs.HideLoading();
                    userDialogs.AlertAsync("Tick List Requires an Internet Connection.", "Network Error", "Continue");
                });
                return;
            }

            userDialogs.HideLoading();
            await navigationService.GoBackAsync();
        }

        private async void ExecuteOnSends(string obj)
        {
            try
            {
                //if (!Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
                //{
                //    //Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.ShowError("SendModel requires an internet connection."));
                //    Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.AlertAsync("SendModel Requires an Internet Connection.", "Network Error", "Continue"));
                //    return;
                //}

                userDialogs.ShowLoading("Loading");

                Cache.SendBackArrowCount = 1;

                var navigationParameters = new NavigationParameters();
                navigationParameters.Add(NavigationParametersConstants.CurrentRouteIdParameter, CurrentRouteID);
                navigationParameters.Add(NavigationParametersConstants.CurrentSectorParameter, CurrentSector);
                navigationParameters.Add(NavigationParametersConstants.TopoImageResponseParameter, topoImg);
                navigationParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, CurrentSector);
                await navigationService.NavigateAsync<AscentProcessViewModel>(navigationParameters);
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.ExecuteOnSends),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                });
            }
        }

        public async void LoadRouteData(int? obj)
        {
            CurrentRouteID = obj;
            var routeData = await routeRepository.FindAsync(route => route.route_id == CurrentRouteID && route.is_enabled);
            if (routeData == null)
            {
                return;
            }

            TopoDetailIcons_20 = new ObservableCollection<ImageSource>();
            TopoDetailIcons_54 = new ObservableCollection<ImageSource>();

            // Hold Type
            if (!string.IsNullOrEmpty(routeData.hold_type_top_1) && Convert.ToInt32(routeData.hold_type_top_1) > 0)
            {
                TopoDetailIcons_20.Add(ImageSource.FromFile(GetTopHoldResourceName(routeData.hold_type_top_1) + "_20x20"));
                TopoDetailIcons_54.Add(ImageSource.FromFile(GetTopHoldResourceName(routeData.hold_type_top_1) + "_54x54"));
            }

            // Route climbing style
            if (!string.IsNullOrEmpty(routeData.route_style_top_1) &&
                Convert.ToInt32(routeData.route_style_top_1) > 0)
            {
                TopoDetailIcons_20.Add(ImageSource.FromFile(GetTopRouteStyleResourceName(routeData.route_style_top_1) +
                                                         "_20x20"));
                TopoDetailIcons_54.Add(ImageSource.FromFile(GetTopRouteStyleResourceName(routeData.route_style_top_1) +
                                                            "_54x54"));
            }

            //Special Gear
            if (!string.IsNullOrEmpty(routeData.special_gear) && routeData.special_gear != "0")
            {
                string[] specialgear = routeData.special_gear.Split(',');
                int count = 0;

                while (specialgear.Count() > 0)
                {
                    string special_gear = string.Empty, image = string.Empty;

                    if (specialgear.Count() - 1 >= count)
                    {
                        special_gear = specialgear[count];
                        image = GetSpecialGear(special_gear);
                        if (!string.IsNullOrEmpty(image))
                        {
                            TopoDetailIcons_20.Add(ImageSource.FromFile(image + "_20x20"));
                            TopoDetailIcons_54.Add(ImageSource.FromFile(image + "_54x54"));
                        }
                        count++;
                    }
                    else
                        break;
                }
                //TopoDetailIcons_20.Add(ImageSource.FromFile(GetSpecialGear(routeData.special_gear) + "_20x20"));
                //TopoDetailIcons_54.Add(ImageSource.FromFile(GetSpecialGear(routeData.special_gear) + "_54x54"));
            }

            //Safety Rating
            if (!string.IsNullOrEmpty(routeData.safety_rating) && routeData.safety_rating != "0" &&
                !string.IsNullOrEmpty(routeData.safety_rating_type) && routeData.safety_rating_type != "0")
            {
                TopoDetailIcons_20.Add(ImageSource.FromFile(GetsafetyRating(routeData.safety_rating) + "_20x20"));
                TopoDetailIcons_54.Add(ImageSource.FromFile(GetsafetyRating(routeData.safety_rating) + "_54x54"));
            }



            //if (!string.IsNullOrEmpty(routeData.angles_top_1) && Convert.ToInt32(routeData.angles_top_1) > 0)
            //{
            //    TopoDetailIcons_20.Add(ImageSource.FromFile(GetTopAngleResourceName(routeData.angles_top_1) + "_20x20"));
            //    TopoDetailIcons_54.Add(ImageSource.FromFile(GetTopAngleResourceName(routeData.angles_top_1) + "_54x54"));
            //}
            //else
            //{
            //    TopAngle_20 = ImageSource.FromFile(GetTopAngleResourceName("2") + "_20x20");
            //    TopAngle_54 = ImageSource.FromFile(GetTopAngleResourceName("2") + "_54x54");
            //}	

            //// Route Type
            // if (!string.IsNullOrEmpty(routeData.route_type) && Convert.ToInt32(routeData.route_type_id) > 0)
            //{
            //   TopoDetailIcons_20.Add(ImageSource.FromFile(GetRouteTypeResourceName(routeData.route_type_id) + "_20x20"));
            //             TopoDetailIcons_54.Add(ImageSource.FromFile(GetRouteTypeResourceName(routeData.route_type_id) + "_54x54"));
            //} 			


            Device.BeginInvokeOnMainThread(() =>
            {
                SectorName = routeData.route_name;
                TechGrade = routeData.tech_grade;
                RouteName = (routeData.route_name).ToUpper() + " " + TechGrade;
                CragName = routeData.crag_id;
                RouteInfo = routeData.route_info;

                Rating = Convert.ToDouble(routeData.rating, CultureInfo.InvariantCulture);
                //Rating = Convert.ToDouble(Math.Round(Convert.ToDecimal(routeData.rating), 0, MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture);

                string strFirstAscentName = routeData.first_ascent_name?.Trim();
                string strEquipperName = routeData.equipper_name?.Trim();

                if (AppSetting.APP_TYPE == "outdoor")
                {
                    DateTime dtFirstAscentDate = routeData.first_ascent_date.HasValue ? routeData.first_ascent_date.Value : new DateTime(1900, 1, 1);
                    DateTime dtEquipperDate = routeData.equipper_date.HasValue ? routeData.equipper_date.Value : new DateTime(1900, 1, 1);

                    if (!string.IsNullOrEmpty(strFirstAscentName))
                    {
                        FirstAscentInfo = dtFirstAscentDate.Year <= 1900 ? strFirstAscentName : strFirstAscentName + ", " + dtFirstAscentDate.Year.ToString();
                        EquipperInfo = dtEquipperDate.Year <= 1900 ? strEquipperName : strEquipperName + ", " + dtEquipperDate.Year.ToString();

                        IsFAVisible = true;
                        IsEquippedVisible = true;

                        if ((FirstAscentInfo == EquipperInfo) || (EquipperInfo == ""))
                        {
                            IsEquippedVisible = false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strEquipperName))
                        {
                            EquipperInfo = dtEquipperDate.Year <= 1900 ? strEquipperName : strEquipperName + ", " + dtEquipperDate.Year.ToString();

                            IsFAVisible = false;
                            IsEquippedVisible = true;
                        }
                        else
                        {
                            IsFAVisible = false;
                            IsEquippedVisible = false;
                        }
                    }
                }

                double routeLength = 0;
                string routeUom = "m";

                if (!string.IsNullOrEmpty(routeData.route_length))
                {
                    if (!string.IsNullOrWhiteSpace(Settings.UnitOfMeasure))
                    {
                        if (Settings.UnitOfMeasure.Equals("ft"))
                        {
                            //What for is 3.28084
                            routeLength = Math.Round(Convert.ToDouble(routeData.route_length, CultureInfo.InvariantCulture) * 3.28084);
                            routeUom = "ft";
                        }
                        else { routeLength = Math.Round(Convert.ToDouble(routeData.route_length, CultureInfo.InvariantCulture)); }
                    }
                    else if (AppSetting.APP_UOM.Equals("imperial"))
                    {
                        //What for is 3.28084
                        routeLength = Math.Round(Convert.ToDouble(routeData.route_length, CultureInfo.InvariantCulture) * 3.28084);
                        routeUom = "ft";
                    }
                    else { routeLength = Math.Round(Convert.ToDouble(routeData.route_length, CultureInfo.InvariantCulture)); }
                }

                int? numberOfBolts = routeData.number_of_bolts;

                if (!numberOfBolts.HasValue)
                {
                    numberOfBolts = 0;
                }

                //if we have both bolts and route length
                if (numberOfBolts > 0 && routeLength > 0)
                {
                    BoltsAndRouteLength = numberOfBolts + " Bolts - " + routeLength + " " + routeUom;

                }
                //if we only have number of bolts
                else if (numberOfBolts > 0)
                {
                    BoltsAndRouteLength = numberOfBolts + " Bolts";

                }
                //if we only have route length
                else if (routeLength > 0)
                {
                    BoltsAndRouteLength = routeLength + " " + routeUom;
                }

                IsNavigatingToTopos = false;
            });
        }

        private void ExecuteOnHideRoutePopupLgCommand()
        {
            if (IsUnlockVisible)
            {
                return;
            }

            DisplayRoutePopupLg = false;
            DisplayRoutePopupSm = true;
        }

        private async void NavigateToUnlockPage()
        {
            try
            {
                userDialogs.ShowLoading();

                if (guideBook == null)
                    guideBook = await guideBookRepository.FindAsync(g => g.GuideBookId == cragData.crag_guide_book);
                navigationService.NavigateAsync<PremiumSubscriptionViewModel>();
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.ExecuteOnShowRoutePopupLgCommand),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(cragData)
                });
            }
        }

        private async void ExecuteOnShowRoutePopupLgCommand()
        {
            DisplayRoutePopupLg = true;
            DisplayRoutePopupSm = false;
        }

        private void ExecuteOnHideSmallPopupCommand()
        {
            DisplayRoutePopupSm = false;
            IsNavigatedFromSectorImage = false;
            InvalidateSuface?.Invoke();
            Cache.IsGlobalRouteId = Cache.IsTapOnSectorImage == true ? true : false;
        }

        private async void ExecuteOnIssueCommand()
        {
            //userDialogs.ShowLoading();
            var navigationParameter = new NavigationParameters();
            navigationParameter.Add(NavigationParametersConstants.SingleRouteImageByteParameter, singleRouteImageBytes);
            navigationParameter.Add(NavigationParametersConstants.CurrentRouteIdParameter, CurrentRouteID);
            navigationParameter.Add(NavigationParametersConstants.CurrentSectorParameter, CurrentSector);
            await navigationService.NavigateAsync<ReportIssueViewModel>(navigationParameter);
        }

        private async void ExecuteOnWarning()
        {
            if (!IsDisplayWarningIcon)
            {
                NavigateToUnlockPage();
                return;
            }

            if (!string.IsNullOrEmpty(RouteNotice))
            {
                await PopupNavigation.PushAsync(new IssueNoticePopupPage(navigationService, userDialogs, CurrentRouteID, RouteNotice), true);
            }
            else
            {
                userDialogs.ShowLoading();
                var navigationParameter = new NavigationParameters();
                navigationParameter.Add(NavigationParametersConstants.CurrentRouteIdParameter, CurrentRouteID);
                navigationParameter.Add(NavigationParametersConstants.CurrentSectorParameter, CurrentSector);
                await navigationService.NavigateAsync<ReportedIssueListViewModel>(navigationParameter);
            }
        }

        //Angle hold
        private string GetTopAngleResourceName(string angle)
        {
            string resource = string.Empty;
            switch (angle)
            {
                case "1":
                    resource = "icon_steepness_1_slab_border";
                    break;
                case "2":
                    resource = "icon_steepness_2_vertical_border";
                    break;
                case "4":
                    resource = "icon_steepness_4_overhanging_border";
                    break;
                case "8":
                    resource = "icon_steepness_8_roof_border";
                    break;
            }
            return resource;
        }

        //Hold Type
        private string GetTopHoldResourceName(string hold)
        {
            string resource = string.Empty;
            switch (hold)
            {
                case "1":
                    resource = "icon_hold_type_1_slopers_border";
                    break;
                case "2":
                    resource = "icon_hold_type_2_crimps_border";
                    break;
                case "4":
                    resource = "icon_hold_type_4_jugs_border";
                    break;
                case "8":
                    resource = "icon_hold_type_8_pockets_border";
                    break;
                case "16":
                    resource = "icon_hold_type_16_pinches_border";
                    break;
                case "32":
                    resource = "icon_hold_type_32_jams_border";
                    break;
            }
            return resource;
        }

        //Climbing style
        private string GetTopRouteStyleResourceName(string route)
        {
            string resource = "icon_route_style_1_technical_border";
            switch (route)
            {
                case "1":
                    resource = "icon_route_style_1_technical_border";
                    break;
                case "2":
                    resource = "icon_route_style_2_sequential_border";
                    break;
                case "4":
                    resource = "icon_route_style_4_powerful_border";
                    break;
                case "8":
                    resource = "icon_route_style_8_sustained_border";
                    break;
                case "16":
                    resource = "icon_route_style_16_one_move_border";
                    break;
                case "32":
                    resource = "icon_route_style_32_exposed_border";
                    break;
            }
            return resource;
        }

        //Route Type
        private string GetRouteTypeResourceName(int route)
        {
            string resource = string.Empty;
            switch (route)
            {
                case 1:
                    resource = "icon_route_type_sport";
                    break;
                case 2:
                    resource = "icon_route_type_bouldering";
                    break;
                case 3:
                    resource = "icon_route_type_traditional";
                    break;
                case 5:
                    resource = "icon_route_type_aid";
                    break;
            }

            return resource;
        }

        //safety rating
        private string GetsafetyRating(string safety_rating)
        {
            string resource = string.Empty;

            switch (safety_rating)
            {
                case "Yes":
                    resource = "icon_safety_rating_dangerous_route";
                    break;
                case "Minor":
                    resource = "icon_safety_rating_minor";
                    break;
                case "Major":
                    resource = "icon_safety_rating_major";
                    break;
                case "Death":
                    resource = "icon_safety_rating_death";
                    break;
                case "X":
                    resource = "icon_safety_rating_x";
                    break;
                case "R":
                    resource = "icon_safety_rating_r";
                    break;
                case "PG":
                    resource = "icon_safety_rating_pg";
                    break;
                case "PG-13":
                    resource = "icon_safety_rating_pg13";
                    break;
            }
            return resource;
        }

        //special gear
        private string GetSpecialGear(string special_gear)
        {
            //string[] specialgear = special_gear.Split(',');
            string resource = string.Empty;
            //int count = 0;

            //while (specialgear.Count() > 0)
            //{
            //	special_gear = specialgear[count];

            switch (special_gear)
            {
                case "Carrot Hanger":
                    resource = "icon_special_gear_carrot_hanger";
                    break;
                case "Stick Clip":
                    resource = "icon_special_gear_stick_clip";
                    break;
                case "Knee Pad(s)":
                    resource = "icon_special_gear_knee_pads";
                    break;
                case "Batman":
                    resource = "icon_special_gear_batman";
                    break;
            }
            //	if (String.IsNullOrEmpty(resource) && specialgear.Count()-1 > count)	
            //		count++;  	
            //	else
            //	   break;
            //}
            return resource;
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
            if (parameters.Count == 0)
            {
                return;
            }

            if (parameters.Count < 2 && parameters.ContainsKey(NavigationParametersConstants.IsUnlockedParameter))
            {
                isUnlocked = (bool)parameters[NavigationParametersConstants.IsUnlockedParameter];
                if (isUnlocked)
                {
                    IsUnlockVisible = !isUnlocked;
                    IsNavigatingToTopos = false;
                    IsImageBackgroundVisible = false;
                    IsAndroidScrollVisible = true;
                    IsIOSScrollVisible = true;
                    DisplayRoutePopupLg = !IsNavigatedFromSectorImage;
                    IsNavigatedFromSectorImage = false;
                }
                else if (!Settings.AppPurchased)
                {
                    cragData = await cragExtendedRepository.FindAsync(crag => crag.crag_id == Settings.ActiveCrag);
                    IsUnlockVisible = !cragData.Unlocked;
                    IsNavigatingToTopos = !cragData.Unlocked;
                    IsImageBackgroundVisible = !cragData.Unlocked;
                    IsAndroidScrollVisible = cragData.Unlocked;
                    IsIOSScrollVisible = cragData.Unlocked;
                    DisplayRoutePopupLg = IsNavigatedFromSectorImage;
                }
                else
                {
                    IsUnlockVisible = !Settings.AppPurchased;
                    IsNavigatingToTopos = false;
                    IsImageBackgroundVisible = false;
                    IsAndroidScrollVisible = true;
                    IsIOSScrollVisible = true;
                    DisplayRoutePopupLg = !IsNavigatedFromSectorImage;
                }

                return;
            }

            try
            {
                CurrentSector = (MapListModel)parameters[NavigationParametersConstants.SelectedSectorObjectParameter];
                topoImg = (TopoImageResponseModel)parameters[NavigationParametersConstants.TopoImageResponseParameter];
                routeId = (int)parameters[NavigationParametersConstants.RouteIdParameter];
                singleRoute = (bool)parameters[NavigationParametersConstants.SingleRouteParameter];
                guideBookId = (long)parameters[NavigationParametersConstants.GuideBookIdParameter];
                cragImage = await cragImageRepository.FindAsync(tcragimg => tcragimg.crag_id == Settings.ActiveCrag);
                isUnlocked = (bool)parameters[NavigationParametersConstants.IsUnlockedParameter];
                IsNavigatedFromSectorImage = (bool)parameters[NavigationParametersConstants.IsNavigatedFromSectorImageParameter];
                if (isUnlocked)
                {
                    IsUnlockVisible = !isUnlocked;
                    IsNavigatedFromSectorImage = !isUnlocked;
                }
                else if (!Settings.AppPurchased)
                {
                    cragData = await cragExtendedRepository.FindAsync(crag => crag.crag_id == Settings.ActiveCrag);
                    IsUnlockVisible = !cragData.Unlocked;
                    IsNavigatingToTopos = IsNavigatedFromSectorImage && IsUnlockVisible;
                    DisplayRoutePopupLg = IsUnlockVisible;
                }
                else
                {
                    IsUnlockVisible = !Settings.AppPurchased;
                }

                if (routeId > 0 && topoImg != null && singleRoute && !IsUnlockVisible)
                {
                    DisplayRoutePopupSm = true;
                }

                //if (IsUnlockVisible)
                //{
                //    LoadCragAndDefaultImage();
                //}

                Cache.SendBackArrowCount = 1;
                // load the scenic shot if there are no topos available
                if (topoImg == null)
                {
                    if (routeId > 0)
                    {
                        DisplayRoutePopupLg = true;
                        IsHideSwipeUp = false;
                    }

                    if (CurrentSector.IsCragOrDefaultImageCount != 1)
                    {
                        var secimglist = await topoRepository.GetAsync(tp => tp.sector_id == CurrentSector.SectorId);
                        foreach (var _item in secimglist)
                        {
                            if (!(_item.topo_json == "[]"))
                            {
                                isNoSingleRouteDrawn = true;
                                var topoimage = JsonConvert.DeserializeObject<List<TopoImageResponseModel>>(_item.topo_json);
                                topoImg = topoimage[0];
                                LoadDefault(topoImg);
                            }
                        }
                    }
                    else LoadCragAndDefaultImage();
                }
                // otherwise load the topos
                else
                {
                    IsRunningTasks = true;
                    LoadDefault(topoImg);
                }

                if (singleRoute)
                {
                    LoadRouteData(routeId);
                }

                if ((topoImg?.drawing == null
                     || topoImg == null)
                    && routeId > 0)
                {
                    DisplayRoutePopupSm = false;
                    DisplayRoutePopupLg = true;
                    IsHideSwipeUp = false;
                }

                if (topoImg == null)
                {
                    if (string.IsNullOrEmpty(cragImage?.crag_image))
                    {
                        SetupImageSource();
                    }
                    else
                    {
                        if (CurrentSector.IsCragOrDefaultImageCount == 0)
                        {
                            ImageBackgroundSource = CurrentSector.SectorImage;
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                var imageBytes = Convert.FromBase64String(cragImage.crag_image.Split(',')[1]);
                                ImageBackgroundSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                            });
                        }
                    }

                    IsImageBackgroundVisible = true;
                    IsAndroidScrollVisible = false;
                    IsIOSScrollVisible = false;
                    IsRunningTasks = false;
                }
                PageHeaderText = CurrentSector.SectorName;
                PageSubHeaderText = CurrentSector.SectorSubInfo;
                CurrentSectorImage = CurrentSector.SectorImage;
                ShowHideTickListButton(routeId);

                if (!IsUnlockVisible)
                {
                    PopupTopHeight = new GridLength(8.5, GridUnitType.Star);
                    PopupBottomHeight = new GridLength(1.5, GridUnitType.Star);
                }
                else
                {
                    PopupTopHeight = GridLength.Auto;
                    PopupBottomHeight = GridLength.Auto;
                }
                if (IsNavigatedFromSectorImage)
                    PremiumBoxMargin = new Thickness(0, -250, 0, 0);
                else
                    PremiumBoxMargin = new Thickness(0, 0, 0, 0);

                //ScrollPopupMargin
                if (Device.RuntimePlatform == Device.iOS)
                    ScrollPopupMargin = new Thickness(0, 0, 0, -90);
                else
                    ScrollPopupMargin = new Thickness(0, 0, 0, -10);

                var routeissue = await issueRepository.GetAsync(x => x.route_id == routeId);
                IsDisplayWarningIcon = routeissue.Count > 0 ? true : false;
                var routeinfo = await routeRepository.GetAsync(x => x.route_id == routeId);
                if (routeinfo?.Count > 0)
                {
                    RouteNotice = routeinfo[0].route_safety_notice;
                }
                guideBook = await guideBookRepository.FindAsync(g => g.GuideBookId == guideBookId);

                if(guideBook != null)
                    RouteGuideBook = new GuideBook(guideBook);
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"selectedSectorObject = {JsonConvert.SerializeObject(parameters["selectedSectorObject"] ?? "")}, topoImageResponse = {JsonConvert.SerializeObject(parameters[NavigationParametersConstants.TopoImageResponseParameter] ?? "")}, routeId = {JsonConvert.SerializeObject(parameters["routeId"] ?? "")}, singleRoute = {JsonConvert.SerializeObject(parameters["singleRoute"] ?? "")}"
                });
            }
        }

        public void LoadDefault(TopoImageResponseModel topoimg)
        {
            if (!string.IsNullOrEmpty(topoimg?.image?.data))
            {
                if (topoimg.image.name == "No_Image.jpg")
                {
                    if (routeId > 0)
                    {
                        DisplayRoutePopupLg = true;
                        IsHideSwipeUp = false;
                        DisplayRoutePopupSm = false;
                    }

                    LoadCragAndDefaultImage();
                }
                else
                {
                    if (topoimg.drawing.Count == 0)
                    {
                        string string64 = topoimg.image.data.Split(',')[1];
                        if (!string.IsNullOrEmpty(string64))
                        {
                            SetupBackgroundImageSource(string64);
                        }
                    }
                    else
                    {
                        if (isNoSingleRouteDrawn == true)
                        {
                            string string64 = topoimg.image.data.Split(',')[1];
                            if (!string.IsNullOrEmpty(string64))
                            {
                                SetupBackgroundImageSource(string64);
                            }
                        }
                    }

                    if (routeId <= 0)
                    {
                        return;
                    }

                    if (singleRoute && !IsUnlockVisible)
                    {
                        if (!topoimg.drawing.Any() || !(topoimg.drawing[0].line?.points?.Any() ?? false))
                        {
                            DisplayRoutePopupSm = false;
                            DisplayRoutePopupLg = true;
                            IsHideSwipeUp = false;
                        }
                        else if (isNoSingleRouteDrawn == true)
                        {
                            DisplayRoutePopupSm = false;
                            DisplayRoutePopupLg = true;
                            IsHideSwipeUp = false;
                        }
                        else
                        {
                            IsNavigatingToTopos = IsNavigatedFromSectorImage && IsUnlockVisible;
                            DisplayRoutePopupSm = true;
                            DisplayRoutePopupLg = false;
                        }

                    }
                    else
                    {
                        DisplayRoutePopupSm = false;
                        DisplayRoutePopupLg = true;
                        IsHideSwipeUp = false;
                    }
                }
            }
        }

        private void SetupBackgroundImageSource(string string64)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                byte[] imageBytes = Convert.FromBase64String(string64);

                ImageBackgroundSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                IsImageBackgroundVisible = true;
                IsAndroidScrollVisible = false;
                IsIOSScrollVisible = false;
            });
        }

        public async void LoadCragAndDefaultImage()
        {
            try
            {
                if (cragImage != null && !string.IsNullOrEmpty(cragImage.crag_image))
                {
                    var _string64 = cragImage.crag_image.Split(',')[1];
                    if (!string.IsNullOrEmpty(_string64))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            byte[] imageBytes = Convert.FromBase64String(_string64);

                            ImageBackgroundSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                            IsImageBackgroundVisible = true;
                            IsAndroidScrollVisible = false;
                            IsIOSScrollVisible = false;
                        });
                    }
                    else
                    {
                        SetupBackgroundImage();
                    }
                }
                else
                {
                    SetupBackgroundImage();
                }
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.LoadCragAndDefaultImage),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"RouteId : {routeId} ; SectorName : {PageHeaderText}, cragImage = {Newtonsoft.Json.JsonConvert.SerializeObject(cragImage)}"
                });
            }
        }

        private void SetupBackgroundImage()
        {
            SetupImageSource();
            IsImageBackgroundVisible = true;
            IsAndroidScrollVisible = false;
            IsIOSScrollVisible = false;
        }

        private void SetupImageSource()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (AppSetting.APP_TYPE == "indoor")
                {
                    //this.BackgroundImage = "default_sloper_indoor_portrait";
                    ImageBackgroundSource = ImageSource.FromFile("default_sloper_indoor_portrait.jpg");
                }
                else
                {
                    // this.BackgroundImage = "default_sloper_outdoor_portrait";
                    ImageBackgroundSource = ImageSource.FromFile("default_sloper_outdoor_portrait.jpg");
                }
            });
        }
    }
}
