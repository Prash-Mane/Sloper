using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.SectorViewModels;
using Xamarin.Forms;

namespace SloperMobile.ViewModel
{
	public class NewsViewModel : BaseViewModel
    {
		private readonly INavigationService navigationService;
		private readonly IRepository<SectorTable> sectorRepository;
		private readonly IRepository<CragImageTable> cragImageRepository;
		private readonly IRepository<BucketTable> bucketRepository;
		private readonly IRepository<TopoTable> topoRepository;
	    private readonly IUserDialogs userDialogs;
		private readonly IExceptionSynchronizationManager exceptionManager;

		private ObservableCollection<NewsModel> _newsList;
        public ObservableCollection<NewsModel> NewsList
        {
            get { return _newsList ?? (_newsList = new ObservableCollection<NewsModel>()); }
            set { _newsList = value; RaisePropertyChanged(); }
        }
        public Command LoadMoreNews { get; set; }
		public NewsViewModel(
			INavigationService navigationService,
			IRepository<SectorTable> sectorRepository,
			IRepository<CragImageTable> cragImageRepository,
			IRepository<BucketTable> bucketRepository,
			IRepository<TopoTable> topoRepository,
			IExceptionSynchronizationManager exceptionManager,
			IUserDialogs userDialogs) : base(navigationService, exceptionManager)
		{
			this.navigationService = navigationService;
			this.sectorRepository = sectorRepository;
			this.cragImageRepository = cragImageRepository;
			this.bucketRepository = bucketRepository;
			this.topoRepository = topoRepository;
			this.userDialogs = userDialogs;
			this.exceptionManager = exceptionManager;
			userDialogs.ShowLoading("Loading...");
			PageHeaderText = "NEWS";
			PageSubHeaderText = "What's New?";
            SectorImageHeight = (Application.Current.MainPage.Height) / 3;			
            LoadMoreNews = new Command(LoadNews);
			selectedSector = new MapListModel();

            IsMenuVisible = true;
		}

        private double sectorimageheight;
        public double SectorImageHeight
        {
            get { return sectorimageheight; }
            set { sectorimageheight = value; RaisePropertyChanged(); }
        }

        private async void LoadNews()
        {
            try
            {											 
                //TODO: ????
                //var query = await sectorRepository.QueryAsync<NewsModel>("SELECT RouteTable.date_created AS date, SectorTable.sector_id AS id, UPPER(T_CRAG.crag_name) as title, UPPER(SectorTable.sector_name) as sub_title, COUNT(SectorTable.sector_name) AS count, (COUNT(SectorTable.sector_name) || ' NEW ' || (CASE WHEN COUNT(SectorTable.sector_name) = 1 THEN 'ROUTE' ELSE 'ROUTES' END)) as message, 'newRoutes' AS news_type, T_CRAG.crag_id AS cragid FROM SectorTable INNER JOIN RouteTable ON SectorTable.sector_id = RouteTable.sector_id INNER JOIN T_CRAG ON SectorTable.crag_id = T_CRAG.crag_id WHERE SectorTable.is_enabled=1 GROUP BY RouteTable.date_created, SectorTable.sector_id, T_CRAG.crag_name, SectorTable.sector_name, SectorTable.sort_order ORDER BY date DESC, SectorTable.sort_order LIMIT 10");
                var query = await sectorRepository.QueryAsync<NewsModel>("SELECT T_ROUTE.date_created AS date, T_SECTOR.sector_id AS id, UPPER(T_CRAG.crag_name) as title, UPPER(T_SECTOR.sector_name) as sub_title, COUNT(T_SECTOR.sector_name) AS count, (COUNT(T_SECTOR.sector_name) || ' NEW ' || (CASE WHEN COUNT(T_SECTOR.sector_name) = 1 THEN 'ROUTE' ELSE 'ROUTES' END)) as message, 'newRoutes' AS news_type, T_CRAG.crag_id AS cragid FROM T_SECTOR INNER JOIN T_ROUTE ON T_SECTOR.sector_id = T_ROUTE.sector_id INNER JOIN T_CRAG ON T_SECTOR.crag_id = T_CRAG.crag_id WHERE T_SECTOR.is_enabled=1 GROUP BY T_ROUTE.date_created, T_SECTOR.sector_id, T_CRAG.crag_name, T_SECTOR.sector_name, T_SECTOR.sort_order ORDER BY date DESC, T_SECTOR.sort_order LIMIT 10");
                var app_news = query;
                foreach (NewsModel nm in app_news)
                {
                    string strimg64 = string.Empty;
                    var sec_img = await topoRepository.FindAsync(sec => sec.sector_id == nm.id);
                    if (sec_img != null)
                    {
                        if (!(sec_img.topo_json == "[]"))
                        {
                            var topoimg = JsonConvert.DeserializeObject<List<TopoImageResponseModel>>(sec_img.topo_json);
                            if (topoimg != null && topoimg.Count > 0)
                            {
                                for (int i = 0; i < topoimg.Count; i++)
                                {
                                    if (!string.IsNullOrEmpty(topoimg[0].image.data))
                                    {
                                        if (topoimg[0].image.name == "No_Image.jpg")
                                        {
                                            nm.news_image = await LoadCragAndDefaultImage(nm.cragid);
                                        }
                                        else
                                        {
                                            strimg64 = topoimg[0].image.data.Split(',')[1];
                                            if (!string.IsNullOrEmpty(strimg64))
                                            {
                                                byte[] imageBytes = Convert.FromBase64String(strimg64);
                                                nm.news_image = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (topoimg.Count == 2)
                                        {
                                            if (!string.IsNullOrEmpty(topoimg[1].image.data))
                                            {
                                                if (topoimg[1].image.name == "No_Image.jpg")
                                                {
                                                    nm.news_image = await LoadCragAndDefaultImage(nm.cragid);
                                                }
                                                else
                                                {
                                                    strimg64 = topoimg[1].image.data.Split(',')[1];
                                                    if (!string.IsNullOrEmpty(strimg64))
                                                    {
                                                        byte[] imageBytes = Convert.FromBase64String(strimg64);
                                                        nm.news_image = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            nm.news_image = await LoadCragAndDefaultImage(nm.cragid);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            nm.news_image = await LoadCragAndDefaultImage(nm.cragid);
                        }
                    }
                    else
                    {
                        nm.news_image = await LoadCragAndDefaultImage(nm.cragid);
                    }
                    NewsList.Add(nm);
                }
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.LoadNews),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				}); 
			}
            finally
            {
	            userDialogs.HideLoading();
            }
        }

        private async Task<ImageSource> LoadCragAndDefaultImage(int cragid)
        {
            CragImageTable item = null;				 
            string strimg64 = string.Empty;
            ImageSource news_image = null;
            //load Crag Scenic Action Portrait Shot (specific to Gym)
            if (Settings.ActiveCrag == cragid)
            {
                //get selected crag image info
                item = await cragImageRepository.FindAsync(tcragimg => tcragimg.crag_id == Settings.ActiveCrag);
            }
            else
            {   //get Non-selected crag image info
                item = await cragImageRepository.FindAsync(tcragimg => tcragimg.crag_id == cragid);
            }
            if (item != null)
            {
                strimg64 = item?.crag_image?.Split(',')[1];
                if (!string.IsNullOrEmpty(strimg64))
                {
                    byte[] imageBytes = Convert.FromBase64String(strimg64);
                    news_image = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            else
            {
                //other wise show default                                
                if (AppSetting.APP_TYPE == "indoor")
                {
                    news_image = ImageSource.FromFile("default_sloper_indoor_landscape.jpg");
                }
                else { news_image = ImageSource.FromFile("default_sloper_outdoor_landscape.jpg"); }
            }
            return news_image;
		}

		private MapListModel selectedSector;
		public MapListModel SelectedSector
		{
			get { return selectedSector; }
			set { SetProperty(ref selectedSector, value); }
		}

		public ICommand NewsTapCommand
		{
			get
			{
				return new Command<NewsModel>(async news =>
				{
					if (news == null)
					{
						return;
					}
                    SelectedSector = await MapModelHelper.GetFromSectorIdAsync(news.id, sectorRepository, topoRepository, cragImageRepository);

                    //not used
					//int totalbuckets = await bucketRepository.ExecuteScalarAsync<int>("SELECT Count(grade_type_id) As BucketCount FROM T_BUCKET GROUP BY grade_type_id LIMIT 1");
					//if (totalbuckets != 0)
					//{

					//	SelectedSector.BucketCountTemplate = new DataTemplate(() =>
					//	{
					//		var slBucketFrame = new StackLayout();
					//		slBucketFrame.Orientation = StackOrientation.Horizontal;
					//		slBucketFrame.HorizontalOptions = LayoutOptions.EndAndExpand;
					//		slBucketFrame.VerticalOptions = LayoutOptions.Start;
					//		for (int i = 1; i <= totalbuckets; i++)
					//		{
					//			Frame countframe = new Frame();
					//			countframe.HasShadow = false;
					//			countframe.Padding = 0; countframe.WidthRequest = 25; countframe.HeightRequest = 20;
					//			var bucketColor = bucketRepository.ExecuteScalarAsync<string>($"SELECT hex_code FROM T_BUCKET Where grade_bucket_id= {i} Limit 1").Result;
					//			countframe.BackgroundColor = Color.FromHex(string.IsNullOrEmpty(bucketColor) ? "#cccccc" : bucketColor);
					//			Label lblcount = new Label();
					//			var bucketCount = bucketRepository.ExecuteScalarAsync<string>($"SELECT grade_bucket_id_count FROM T_GRADE WHERE sector_id = {SelectedSector.SectorId} and grade_bucket_id = {i}").Result;
					//			if (bucketCount != null)
					//			{
					//				lblcount.Text = bucketCount;
					//			}
					//			else
					//			{
					//				lblcount.Text = "0";
					//			}
					//			lblcount.HorizontalOptions = LayoutOptions.CenterAndExpand;
					//			lblcount.VerticalOptions = LayoutOptions.CenterAndExpand;
					//			lblcount.TextColor = Color.White; lblcount.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
					//			countframe.Content = lblcount;
					//			slBucketFrame.Children.Add(countframe);
					//		}
					//		return slBucketFrame;
					//	});
					//}

					var navigationParameters = new NavigationParameters();
					navigationParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, SelectedSector);
					await navigationService.NavigateAsync<SectorRoutesViewModel>(navigationParameters);
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
            LoadNews();
            Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
            App.IsNavigating = false;
		}

		public override void OnNavigatingTo(NavigationParameters parameters)
		{
            base.OnNavigatingTo(parameters);
        }
	}
}
