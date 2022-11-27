using Acr.UserDialogs;
using Plugin.Connectivity;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.RankingModels;
using SloperMobile.UserControls;
using SloperMobile.ViewModel.MyViewModels;
using SloperMobile.ViewModel.SocialModels;
using SloperMobile.ViewModel.SubscriptionViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.ProfileViewModels
{
	//TODO: Refactor. Too much code duplicate
	public class ProfileRankingViewModel : BaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<ProfileFilterTable> profileFilterTable;
        private readonly IRepository<ProfileFilterTypeTable> profileFilterTypeTable;
        private readonly IRepository<GuideBookTable> guidebookRepository;
        private CragTable currentCrag;

        int userId;

        public ProfileRankingViewModel(
             INavigationService navigationService,
             IRepository<UserInfoTable> userInfoRepository,
             IRepository<ProfileFilterTable> profileFilterTable,
             IRepository<ProfileFilterTypeTable> profileFilterTypeTable,
             IRepository<AreaTable> areaRepository,
			 IExceptionSynchronizationManager exceptionManager,
			 IUserDialogs userDialogs,
             IRepository<CragExtended> cragRepository,
             IRepository<GuideBookTable> guidebookRepository,
             IHttpHelper httpClinet)
            : base(navigationService, exceptionManager, httpClinet)
        {
            this.cragRepository = cragRepository;
            this.userInfoRepository = userInfoRepository;
            this.profileFilterTable = profileFilterTable;
            this.profileFilterTypeTable = profileFilterTypeTable;
            this.guidebookRepository = guidebookRepository;
			this.userDialogs = userDialogs;

            AllGaugeTappedCommand = new Command(OnAllGaugeTappedAsync);
            MonthlyGaugeTappedCommand = new Command(OnMonthlyGaugeTappedAsync);
            YearlyGaugeTappedCommand = new Command(OnYearlyGaugeTappedAsync);
            ApplyFilterCommand = new Command(OnApplyFilterAsync);

            GaugeThickness = ((Application.Current.MainPage.Width) / (40));
            CenterGaugeMargin = 0;

            RimThickness = (Device.RuntimePlatform == Device.iOS) ? GaugeThickness * 2 : GaugeThickness;
           
            EditProfileCommand = new Command(ExecuteOnEditProfile);
        }

        #region Delegate Commands
        public ICommand AllGaugeTappedCommand { get; set; }
        public ICommand MonthlyGaugeTappedCommand { get; set; }
        public ICommand YearlyGaugeTappedCommand { get; set; }
        public ICommand ApplyFilterCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
        #endregion

        #region Properties

        private int centergaugemargin;
        public int CenterGaugeMargin
        {
            get { return centergaugemargin; }
            set { centergaugemargin = value; RaisePropertyChanged(); }
        }

        private double rimThickness;
        public double RimThickness
        {
            get { return rimThickness; }
            set { rimThickness = value; RaisePropertyChanged(); }
        }

        private double gaugethickness;
        public double GaugeThickness
        {
            get { return gaugethickness; }
            set { gaugethickness = value; RaisePropertyChanged(); }
        }

        //private double gaugefontsize;
        //public double GaugeFontSize
        //{
        //    get { return gaugefontsize; }
        //    set { gaugefontsize = value; RaisePropertyChanged(); }
        //}

        //private double gaugefontsizesub;
        //public double GaugeFontSizeSub
        //{
        //    get { return gaugefontsizesub; }
        //    set { gaugefontsizesub = value; RaisePropertyChanged(); }
        //}

        private ObservableCollection<ShowRankingModel> rankings = new ObservableCollection<ShowRankingModel>();
        public ObservableCollection<ShowRankingModel> Rankings
        {
            get { return rankings; }
            set { SetProperty(ref rankings, value); }
        }

        private ObservableCollection<ShowRankingModel> _allRankings = new ObservableCollection<ShowRankingModel>();
        public ObservableCollection<ShowRankingModel> AllRankings
        {
            get { return _allRankings; }
            set { _allRankings = value; RaisePropertyChanged("AllRankings"); }
        }

        private ObservableCollection<ShowRankingModel> _monthlyRankings = new ObservableCollection<ShowRankingModel>();
        public ObservableCollection<ShowRankingModel> MonthlyRankings
        {
            get { return _monthlyRankings; }
            set { _monthlyRankings = value; RaisePropertyChanged("MonthlyRankings"); }
        }

        private ObservableCollection<ShowRankingModel> _yearlyRankings = new ObservableCollection<ShowRankingModel>();
        public ObservableCollection<ShowRankingModel> YearlyRankings
        {
            get { return _yearlyRankings; }
            set { _yearlyRankings = value; RaisePropertyChanged("YearlyRankings"); }
        }

        private int _allUserRank;
        public int AllUserRank
        {
            get { return _allUserRank; }
            set { _allUserRank = value; RaisePropertyChanged(); }
        }

        private int _monthlyUserRank;
        public int MonthlyUserRank
        {
            get { return _monthlyUserRank; }
            set { _monthlyUserRank = value; RaisePropertyChanged(); }
        }

        private int _yearlyUserRank;
        public int YearlyUserRank
        {
            get { return _yearlyUserRank; }
            set { _yearlyUserRank = value; RaisePropertyChanged(); }
        }

        private double _allRankPercentage;
        public double AllRankPercentage
        {
            get { return _allRankPercentage; }
            set { _allRankPercentage = value; RaisePropertyChanged(); }
        }

        private double _monthlyRankPercentage;
        public double MonthlyRankPercentage
        {
            get { return _monthlyRankPercentage; }
            set { _monthlyRankPercentage = value; RaisePropertyChanged(); }
        }

        private double _yearlyRankPercentage;
        public double YearlyRankPercentage
        {
            get { return _yearlyRankPercentage; }
            set { _yearlyRankPercentage = value; RaisePropertyChanged(); }
        }

        private Color _todayGaugeColor;
        public Color TodayGaugeColor
        {
            get { return _todayGaugeColor; }
            set { _todayGaugeColor = value; RaisePropertyChanged("TodayGaugeColor"); }
        }

        private Color _monthlyGaugeColor;
        public Color MonthlyGaugeColor
        {
            get { return _monthlyGaugeColor; }
            set { _monthlyGaugeColor = value; RaisePropertyChanged("MonthlyGaugeColor"); }
        }

        private Color _yearlyGaugeColor;
        public Color YearlyGaugeColor
        {
            get { return _yearlyGaugeColor; }
            set { _yearlyGaugeColor = value; RaisePropertyChanged("YearlyGaugeColor"); }
        }

        private bool _isStatusVisible;
        public bool IsStatusVisible
        {
            get { return _isStatusVisible; }
            set { _isStatusVisible = value; RaisePropertyChanged("IsStatusVisible"); }
        }

        private string _cragName;
        public string CragName
        {
            get { return _cragName; }
            set { _cragName = value; RaisePropertyChanged("CragName"); }
        }

        private string _areaName;
        public string AreaName
        {
            get { return _areaName; }
            set { _areaName = value; RaisePropertyChanged("AreaName"); }
        }

        private double _allGaugeOpacity;
        public double AllGaugeOpacity
        {
            get { return _allGaugeOpacity; }
            set { _allGaugeOpacity = value; RaisePropertyChanged("AllGaugeOpacity"); }
        }

        private double _monthlyGaugeOpacity;
        public double MonthlyGaugeOpacity
        {
            get { return _monthlyGaugeOpacity; }
            set { _monthlyGaugeOpacity = value; RaisePropertyChanged("MonthlyGaugeOpacity"); }
        }

        private double _yearlyGaugeOpacity;
        public double YearlyGaugeOpacity
        {
            get { return _yearlyGaugeOpacity; }
            set { _yearlyGaugeOpacity = value; RaisePropertyChanged("YearlyGaugeOpacity"); }
        }

        private bool _isHeaderVisible;
        public bool IsHeaderVisible
        {
            get { return _isHeaderVisible; }
            set { _isHeaderVisible = value; RaisePropertyChanged("IsHeaderVisible"); }
        }

        private string _gender;
        public string Gender
        {
            get { return _gender; }
            set { SetProperty(ref _gender, value); }
        }

        private string _age;
        public string Age
        {
            get { return _age; }
            set { SetProperty(ref _age, value); }
        }

        private string _height;
        public string Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        private string _weight;
        public string Weight
        {
            get { return _weight; }
            set { SetProperty(ref _weight, value); }
        }

        private string _yearsClimbing;
        public string YearsClimbing
        {
            get { return _yearsClimbing; }
            set { SetProperty(ref _yearsClimbing, value); }
        }

        private bool _isGenderToggled;
        public bool IsGenderToggled
        {
            get { return _isGenderToggled; }
            set { SetProperty(ref _isGenderToggled, value); }
        }

        private bool _isGenderEnable;
        public bool IsGenderEnable
        {
            get { return _isGenderEnable; }
            set { SetProperty(ref _isGenderEnable, value); }
        }

        private bool _isAgeToggled;
        public bool IsAgeToggled
        {
            get { return _isAgeToggled; }
            set { SetProperty(ref _isAgeToggled, value); }
        }

        private bool _isAgeEnable;
        public bool IsAgeEnable
        {
            get { return _isAgeEnable; }
            set { SetProperty(ref _isAgeEnable, value); }
        }

        private bool _isHeightToggled;
        public bool IsHeightToggled
        {
            get { return _isHeightToggled; }
            set { SetProperty(ref _isHeightToggled, value); }
        }

        private bool _isHeightEnable;
        public bool IsHeightEnable
        {
            get { return _isHeightEnable; }
            set { SetProperty(ref _isHeightEnable, value); }
        }

        private bool _isWeightToggled;
        public bool IsWeightToggled
        {
            get { return _isWeightToggled; }
            set { SetProperty(ref _isWeightToggled, value); }
        }

        private bool _isWeightEnable;
        public bool IsWeightEnable
        {
            get { return _isWeightEnable; }
            set { SetProperty(ref _isWeightEnable, value); }
        }

        private bool _isYearsClimbingToggled;
        public bool IsYearsClimbingToggled
        {
            get { return _isYearsClimbingToggled; }
            set { SetProperty(ref _isYearsClimbingToggled, value); }
        }

        private bool _isYearsClimbingEnable;
        public bool IsYearsClimbingEnable
        {
            get { return _isYearsClimbingEnable; }
            set { SetProperty(ref _isYearsClimbingEnable, value); }
        }

        private bool _isWarningVisible;
        public bool IsWarningVisible
        {
            get { return _isWarningVisible; }
            set { SetProperty(ref _isWarningVisible, value); }
        }

        private string _filteredBy;
        public string FilteredBy
        {
            get { return _filteredBy; }
            set { SetProperty(ref _filteredBy, value); }
        }

        private Color _genderColor;
        public Color GenderColor
        {
            get { return _genderColor; }
            set { SetProperty(ref _genderColor, value); }
        }

        private Color _ageColor;
        public Color AgeColor
        {
            get { return _ageColor; }
            set { SetProperty(ref _ageColor, value); }
        }

        private Color _heightColor;
        public Color HeightColor
        {
            get { return _heightColor; }
            set { SetProperty(ref _heightColor, value); }
        }

        private Color _weightColor;
        public Color WeightColor
        {
            get { return _weightColor; }
            set { SetProperty(ref _weightColor, value); }
        }

        private Color _yearClimbingColor;
        public Color YearClimbingColor
        {
            get { return _yearClimbingColor; }
            set { SetProperty(ref _yearClimbingColor, value); }
        }

        private double _filterByHeight;
        public double FilterByHeight
        {
            get { return _filterByHeight; }
            set { SetProperty(ref _filterByHeight, value); }
        }

        private ShowRankingModel _selectedmember;
        public ShowRankingModel SelectedMember
        {
            get { return _selectedmember; }
            set
            {
                _selectedmember = value;
                GoToSeletedMember();
            }
        }
        #endregion
        private async void GoToSeletedMember()
        {
            if (SelectedMember == null)
            {
                return;
            }

            UserDialogs.Instance.ShowLoading("Loading...");
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(NavigationParametersConstants.MemberProfileId, SelectedMember.UserID);
            navigationParameters.Add(NavigationParametersConstants.MemberProfileName, SelectedMember.Name);
            await navigationService.NavigateAsync<MemberProfileViewModel>(navigationParameters);
        }
        public async void OnFilterIconClick()
        {
            var cragData = await cragRepository.FindAsync(crag => crag.crag_id == Settings.ActiveCrag);
            if (cragData != null && !cragData.Unlocked)
            {
                var navigationParameters = new NavigationParameters();

                var guideBook = await guidebookRepository.FindAsync(g => g.GuideBookId == currentCrag.crag_guide_book);
                await navigationService.NavigateAsync<PremiumSubscriptionViewModel>();
                return;
            }

            await BindFilter();
            await PopupNavigation.PushAsync(new RankingFiltersUC(this), true);
        }

        public async Task BindFilter()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Settings.FilterOptions))
                {
                    char[] filterChars = Settings.FilterOptions.ToCharArray();
                    foreach (var filter in filterChars)
                    {
                        switch (filter.ToString())
                        {
                            case "g":
                                IsGenderEnable = IsGenderToggled = true;
                                GenderColor = Color.Black;
                                break;
                            case "a":
                                IsAgeEnable = IsAgeToggled = true;
                                AgeColor = Color.Black;
                                break;
                            case "h":
                                IsHeightEnable = IsHeightToggled = true;
                                HeightColor = Color.Black;
                                break;
                            case "w":
                                IsWeightEnable = IsWeightToggled = true;
                                WeightColor = Color.Black;
                                break;
                            case "y":
                                IsYearsClimbingEnable = IsYearsClimbingToggled = true;
                                YearClimbingColor = Color.Black;
                                break;
                        }
                    }
                }
                //var httpClinetHelper = App.HttpClient;
                //httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_GetUserInfo, Settings.AccessToken);
                //var response = await httpClinetHelper.Get<EditProfileModel>(null);
                var response = await userInfoRepository.GetAsync(Convert.ToInt32(Settings.UserID));
                if (response != null)
                {
                    IsWarningVisible = false;
                    Settings.UserID = response.UserID;

                    #region Bind Gender
                    if (!string.IsNullOrWhiteSpace(response.Gender))
                    {
                        IsGenderEnable = true;
                        GenderColor = Color.Black;
                        Gender = "(" + response.Gender + ")";
                    }
                    else
                    {
                        IsGenderEnable = false;
                        GenderColor = Color.Red;
                        Gender = String.Empty;
                        IsWarningVisible = true;
                    }
                    #endregion

                    #region Bind Age
                    string ageGroup = String.Empty;
                    if (response.DOB.HasValue)
                    {
                        int age = DateTime.Now.Year - response.DOB.Value.Year;
                        string ageFilter = "((" + age + " >= min) and (" + age + " < max) and type = 'a') ";
                        string filterSQL = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + ageFilter;
                        ageGroup = await profileFilterTable.ExecuteScalarAsync<string>(filterSQL);
                    }

                    if (!string.IsNullOrWhiteSpace(ageGroup))
                    {
                        IsAgeEnable = true;
                        AgeColor = Color.Black;
                        Age = "(" + ageGroup + ")";
                    }
                    else
                    {
                        IsAgeEnable = false;
                        AgeColor = Color.Red;
                        Age = String.Empty;
                        IsWarningVisible = true;
                    }
                    #endregion

                    #region Bind Height
                    string heightGroup = String.Empty;
                    if (response.Height.HasValue)
                    {
                        int userHeight = Convert.ToInt32(response.Height.Value);
                        if (response.height_uom == "in")
                            userHeight = Convert.ToInt32(userHeight * 0.393);
                        string heightFilter = "((" + userHeight + " >= min) and (" + userHeight + " < max) and type = 'h' and Trim(uom) = '" + response.height_uom + "') ";
                        string heightFilterSql = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + heightFilter;
                        heightGroup = await profileFilterTable.ExecuteScalarAsync<string>(heightFilterSql);
                    }
                    if (!string.IsNullOrWhiteSpace(heightGroup))
                    {
                        IsHeightEnable = true;
                        HeightColor = Color.Black;
                        Height = "(" + heightGroup + ")";
                    }
                    else
                    {
                        IsHeightEnable = false;
                        HeightColor = Color.Red;
                        Height = String.Empty;
                        IsWarningVisible = true;
                    }
                    #endregion

                    #region Bind Weight
                    string weightGroup = String.Empty;
                    if (response.Weight.HasValue)
                    {
                        int userWeight = Convert.ToInt32(response.Weight.Value);
                        if (response.weight_uom == "lbs")
                            userWeight = Convert.ToInt32(userWeight * 2.20462);
                        string weightFilter = "((" + userWeight + " >= min) and (" + userWeight + " < max) and type = 'w' and Trim(uom) = '" + response.weight_uom + "') ";
                        string weightFilterSql = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + weightFilter;
                        weightGroup = await profileFilterTable.ExecuteScalarAsync<string>(weightFilterSql);
                    }
                    if (!string.IsNullOrWhiteSpace(weightGroup))
                    {
                        IsWeightEnable = true;
                        WeightColor = Color.Black;
                        Weight = "(" + weightGroup + ")";
                    }
                    else
                    {
                        IsWeightEnable = false;
                        WeightColor = Color.Red;
                        Weight = String.Empty;
                        IsWarningVisible = true;
                    }
                    #endregion

                    #region Bind Year Climb
                    string yearClimbGroup = String.Empty;
                    if (response.FirstYearClimb.HasValue)
                    {
                        int year = DateTime.Now.Year - response.FirstYearClimb.Value;
                        string yearClimbFilter = "((" + year + " >= min) and (" + year + " < max) and type = 'y') ";
                        string yearClimbFilterSql = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + yearClimbFilter;
                        yearClimbGroup = await profileFilterTable.ExecuteScalarAsync<string>(yearClimbFilterSql);
                    }
                    if (!string.IsNullOrWhiteSpace(yearClimbGroup))
                    {
                        IsYearsClimbingEnable = true;
                        YearClimbingColor = Color.Black;
                        YearsClimbing = "(" + yearClimbGroup + ")";
                    }
                    else
                    {
                        IsYearsClimbingEnable = false;
                        YearClimbingColor = Color.Red;
                        YearsClimbing = String.Empty;
                        IsWarningVisible = true;
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.BindFilter),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        private async void ExecuteOnEditProfile()
		{
            if (PopupNavigation.PopupStack.Count > 0)
			    await PopupNavigation.PopAllAsync();
			await navigationService.NavigateAsync<MyPreferencesViewModel>();
		}

        public async void OnPagePrepration(int Id)
        {
            userId = Id;
            if (!GeneralHelper.IsCragsDownloaded)
                return;

            var filter = Settings.FilterOptions;
            try
            {
                var cragid = Settings.ActiveCrag;
                var item = await cragRepository.FindAsync(c => c.crag_id == cragid);
                currentCrag = item;
               
                if (userId != Settings.UserID)
                    filter = "";

               // PageSubHeaderText = "Rankings";
                CragName = currentCrag?.crag_name;
                AreaName = currentCrag?.area_name;

                //TODO: Optimise, http requests are called in each method call
                //why are we calling it 3 times? We may just get all items once and the sort them by groups
                if (!GuestHelper.IsGuest)
                {
                    await InvokeServiceGetAscentDates("a", filter);
                    await InvokeServiceGetAscentDates("m", filter);
                    await InvokeServiceGetAscentDates("y", filter);
                }

                Rankings = YearlyRankings;

                if (YearlyRankings.Count > 0)
                {
                    IsHeaderVisible = true;
                    IsStatusVisible = false;
                }
                else
                {
                    IsHeaderVisible = false;
                    IsStatusVisible = true;
                }

                TodayGaugeColor = MonthlyGaugeColor = YearlyGaugeColor = Color.FromHex("#FF8E2D");
				YearlyGaugeOpacity= 1;
                AllGaugeOpacity = MonthlyGaugeOpacity = 0.5;

                BindFilterByLabel();
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnPagePrepration),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"currentCrag = {Newtonsoft.Json.JsonConvert.SerializeObject(currentCrag)}, filter = {Newtonsoft.Json.JsonConvert.SerializeObject(filter)}"
				});
			}
        }

        private async Task InvokeServiceGetAscentDates(string typeId, string filter)
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    var response = await httpHelper.GetAsync<List<RankingModel>>(ApiUrls.Url_M_GetRankings(typeId, Settings.ActiveCrag, filter, userId));

                    if (response.ValidateResponse())
                    {
                        var result = response.Result;

                        if (result.Any())
                        {                            
							if (typeId.Equals("a"))
                                AllRankings = AllRankingAscentDates(result);
                            else if (typeId.Equals("m"))
                                MonthlyRankings = MonthlyRankingAscentDates(result);
                            else if (typeId.Equals("y"))
                                YearlyRankings = YearlyRankingAscentDates(result);
                        }
                    }
                }
                else
                {
                    var toastMessage = new ToastConfig("No Internet Connection");
                }
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.InvokeServiceGetAscentDates),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"typeId = {Newtonsoft.Json.JsonConvert.SerializeObject(typeId)}, filter = {filter}"
				});
			}
        }

        private ObservableCollection<ShowRankingModel> AllRankingAscentDates(List<RankingModel> response)
        {
            var rank = new ObservableCollection<ShowRankingModel>();
            for (int i = 0; i < response.Count; i++)
            {
                if (response[i].logged_in_user)
                {
                    AllUserRank = i + 1;
                    AllRankPercentage = (1 - (((double)AllUserRank / response.Count) - ((double)1 / response.Count))) * 100;
                    rank.Add(new ShowRankingModel()
                    {
                        Rank = i + 1,
                        Name = response[i].user_display_name,
                        UserID = response[i].user_id,
                        Points = response[i].points,
                        HighlightTextColor = Color.FromHex("#FF8E2D")
                    });
                }
                else
                {
                    rank.Add(new ShowRankingModel()
                    {
                        Rank = i + 1,
                        Name = response[i].user_display_name,
                        UserID = response[i].user_id,
                        Points = response[i].points,
                        HighlightTextColor = Color.White
                    });
                }
            }

            return rank;
        }

        private ObservableCollection<ShowRankingModel> MonthlyRankingAscentDates(List<RankingModel> response)
        {
            var rank = new ObservableCollection<ShowRankingModel>();
            for (int i = 0; i < response.Count; i++)
            {
                if (response[i].logged_in_user)
                {
                    MonthlyUserRank = i + 1;
                    MonthlyRankPercentage = (1 - (((double)MonthlyUserRank / response.Count) - ((double)1 / response.Count))) * 100;
                    rank.Add(new ShowRankingModel()
                    {
                        Rank = i + 1,
                        Name = response[i].user_display_name,
                        UserID = response[i].user_id,
                        Points = response[i].points,
                        HighlightTextColor = Color.FromHex("#FF8E2D")
                    });
                }
                else
                {
                    rank.Add(new ShowRankingModel()
                    {
                        Rank = i + 1,
                        Name = response[i].user_display_name,
                        UserID = response[i].user_id,
                        Points = response[i].points,
                        HighlightTextColor = Color.White
                    });
                }
            }

            return rank;
        }

        private ObservableCollection<ShowRankingModel> YearlyRankingAscentDates(List<RankingModel> response)
        {
            var rank = new ObservableCollection<ShowRankingModel>();
            for (int i = 0; i < response.Count; i++)
            {
                if (response[i].logged_in_user)
                {
                    YearlyUserRank = i + 1;
                    YearlyRankPercentage = (1 - (((double)YearlyUserRank / response.Count) - ((double)1 / response.Count))) * 100;
                    rank.Add(new ShowRankingModel()
                    {
                        Rank = i + 1,
                        Name = response[i].user_display_name,
                        UserID = response[i].user_id,
                        Points = response[i].points,
                        HighlightTextColor = Color.FromHex("#FF8E2D")
                    });
                }
                else
                {
                    rank.Add(new ShowRankingModel()
                    {
                        Rank = i + 1,
                        Name = response[i].user_display_name,
                        UserID = response[i].user_id,
                        Points = response[i].points,
                        HighlightTextColor = Color.White
                    });
                }
            }

            return rank;
        }

        private async void OnApplyFilterAsync(object obj)
        {
            try
            {
                string filter = String.Empty;
                if (IsGenderToggled)
                    filter = "g";
                if (IsAgeToggled)
                    filter += "a";
                if (IsHeightToggled)
                    filter += "h";
                if (IsWeightToggled)
                    filter += "w";
                if (IsYearsClimbingToggled)
                    filter += "y";

                Settings.FilterOptions = filter;
                OnPagePrepration(userId);
                BindFilterByLabel();
                if (PopupNavigation.PopupStack.Count > 0)
                    await PopupNavigation.PopAllAsync();
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnApplyFilterAsync),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
				});
			}
        }

        private async void OnAllGaugeTappedAsync(object obj)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading...");
                await Task.Delay(500);

                Rankings = AllRankings;

                if (AllRankings.Count > 0)
                {
                    IsStatusVisible = false;
                    IsHeaderVisible = true;
                }
                else
                {
                    IsHeaderVisible = false;
                    IsStatusVisible = true;
                }

                AllGaugeOpacity = 1;
                MonthlyGaugeOpacity = YearlyGaugeOpacity = 0.5;
                UserDialogs.Instance.Loading().Hide();
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnAllGaugeTappedAsync),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
				});
			}
        }

        private async void OnMonthlyGaugeTappedAsync(object obj)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading...");
                await Task.Delay(500);
                Rankings = MonthlyRankings;
                if (MonthlyRankings.Count > 0)
                {
                    IsHeaderVisible = true;
                    IsStatusVisible = false;
                }
                else
                {
                    IsHeaderVisible = false;
                    IsStatusVisible = true;
                }

                MonthlyGaugeOpacity = 1;
                AllGaugeOpacity = YearlyGaugeOpacity = 0.5;
                UserDialogs.Instance.Loading().Hide();
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnMonthlyGaugeTappedAsync),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
				});
			}
        }

        private async void OnYearlyGaugeTappedAsync(object obj)
        {
            try
            {
                Acr.UserDialogs.UserDialogs.Instance.ShowLoading("Loading...");
                await Task.Delay(500);

                Rankings = YearlyRankings;

                if (YearlyRankings.Count > 0)
                {
                    IsHeaderVisible = true;
                    IsStatusVisible = false;
                }
                else
                {
                    IsHeaderVisible = false;
                    IsStatusVisible = true;
                }

                YearlyGaugeOpacity = 1;
                MonthlyGaugeOpacity = AllGaugeOpacity = 0.5;
                Acr.UserDialogs.UserDialogs.Instance.Loading().Hide();
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnYearlyGaugeTappedAsync),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
				});
			}
        }

        private void BindFilterByLabel()
        {
            string filterby = String.Empty;
            if (IsGenderToggled)
            {
                string gender = Gender;
                var g = gender.Trim('(').Trim(')');
                filterby += ", " + g;
            }
            if (IsAgeToggled)
            {
                string age = Age;
                var a = age.Trim('(').Trim(')'); 
                filterby += ", " + a + "yrs old";
            }
            if (IsHeightToggled)
            {
                string height = Height;
                var h = height.Trim('(').Trim(')');
                filterby += ", " + h;
            }
            if (IsWeightToggled)
            {
                string weight = Weight;
                var w = weight.Trim('(').Trim(')');
                filterby += ", " + w;
            }
            if (IsYearsClimbingToggled)
            {
                string yearsClimbing = YearsClimbing;
                var y = yearsClimbing.Trim('(').Trim(')');
                filterby += ", " + y + "climbing";
            }
            if (!string.IsNullOrWhiteSpace(filterby))
            {
                string filter = filterby.Remove(0, 2);
                FilterByHeight = 15;
                FilteredBy = "Filtered by: " + filter;
            }
            else
            {
                FilterByHeight = 0;
                FilteredBy = String.Empty;
            }
        }
    }
}
