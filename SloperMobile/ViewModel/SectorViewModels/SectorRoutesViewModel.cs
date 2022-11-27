using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.NotifyProperties;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using System.Diagnostics;
using Syncfusion.DataSource.Extensions;
using Microsoft.AppCenter.Analytics;

namespace SloperMobile.ViewModel.SectorViewModels
{
    public class SectorRoutesViewModel : BaseViewModel
	{
		private readonly IRepository<SectorTable> sectorRepository;
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IRepository<CragImageTable> cragImageRepository;
		private readonly IRepository<BucketTable> bucketRepository;
		private readonly IRepository<TopoTable> topoRepository;
		private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<AppProductTable> appProductRepository;
		private readonly IRepository<UserInfoTable> userInfoRepository;
		private readonly IInAppPurchase inAppPurchase;
        private readonly IUserDialogs userDialogs;
		private readonly IPurchasedCheckService purchasedCheckService;

		private ObservableCollection<RouteDataModel> routes;
		private IOrderedEnumerable<RouteTable> routeTables;

		private ImageSource sectorImage;
		private ObservableCollectionFast<RouteDataModel> routeData;
		private MapListModel currentSector;
		private CragTable currentCrag;
		private bool isListEnabled;
		private bool wasPageLoaded;
		private int legengthHeight = 0;
        private int isCragOrDefaultImageCount = 0;
        private bool isNavigatingToTopos;
        private IList<int> filteredBucketIds;
      
        public SectorRoutesViewModel(
            INavigationService navigationService,
            IRepository<SectorTable> sectorRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<CragImageTable> cragImageRepository,
            IRepository<BucketTable> bucketRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<AppProductTable> appProductRepository,
			IRepository<UserInfoTable> userInfoRepository,
			IExceptionSynchronizationManager exceptionManager,
			IInAppPurchase inAppPurchase,
            IUserDialogs userDialogs,
			IPurchasedCheckService purchasedCheckService) : base(navigationService, exceptionManager)
		{
			this.sectorRepository = sectorRepository;
			this.cragRepository = cragRepository;
			this.cragImageRepository = cragImageRepository;
			this.bucketRepository = bucketRepository;
			this.topoRepository = topoRepository;
			this.routeRepository = routeRepository;
            this.appProductRepository = appProductRepository;
			this.userInfoRepository = userInfoRepository;
			this.inAppPurchase = inAppPurchase;
            this.userDialogs = userDialogs;
			this.purchasedCheckService = purchasedCheckService;

			TapSectorCommand = new Command(TapOnSectorImage);
            routeData = new ObservableCollectionFast<RouteDataModel>();
			//IsShowFooter = (GeneralHelper.IsCragsDownloaded && IsMenuVisible);
			IsBackButtonVisible = true;
            HasFade = true;    
        }

        private List<BucketsSegmentModel> legendsDataSource;
        public List<BucketsSegmentModel> LegendsDataSource
        {
            get { return legendsDataSource ?? new List<BucketsSegmentModel>(); }
            set { SetProperty(ref legendsDataSource, value); }
        }

        public ICommand FilterCommand => new Command<BucketsSegmentModel>((b) => ApplyFilter());

        public ObservableCollection<RouteDataModel> Routes
        {
            get { return routes; }
            set { SetProperty(ref routes, value); }
        }

        public int IsCragOrDefaultImageCount {
            get { return isCragOrDefaultImageCount; }
            set { SetProperty(ref isCragOrDefaultImageCount, value); }
        }

        public ImageSource SectorImage
		{
			get { return sectorImage; }
			set { SetProperty(ref sectorImage, value); }
		}

		public MapListModel CurrentSector
		{
			get { return currentSector; }
			set { SetProperty(ref currentSector, value); }
		}

		public int LegendsHeight
		{
			get { return legengthHeight; }
			set { SetProperty(ref legengthHeight, value); }
		}

		public bool IsNavigatingToTopos
		{
			get { return isNavigatingToTopos; }
			set { SetProperty(ref isNavigatingToTopos, value);	}
		}

        public bool ShowEmptyOverlay => wasPageLoaded && (!Routes?.Any() ?? false);

        public void UpdateActivityIndicator(bool isNavigationToTopos)
		{
			if (isNavigationToTopos == this.isNavigatingToTopos)
			{
				return;
			}

			IsNavigatingToTopos = isNavigationToTopos;
		}

		public ICommand TapSectorCommand { get; set; }

		private async void TapOnSectorImage()
		{
			IsNavigatingToTopos = true;
            Cache.IsGlobalRouteId = true;
            Cache.IsTapOnSectorImage = true;
            try
			{
				bool navigate = false;
				var topolistData = await topoRepository.FindAsync(sec => sec.sector_id == (CurrentSector.SectorId));
				navigate = topolistData != null && topolistData.topo_json != "[]";
				if (!navigate)
				{
					Device.BeginInvokeOnMainThread(() =>
						IsNavigatingToTopos = false);
                    userDialogs.Toast(new ToastConfig("")
                    {
                        Message = " No Topos Found!",
                        BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                        MessageTextColor = System.Drawing.Color.White,
                        Duration = TimeSpan.FromSeconds(3)
                    });

					return;
				}

                var navigationParameters = new NavigationParameters
                {
                    {NavigationParametersConstants.SelectedSectorObjectParameter, CurrentSector},
                    {NavigationParametersConstants.RouteIdParameter, default(int)},
                    {NavigationParametersConstants.SingleRouteParameter, false},
                    {NavigationParametersConstants.NavigatonServiceParameter, navigationService},
                    {
                        NavigationParametersConstants.IsUnlockedParameter, (Settings.AppPurchased || IsCragUnlocked) ? (Settings.AppPurchased || IsCragUnlocked) : currentCrag.Unlocked
                    },
                    {NavigationParametersConstants.GuideBookIdParameter, currentCrag.crag_guide_book},
                    {NavigationParametersConstants.IsNavigatedFromSectorImageParameter, true}
                };
                await navigationService.NavigateAsync<SectorToposViewModel>(navigationParameters);
				IsNavigatingToTopos = false;
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.TapOnSectorImage),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message
				});
			}
		}

		//steepness
		private string GetTopAngleResourceName(string angle)
		{
			string resource = "icon_steepness_1_slab_border";
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

		private string GetTopHoldResourceName(string hold)
		{
			string resource = "icon_hold_type_1_slopers_border";
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
		//route type 
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
                case 3: case 7: case 8:
					resource = "icon_route_type_traditional";
					break;
				case 5:
					resource = "icon_route_type_aid";
					break;
			}
			return resource;
		}

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

	   //star 
		private string GetStarImage(float? rating)
		{
			string resource = "";
            if (rating == null)
                return "icon_star0";

            var ratingRounded = Math.Round(rating.Value, 0, MidpointRounding.AwayFromZero);
			switch (ratingRounded)
			{
				case 0:
					resource = "icon_star0";
					break;
				case 1:
					resource = "icon_star1";
					break;
				case 2:
					resource = "icon_star2";
					break;
				case 3:
					resource = "icon_star3";
					break;
				case 4:
					resource = "icon_star4";
					break;
				case 5:
					resource = "icon_star5";
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

		public ICommand ItemTappedCommand
		{
			get
			{
				return new Command<RouteDataModel>(OnItemTapped);
			}
		}

		public bool IsListEnabled
		{
			get { return isListEnabled; }
			set { SetProperty(ref isListEnabled, value); }
		}

        public bool IsCragUnlocked { get; private set; }

        private async void OnItemTapped(RouteDataModel route)
		{
			try
            {
                Cache.IsGlobalRouteId = false;
                Cache.IsTapOnSectorImage = false;
                IsListEnabled = false;
                var navigationParameters = new NavigationParameters
                {
                    {NavigationParametersConstants.SelectedSectorObjectParameter, CurrentSector},
                    {NavigationParametersConstants.RouteIdParameter, route.RouteId},
                    {NavigationParametersConstants.SingleRouteParameter, true},
                    {
                        NavigationParametersConstants.IsUnlockedParameter,
							(Settings.AppPurchased || IsCragUnlocked) //??
							? (Settings.AppPurchased || IsCragUnlocked)
							: currentCrag.Unlocked
					},
                    {NavigationParametersConstants.GuideBookIdParameter, currentCrag.crag_guide_book},
                    {NavigationParametersConstants.IsNavigatedFromSectorImageParameter, false}
                };
                await navigationService.NavigateAsync<SectorToposViewModel>(navigationParameters);
				IsListEnabled = true;
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnItemTapped),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					//Data = Newtonsoft.Json.JsonConvert.SerializeObject(route)
				});
			}
		}

		public override void OnNavigatedFrom(NavigationParameters parameters)
		{
            base.OnNavigatedFrom(parameters);
        }

		public async override void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
            try
			{
				UserDialogs.Instance.HideLoading();
				IsListEnabled = true;
				App.IsNavigating = false;
			
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatedTo),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"userId = {Settings.UserID}"
				});
			}
		}

		public async override void OnNavigatingTo(NavigationParameters parameters)
		{
            base.OnNavigatingTo(parameters);
            try
			{
                if (wasPageLoaded)
                    return;

				currentCrag = await cragRepository.FindAsync(c => c.crag_id == Settings.ActiveCrag);
                CurrentSector = (MapListModel)parameters[NavigationParametersConstants.SelectedSectorObjectParameter];

                var dict = new Dictionary<string, string>() { { "Sector", $"id: {CurrentSector.SectorId}, name: {CurrentSector.SectorName}" } };
                Analytics.TrackEvent(GetType().Name.TruncateVMName(), dict);

                Cache.SelctedCurrentSector = CurrentSector;
                PageHeaderText = CurrentSector.SectorName;
				PageSubHeaderText = currentCrag.crag_name;
				CurrentSector.SectorSubInfo = currentCrag.crag_name;
                SectorImage = CurrentSector.SectorImage;

                if (currentCrag.Unlocked)
                {
                    IsCragUnlocked = true;
                }

                if (SectorImage == null)
                {
                    SectorImage = await MapModelHelper.GetCragImageAsync(cragImageRepository);
                }
                IsCragOrDefaultImageCount = CurrentSector.IsCragOrDefaultImageCount;

                routeTables = (await routeRepository.GetAsync(route => route.sector_id == CurrentSector.SectorId &&
				                                                       route.is_enabled))
					.OrderBy(x => x.sort_order);

                var tempBuckets = await DataTemplateHelper.GetBucketsSourceAsync(Settings.ActiveCrag, routeTables.ToList(), exceptionManager);
                if (parameters.TryGetValue<List<int>>(NavigationParametersConstants.BucketsFilterIndexes, out var selectedIdexes))
                {
                    foreach (var index in selectedIdexes)
                        tempBuckets[index].Selected = true;
                }
                LegendsDataSource = tempBuckets;
                tempBuckets = null;


                int i = 1;
                int j = 0;
                var routeObjects = new List<RouteDataModel>();
				foreach (RouteTable route in routeTables)
				{
					var routeobj = new RouteDataModel();
                    if(AppSetting.APP_TYPE.Equals("outdoor") && route.first_ascent_date.HasValue)
                    {
                        var firstAscentDate = route.first_ascent_date.Value;
                        var totalDays = DateTime.Today.Subtract(firstAscentDate);
                        if (totalDays.TotalDays < 365)
                            routeobj.RouteIndex = "True";
                        else
                            routeobj.RouteIndex = "False";
                    }
                    else if(AppSetting.APP_TYPE.Equals("indoor"))
                    {
                        var total = DateTime.Today.Subtract(route.route_set_date);
                        if (total.Days < 15)
                            routeobj.RouteIndex = "True";
                        else
                            routeobj.RouteIndex = "False";                        
                    }
					//if (i < 10)
					//{
					//	routeobj.RouteIndex = "0" + i.ToString();
					//}
					//else
					//{
					//	routeobj.RouteIndex = i.ToString();
					//}

					routeobj.TitleText = route.route_name;
					routeobj.SubText = route.route_info;
					routeobj.RouteId = route.route_id;
                    routeobj.GrageBucketId = route.grade_bucket_id;
                    try
                    {
                        if (route.rating != null && route.rating >= 1)
                        {
                            routeobj.Rating = Math.Round(route.rating.Value).ToString();
                            routeobj.StarImage = ImageSource.FromFile(GetStarImage(route.rating));
							routeobj.HasStarImage = true;

						}
                        else
                        {
                            routeobj.Rating = "0";
                            routeobj.StarImage = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        await exceptionManager.LogException(new ExceptionTable
                        {
                            Method = nameof(this.OnNavigatingTo),
                            Page = this.GetType().Name,
                            StackTrace = ex.StackTrace,
                            Exception = ex.Message,
                            Data = $"Routes = {Newtonsoft.Json.JsonConvert.SerializeObject(route)}"
                        })
                            .ConfigureAwait(false);

                        routeobj.Rating = "0";
                        routeobj.StarImage = null;
                    }

					//if (!string.IsNullOrEmpty(route.angles_top_1) && Convert.ToInt32(route.angles_top_1) > 0)
					//	routeobj.Steepness1 = ImageSource.FromFile(GetTopAngleResourceName(route.angles_top_1) + "_20x20");
					//else
					//	routeobj.Steepness1 = ImageSource.FromFile(GetTopAngleResourceName("2") + "_20x20");

					//if (!string.IsNullOrEmpty(route.hold_type_top_1) && Convert.ToInt32(route.hold_type_top_1) > 0)
					//	routeobj.Steepness2 = ImageSource.FromFile(GetTopHoldResourceName(route.hold_type_top_1) + "_20x20");
					//else
					//	routeobj.Steepness2 = ImageSource.FromFile(GetTopHoldResourceName("4") + "_20x20");

					// Route Type icon
					if (!string.IsNullOrEmpty(route.route_type) && Convert.ToInt32(route.route_type_id) > 0)
					routeobj.Steepness2 = ImageSource.FromFile(GetRouteTypeResourceName(route.route_type_id));


					//if (!string.IsNullOrEmpty(route.route_style_top_1) && Convert.ToInt32(route.route_style_top_1) > 0)
					//	routeobj.Steepness3 = ImageSource.FromFile(GetTopRouteStyleResourceName(route.route_style_top_1) + "_20x20");
					//else
					//	routeobj.Steepness3 = ImageSource.FromFile(GetTopRouteStyleResourceName("2") + "_20x20");

					// Safety Rating icon
					if (!string.IsNullOrEmpty(route.safety_rating) && route.safety_rating != "0" 
					    && !string.IsNullOrEmpty(route.safety_rating_type) && route.safety_rating_type != "0")
					{
						routeobj.Steepness3 = ImageSource.FromFile(GetsafetyRating(route.safety_rating) + "_20x20");
						routeobj.HasSteepness3 = true;
					}

					routeobj.RouteTechGrade = route.tech_grade;
					if (j > 4)
					{ j = 0; }
					//routeobj.RouteGradeColor = GetGradeBucketHex(route.grade_bucket_id);
					var bucketColor = await bucketRepository.ExecuteScalarAsync<string>($"SELECT hex_code FROM T_BUCKET Where grade_bucket_id= {route.grade_bucket_id} Limit 1").ConfigureAwait(false);
					routeobj.RouteGradeColor = string.IsNullOrEmpty(bucketColor) ? "#cccccc" : bucketColor;
                    routeObjects.Add(routeobj);
					i++;
					j++;
				}

                routeData.AddRange(routeObjects);

                App.ChangeActivityIndicator = UpdateActivityIndicator;
				wasPageLoaded = true;
                ApplyFilter();
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
					{
						Method = nameof(this.OnNavigatingTo),
						Page = this.GetType().Name,
						StackTrace = ex.StackTrace,
						Exception = ex.Message,
						Data = $"Routes = {Newtonsoft.Json.JsonConvert.SerializeObject(routeTables)}"
					})
					.ConfigureAwait(false);
			}
			finally
			{
				routeTables = null;
			}
		}

        void ApplyFilter()
        {
            var filters = LegendsDataSource.Where(f => f.Selected).ToList();
            filteredBucketIds = filters.Select(x => x.Buckets.FirstOrDefault().grade_bucket_id).ToList();

            if (!filters?.Any() ?? true)
                Routes = new ObservableCollection<RouteDataModel>(routeData);
            else
                Routes = routeData.Where(r => filteredBucketIds.Any(f => r.GrageBucketId == f)).ToObservableCollection();

            RaisePropertyChanged(nameof(ShowEmptyOverlay));
        }
    }
}