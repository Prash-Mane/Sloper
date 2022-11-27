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
using SloperMobile.UserControls;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.ViewModel.SubscriptionViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ImageSource = Xamarin.Forms.ImageSource;
using System.Diagnostics;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.Views.CragPages;
using SloperMobile.Model.CragModels;
using SloperMobile.ViewModel.GuideBookViewModels;
using System.Collections.ObjectModel;
using SloperMobile.Common.Enumerators;
using Plugin.Connectivity;
using WeatherModel;
using SloperMobile.ViewModel.SectorViewModels;
using System.Text;
using Microsoft.AppCenter.Analytics;

namespace SloperMobile.ViewModel
{
    //TODO: refactor
    public class CragDetailsViewModel : BaseViewModel
    {
        private readonly IRepository<TechGradeTable> techGradeRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<CragImageTable> cragImageRepository;
        private readonly IRepository<CragSectorMapTable> cragSectorMapRepository;
        private readonly IRepository<GradeTable> gradeRepository;
        private readonly IRepository<BucketTable> bucketRepository;
        private readonly IRepository<IssueTable> tissueRepository;
        private readonly IRepository<AppProductTable> appProductRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<TrailTable> trailRepository;
        private readonly IRepository<MapTable> mapRepository;
        private readonly IUserDialogs userDialogs;
        private readonly IDownloadCragService downloadCragService;
        private readonly IPurchasedCheckService purchasedCheckService;
        private readonly IDownloadManager downloadManager;

        private CragExtended currentCrag;
        private IList<Color> barColors;
        private int selectedCragId;
        private ImageSource cragImage;
        private string _creditValue;
        private string _captionValue;
        private List<RepeaterItem> repeaterItems = new List<RepeaterItem>();

        private int progressCounter = 0;
        private int TotalItemsForRemove = 11;
        private ProgressPopup progressPopup;

        private string crag_name;
		private string guideButtonText;
		ObservableCollection<CragMapModel> cragMaps = new ObservableCollection<CragMapModel>();


		private GuideBookTable guideBook;

        public CragDetailsViewModel(
            INavigationService navigationService,
            IRepository<TechGradeTable> techGradeRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<CragImageTable> cragImageRepository,
            IRepository<MapTable> mapRepository,
			IRepository<CragSectorMapTable> cragSectorMapRepository,
			IRepository<GradeTable> gradeRepository,
			IRepository<BucketTable> bucketRepository,
			IRepository<IssueTable> tissueRepository,
			IRepository<AppProductTable> appProductRepository,
			IRepository<GuideBookTable> guideBookRepository,
			IRepository<UserInfoTable> userInfoRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IExceptionSynchronizationManager exceptionManager,
			IUserDialogs userDialogs,
            IHttpHelper httpHelper,
            IDownloadCragService downloadCragService,
            IPurchasedCheckService purchasedCheckService,
            IDownloadManager downloadManager)
            : base(navigationService, exceptionManager, httpHelper)
        {
            this.techGradeRepository = techGradeRepository;
            this.cragRepository = cragRepository;
            this.routeRepository = routeRepository;
            this.appSettingsRepository = appSettingsRepository;
            this.topoRepository = topoRepository;
            this.cragImageRepository = cragImageRepository;
            this.cragSectorMapRepository = cragSectorMapRepository;
            this.mapRepository = mapRepository;
			this.gradeRepository = gradeRepository;
			this.bucketRepository = bucketRepository;
			this.tissueRepository = tissueRepository;
			this.appProductRepository = appProductRepository;
			this.guideBookRepository = guideBookRepository;
			this.userInfoRepository = userInfoRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
			this.userDialogs = userDialogs;
            this.downloadCragService = downloadCragService;
            this.purchasedCheckService = purchasedCheckService;
            this.downloadManager = downloadManager;

            barColors = new List<Color>();
            graphData = new List<CragDetailModel>();

            GeneralHelper.AppOrGuidebookPurchased += OnAppOrGuidebookPurchased;
            GeneralHelper.CragModified += OnCragModified;
            downloadManager.PopupProgressChanged += DMProgressChanged;

			IsBackButtonVisible = true;
            HasFade = true;
            IsShowFooter = true;
            Offset = Offsets.Footer;
        }

        public string CaptionValue
        {
            get => _captionValue;
            set
            {
                SetProperty(ref _captionValue, value);
                RaisePropertyChanged(nameof(IsCameraIconVisible));
            }
        }

        public string CreditValue
        {
            get => _creditValue;
            set
            {
                SetProperty(ref _creditValue, value);
                RaisePropertyChanged(nameof(IsCameraIconVisible));
            }
        }

        public bool IsCameraIconVisible => !string.IsNullOrEmpty(CaptionValue) && !string.IsNullOrEmpty(CreditValue);

        public bool SearchVisible => currentCrag != null && currentCrag.is_downloaded;

        public IList<Color> BarColors
        {
            get
            {
                return barColors;
            }
            set
            {
                SetProperty(ref barColors, value);
            }
        }

        public List<RepeaterItem> RepeaterItems
        {
            get => repeaterItems;
            set => SetProperty(ref repeaterItems, value);
        }

        public ICommand OnCameraIconClickCommand => new Command(OnCameraIconClick);

        public ICommand DownloadCommand => new Command(async () => await OnDownload());

		public ICommand RemoveGuideCommand => new Command(async () => await OnRemoveGuide());

        public ICommand CancelRemoveCommand => new Command(ClosePopup);

        public ICommand SearchCommand => new Command(OnSearchCommand);

        public ICommand GuideButtonCommand
		{
			get
			{
				return new Command<string>(OnClickGuideButton);
			}
		}
		private void OnClickGuideButton(string param)
		{
           	if (param == "DOWNLOAD GUIDE")
				DownloadGuide();
			else
				NavigateToSectors();

		}

        private async void OnCameraIconClick()
        {
            await PopupNavigation.PushAsync(new CragSummaryCameraPage(this), true);
        }

        public Aspect CragImageAspect
        {
            get
            {
                return cragImageAspect;
            }
            set
            {
                cragImageAspect = value;
                RaisePropertyChanged();
            }
        }

		private Aspect cragImageAspect = Aspect.AspectFill;

        public ImageSource CragImage
        {
            get { return cragImage; }
            set { SetProperty(ref cragImage, value); }
        }

        private string _cragdesc;
        public string CragDesc
        {
            get { return _cragdesc; }
            set { _cragdesc = value; RaisePropertyChanged(); }
        }
        private DataTemplate legendsdata;
        public DataTemplate LegendsDataTemplate
        {
            get { return legendsdata; }
            set { legendsdata = value; RaisePropertyChanged(); }
        }

        private List<CragDetailModel> graphData;
        public List<CragDetailModel> GraphData
        {
            get { return graphData; }
            set { SetProperty(ref graphData, value); }
        }
       	 
        private bool isDisplayRemoveGuideButton;
		public bool IsDisplayRemoveGuideButton
		{
			get { return isDisplayRemoveGuideButton; }
			set { SetProperty(ref isDisplayRemoveGuideButton, value); }
		}
		public string CragName
        {
            get { return crag_name; }
            set { crag_name = value; RaisePropertyChanged(); }
        }
		public string GuideButtonText
		{
			get { return guideButtonText; }
			set { guideButtonText = value; RaisePropertyChanged(); }
		}

		private async Task LoadLegendsBucket()
        {
            try
            {
                LegendsDataTemplate = await DataTemplateHelper.GetBucketsTemplateAsync(selectedCragId, exceptionManager: exceptionManager);
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.LoadLegendsBucket),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        private async Task LoadGraphData()
        {
            try
            {
                int totalbuckets = await bucketRepository.ExecuteScalarAsync<int>("SELECT Count(grade_type_id) As BucketCount FROM T_BUCKET GROUP BY grade_type_id LIMIT 1");
                var barColors = new List<Color>();
                var graphData = new List<CragDetailModel>();
                for (int i = 1; i <= totalbuckets; i++)
                {
                    var obj = new CragDetailModel();
                    var grades = await gradeRepository.GetAsync(grade => grade.crag_id == currentCrag.crag_id && grade.grade_bucket_id == i);
                    var bucketCount = grades.Sum(grade => grade.grade_bucket_id_count);
                    obj.BucketCount = bucketCount.ToString();
                    var barColor = await gradeRepository.ExecuteScalarAsync<string>($"SELECT hex_code FROM T_BUCKET Where grade_bucket_id= {i} Limit 1");
                    barColors.Add(Color.FromHex(string.IsNullOrEmpty(barColor) ? "#cccccc" : barColor));
                    graphData.Add(obj);
                }

                BarColors = barColors;
                GraphData = graphData;
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.LoadGraphData),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

		public override void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
        }

        private async Task UpdateButtons()
		{
            try
            {
                var tempItems = new List<RepeaterItem>();
                var crag = await cragRepository.GetAsync(selectedCragId);

                if (CrossConnectivity.Current.IsConnected)
                {
                    var weatherTempPlaceholder = new RepeaterItem
                    {
                        ImageSourceItems = ImageSource.FromFile("icon_season"),
                        LabelText = "Loading"
                    };
                    tempItems.Add(weatherTempPlaceholder);
                    LoadWeatherAsync(tempItems, crag);
                }

                var routeNames = (await routeRepository.GetAsync(tr => tr.crag_id == selectedCragId && tr.is_enabled)).OrderBy(tr1 => tr1.route_type).Select(tr2 => tr2.route_type).Distinct().Take(2);
                foreach (var routeName in routeNames)
                {
                    var routeItem = new RepeaterItem
                    {
                        ImageSourceItems = ImageSource.FromFile(GetIconNameByRouteTypeName(routeName)),
                        LabelText = routeName.ToUpper()
                    };
                    tempItems.Add(routeItem);
                }

                IsDisplayRemoveGuideButton = crag.is_downloaded;

                if (crag.is_downloaded)
                {
                    GuideButtonText = "VIEW GUIDE";
                    //TODO: Uncomment when ready
                    //var approachItem = new RepeaterItem
                    //{
                    //    ImageSourceItems = ImageSource.FromFile("pedestrian"),
                    //    LabelText = "APPROACH",
                    //    CommandToRun = new Command(OnApproachClicked)
                    //    //LabelInfoText = "1-2hrs" //todo: support
                    //};
                    //tempItems.Add(approachItem);

                    var maps = await mapRepository.GetAsync(m => m.crag_id == currentCrag.crag_id && m.is_enabled);
                    if (maps.Count > 0)
                    {
                        var mapItem = new RepeaterItem
                        {
                            ImageSourceItems = ImageSource.FromFile("navMap"),
                            LabelText = "SECTOR\nMAP",
                            CommandToRun = new Command(NavigateToMaps)
                        };
                        tempItems.Add(mapItem);
                    }
                }
                else
                {
                    GuideButtonText = "DOWNLOAD GUIDE";
                }

                RepeaterItems = tempItems;
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.UpdateButtons),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        private string GetIconNameByRouteTypeName(string routetypename)
        {
            if (routetypename == "Bouldering")
                return "icon_route_type_bouldering";

            if (routetypename == "Sport")
                return "icon_route_type_sport";

            if (routetypename == "Traditional" || routetypename== "Mixed" || routetypename == "Mixed Pro" || routetypename == "Trad")
                return "icon_route_type_traditional";

            if (routetypename == "Aid Climb")
                return "icon_route_type_aid";

            return "";
        }

		private async void SetImageSource()
		{
            try
            {
                var cragImage = await cragImageRepository.FindAsync(tci => tci.crag_id == selectedCragId);
                if (cragImage == null
                    || string.IsNullOrEmpty(cragImage?.crag_image))
                {

                    CragImage = Common.Constants.AppSetting.APP_TYPE == "indoor"
                                                    ? ImageSource.FromFile("default_sloper_indoor_portrait")
                                                    : ImageSource.FromFile("default_sloper_outdoor_portrait");

                    return;
                }

                var crageImageString = cragImage?.crag_image;
                if (!string.IsNullOrEmpty(crageImageString))
                {
                    var imageBytes = Convert.FromBase64String(crageImageString?.Split(',')[1]);
                    CragImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                //var crageImagePortraitString = cragImage?.crag_portrait_image;
                //if (!string.IsNullOrEmpty(crageImagePortraitString))
                //{
                //    var imagePortraiteBytes = Convert.FromBase64String(crageImagePortraitString?.Split(',')[1]);
                //    CragImagePortrait = ImageSource.FromStream(() => new MemoryStream(imagePortraiteBytes));
                //}
                else
                {
                    CragImage = Common.Constants.AppSetting.APP_TYPE == "indoor"
                    ? ImageSource.FromFile("default_sloper_indoor_portrait")
                    : ImageSource.FromFile("default_sloper_outdoor_portrait");
                }
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.SetImageSource),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        async Task LoadWeatherAsync(List<RepeaterItem> repeaters, CragExtended crag) 
        {
            try
            {
                //todo: consider adding command
                var userId = Settings.UserID;
                var userInfo = await userInfoRepository.GetAsync(userId);
                if (!Enum.TryParse<Metrics>(userInfo.temperature_uom, out var temperatureUnits))
                    temperatureUnits = Metrics.metric; //metric (Celsius) as default

                var weatherResponse = await httpHelper.GetAsync<Forecast>(ApiUrls.GetWeatherForecast(temperatureUnits, crag.crag_latitude.Value, crag.crag_longitude.Value), "");
                if (weatherResponse.ValidateResponse(false))
                {
                    var imgName = weatherResponse.Result.weather.First().icon;
                    //var imgUrl = new Uri($"http://openweathermap.org/img/w/{imgName}.png");
                    var unitLetter = temperatureUnits == Metrics.imperial ? 'F' : 'C';
                    var weatherItem = new RepeaterItem
                    {
                        ImageSourceItems = ImageSource.FromFile($"icon_openweathermap_{imgName}"),
                        LabelText = weatherResponse.Result.weather.FirstOrDefault()?.main?.ToUpper(),
                        LabelInfoText = $"{(int)weatherResponse.Result.main.temp}°{unitLetter}"
                    };
                    repeaters.RemoveAt(0);
                    repeaters.Insert(0, weatherItem);
                    RepeaterItems = new List<RepeaterItem>(repeaters);
                }
            }
            catch (Exception ex)
            {
                exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.LoadWeatherAsync),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"{ex.Data}"
                });
            }
        }

		public override async void OnNavigatingTo(NavigationParameters parameters)
		{
            var debugData = new StringBuilder();
            try
            {
                base.OnNavigatingTo(parameters);
                if (parameters.Count == 0)
                {
                    return;
                }

                if (currentCrag == null)
                {
                    selectedCragId = (int)parameters["selectedCragId"];
                    debugData.Append($"selectedCragId:{selectedCragId}\t|");
                    currentCrag = await cragRepository.FindAsync(c => c.crag_id == selectedCragId);
                    debugData.Append($"currentCrag: {JsonConvert.SerializeObject(currentCrag)}\t|");
                }

                var dict = new Dictionary<string, string>() { { "Crag", $"id: {currentCrag.crag_id}, name: {currentCrag.crag_name}" } };
                Analytics.TrackEvent(GetType().Name.TruncateVMName(), dict);

                RaisePropertyChanged(nameof(SearchVisible));
                PageHeaderText = currentCrag.crag_name;
                PageSubHeaderText = currentCrag.area_name;
                debugData.Append("area loaded\t|");
                CaptionValue = currentCrag.photo_caption;
                CreditValue = currentCrag.photo_credit;
                CragName = currentCrag.crag_name;
                CragDesc = currentCrag.crag_general_info;

                if (!String.IsNullOrEmpty(CreditValue))
                {
                    CreditValue = CreditValue.ToUpper();
                }

                if (!String.IsNullOrEmpty(CragName))
                {
                    CragName = CragName.ToUpper();
                }

                await UpdateButtons();
                await LoadLegendsBucket();
                await LoadGraphData();

                SetImageSource();
                debugData.AppendLine("loading guidebook\t|");
                guideBook = await guideBookRepository.FindAsync(guide => guide.GuideBookId == currentCrag.crag_guide_book); //why do we need it?
                debugData.AppendLine("guidebook loaded");
                userDialogs.HideLoading();
            }
            catch (Exception exceptioin)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = exceptioin.StackTrace,
                    Exception = exceptioin.Message,
                    Data = debugData.ToString()
                });
            }
        }
		private async Task OnRemoveGuide()
		{
			await PopupNavigation.PushAsync(new RemoveCragPopup(this), true);
		}


		private async Task OnDownload()
        {
            if (!Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                //Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.ShowError("Downloading requires an internet connection."));
                Device.BeginInvokeOnMainThread(() =>
                {
                    userDialogs.HideLoading();
                    userDialogs.AlertAsync("Downloading Requires an Internet Connection.", "Network Error", "Continue");
                });

                return;
            }

            progressCounter = 0;

            var isFreeDownload = !currentCrag.Unlocked && Settings.AvailableFreeCrags > 0;

            progressPopup = new ProgressPopup();
            progressPopup.PopOnFinish = false;
            progressPopup.BackgroundImageSource = CragImage;

            if (isFreeDownload)
            {
                var userId = Settings.UserID;
                var numberOfFreeCrags = (await userInfoRepository.GetAsync(userId)).NumberOfFreeCrags;
                progressPopup.MessageTitle = $"Downloading\nFree Premium Crag\n({numberOfFreeCrags - Settings.AvailableFreeCrags + 1} of {numberOfFreeCrags})";
            }
            else
            {
                //progressPopup.MessageTitle = crag_name;
            }

            downloadCragService.ProgressChanged += (s, e) => progressPopup.Report(e);
            await PopupNavigation.PushAsync(progressPopup, false);

            var isDownloaded = await downloadCragService.DownloadAsync(selectedCragId);

            if (!isDownloaded)
            {
                ClosePopup();
                userDialogs.HideLoading();
                await userDialogs.AlertAsync("Error downloading crag, please try again!");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            if (isFreeDownload)
            {
                var freeCragsList = Settings.FreeCragIds;
                freeCragsList.Add(selectedCragId);
                Settings.FreeCragIds = freeCragsList;
                Settings.AvailableFreeCrags--;

                //httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_UpdateUserInfo, Settings.AccessToken);
                var blobledList = JsonConvert.SerializeObject(freeCragsList);
                //await httpClinetHelper.Post<UserInfoTable>(JsonConvert.SerializeObject(new { free_downloaded_crag_ids = blobledList }));
                var response = await httpHelper.PostAsync<UserInfoTable>(ApiUrls.Url_SloperUser_UpdateUserInfo, new { free_downloaded_crag_ids = blobledList });
                if (response.ValidateResponse())
                {
                    currentCrag.Unlocked = true;
                    currentCrag.is_downloaded = true;
                    await cragRepository.UpdateAsync(currentCrag);
                }
                GeneralHelper.CragModified?.Invoke(this, currentCrag);
            }

            //let's wait atleast 2 sec before popup closes
            var waitTimeMs = (int)(2000 - stopwatch.ElapsedMilliseconds);
            if (waitTimeMs > 0)
                await Task.Delay(waitTimeMs);

            ClosePopup();
		}

        public ICommand RemoveCommand => new Command(OnRemove);

        private async void OnRemove()
        {
            if (!Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    userDialogs.HideLoading();
                    userDialogs.AlertAsync("Removing Requires an Internet Connection.", "Network Error", "Continue");
                });
                return;
            }

            progressCounter = 0;

            progressPopup = new ProgressPopup
            {
                InverseProgress = true
            };

            progressPopup.PopOnFinish = false;

            //progressPopup.BackgroundImageSource = await MapModelHelper.GetCragImageAsync(cragImageRepository, false);
            progressPopup.BackgroundImageSource = CragImage;
            await PopupNavigation.PushAsync(progressPopup, false);

            UpdateProgress("Removing\nRoute Issues");
            //delete issues first because we can't joint to deleted routes
            await tissueRepository.ExecuteAsync($@"DELETE FROM TISSUE WHERE (route_id in (
            SELECT DISTINCT T_ROUTE.route_id FROM T_ROUTE JOIN T_SECTOR ON T_ROUTE.sector_id = T_SECTOR.sector_id
            WHERE T_SECTOR.crag_id = {selectedCragId} ))");

            UpdateProgress("Removing\nTopos");
            await bucketRepository.ExecuteAsync($"DELETE FROM T_TOPO WHERE sector_id in (SELECT DISTINCT sector_id FROM T_SECTOR WHERE crag_id= {selectedCragId} )");

            UpdateProgress("Removing\nParking Lots");
            await parkingRepository.RemoveAsync(p => p.crag_id == selectedCragId);
            await trailRepository.RemoveAsync(t => t.crag_id == selectedCragId);
           
            UpdateProgress("Removing\nMaps");
            await mapRepository.RemoveAsync(m => m.crag_id == selectedCragId);

            UpdateProgress("Removing\nCrag");
            await cragSectorMapRepository.RemoveAsync(mr => mr.crag_id == selectedCragId);

			currentCrag.is_downloaded = false; 			
			await cragRepository.UpdateAsync(currentCrag);

            var downloadedCrag = (await cragRepository.GetAsync(c => c.is_enabled
                                                            && c.is_app_store_ready
                                                            && c.is_downloaded
                                                            && c.crag_latitude != default(float)
                                                            && c.crag_longitude != default(float))
                              ).OrderBy(crag => crag.crag_sort_order)
                               .FirstOrDefault();
            Settings.MapSelectedCrag = Settings.ActiveCrag = downloadedCrag?.crag_id ?? 0;

            UpdateProgress("Removing\nCrag News");
            var roleRes = await HttpRemoveCragRole(currentCrag.crag_role_id);
            if (!roleRes.ValidateResponse())
                return;

            UpdateProgress("Updating\nDownload Status");
            var historyRes = await HttpRemoveUserCragHistory();
            if (!historyRes.ValidateResponse())
                return;

            UpdateProgress("Updating\nGrades");
            var gradesSuccess = await downloadCragService.GetAndSaveGradesAsync();
            if (!gradesSuccess)
                return;
           
            UpdateProgress("Updating\nGrade Buckets");
            var bucketsSuccess = await downloadCragService.GetAndSaveBucketsAsync();
            if (!bucketsSuccess)
                return;
           
            UpdateProgress("Updating\nTech Grades");
            var techGradesSuccess = await downloadCragService.GetAndSaveTechGradesAsync();
            if (!techGradesSuccess)
                return;

            if (TrailCollector.Instance.ActiveRecord?.crag_id == selectedCragId)
            {
                try
                {
                    await TrailCollector.Instance.CancelCurrentRecordingAsync();
                }
                catch { }
            }
      
            UpdateProgress("Crag Removal\nFinished!");

            GeneralHelper.CragModified?.Invoke(this, currentCrag);
            await GeneralHelper.UpdateCragsDownloadedStateAsync();

            //MainMasterDetailViewModel.Instance?.FillMenuItems();

            await Task.Delay(2000);

            ClosePopup();
        }

        public async void NavigateToMaps()
		{
			Settings.ActiveCrag = currentCrag.crag_id;
            await navigationService.NavigateAsync<SectorMapListViewModel>();
		}

			public async void NavigateToSectors()
		{
			Settings.ActiveCrag = currentCrag.crag_id;
			await navigationService.NavigateAsync<CragSectorsViewModel>();

		}

		public async void DownloadGuide()
		{
            try
            {
                var dmCrag = downloadManager.DownloadingQueue.FirstOrDefault(c => c.CragID == selectedCragId);
                if (dmCrag != null)
                {
                    if (dmCrag.State == CragInfoModel.CragStatus.DownloadQueued)
                    {
                        var res = await userDialogs.ConfirmAsync("This crag is already in download queue.\nWould you like to view MyDownloads?", "", "Yes,take me there.", "Cancel");
                        if (res)
                        {
                            await navigationService.NavigateAsync<ManageDownloadsViewModel>();
                        }
                    }
                    if (dmCrag.State == CragInfoModel.CragStatus.Downloading)
                    {
                        progressPopup = new ProgressPopup();
                        progressPopup.PopOnFinish = false;
                        progressPopup.BackgroundImageSource = CragImage;
                        progressPopup.Report((dmCrag.ProgressStatus, dmCrag.ProgressValue));
                        await PopupNavigation.PushAsync(progressPopup, false);
                    }
                }
                else
                {
                    if (!currentCrag.Unlocked && Settings.AvailableFreeCrags <= 0)
                    {
                        await navigationService.NavigateAsync<PremiumSubscriptionViewModel>();
                    }
                    else
                    {
                        ClosePopup();
                        await OnDownload();
                    }
                }
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.DownloadGuide),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                });
            }
        }

		private async Task<OperationResult<CragHistoryModel>> HttpRemoveUserCragHistory()
            => await httpHelper.GetAsync<CragHistoryModel>(ApiUrls.Url_M_RemoveUserCragHistory(selectedCragId));

        private async Task<OperationResult<string>> HttpRemoveCragRole(int? crag_role_id)
            => await httpHelper.PostAsync<string>(ApiUrls.Url_SloperUser_RemoveRole(crag_role_id), string.Empty);

        void UpdateProgress(string message, int increment = 1)
        {
            progressCounter += increment;

            var progress = Decimal.Divide(progressCounter, TotalItemsForRemove);

            progressPopup.Report((message, progress));
        }

        public override void Destroy()
        {
            base.Destroy();
            GeneralHelper.AppOrGuidebookPurchased -= OnAppOrGuidebookPurchased;
            GeneralHelper.CragModified -= OnCragModified;
            downloadManager.PopupProgressChanged -= DMProgressChanged;
        }

        async void OnAppOrGuidebookPurchased(object sender, AppProductTable purchase)
        {
            currentCrag = await cragRepository.FindAsync(c => c.crag_id == selectedCragId);

            await OnDownload();

            await UpdateButtons();
        }

        async void OnCragModified(object sender, CragExtended crag)
        {
            await UpdateButtons();

            if (crag.crag_id == selectedCragId && sender != this)
            {
                if (sender is SubscriptionViewModel)
                    await OnDownload();

                currentCrag = await cragRepository.FindAsync(c => c.crag_id == selectedCragId);
                RaisePropertyChanged(nameof(SearchVisible));
                await Task.Delay(2000);
                ClosePopup();
            }
        }

        void OnSearchCommand(object obj)
        {
            //if (!currentCrag.is_downloaded)
            //{
            //    userDialogs.Alert("You have to download the crag to use search");
            //    return;
            //}

            var navParams = new NavigationParameters { { "CragId", selectedCragId }  };
            navigationService.NavigateAsync<SearchViewModel>(navParams);
        }

        private void ClosePopup()
        {
            if (PopupNavigation.PopupStack.Count > 0)
                PopupNavigation.PopAllAsync(false);
        }

        async void OnApproachClicked() 
        {
            if (TrailCollector.Instance.ActiveRecord != null && TrailCollector.Instance.ActiveRecord.crag_id != currentCrag.crag_id)
            {
                IDisposable alert = null;
                var recordCrag = await cragRepository.GetAsync(TrailCollector.Instance.ActiveRecord.crag_id);
                var config = new ActionSheetConfig { 
                    Title = $"You've got trail recording in progress in other crag ({recordCrag.crag_name})"
                };
                config.Add("View", () => {
                    Settings.ActiveCrag = recordCrag.crag_id;
                    navigationService.NavigateAsync<ApproachMapViewModel>();
                });
                config.Add("Stop Recording", async () => {
                    try
                    {
                        await TrailCollector.Instance.CancelCurrentRecordingAsync();
                    }
                    catch { }
                    Settings.ActiveCrag = currentCrag.crag_id;
                    navigationService.NavigateAsync<ApproachMapViewModel>();
                });
                config.Add("Close", () => alert.Dispose());
                alert = userDialogs.ActionSheet(config);
                return;
            }

            Settings.ActiveCrag = currentCrag.crag_id;
            navigationService.NavigateAsync<ApproachMapViewModel>();
        }

        void DMProgressChanged(object sender, (string message, decimal progress) e)
        {
            var crag = sender as CragExtended;

            if (progressPopup != null && crag?.crag_id == selectedCragId)
                progressPopup.Report(e);
        }
    }
}