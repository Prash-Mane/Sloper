using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.CustomControls;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.BucketsModel;
using SloperMobile.Model.CragModels;
using SloperMobile.Model.ResponseModels;
using SloperMobile.ViewModel.SectorViewModels;
using Xamarin.Forms;
using XLabs.Platform.Device;
using System.Diagnostics;
using System.Windows.Input;
using Syncfusion.DataSource.Extensions;

namespace SloperMobile.ViewModel
{
    public class CragSectorsViewModel : BaseViewModel
    {
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<CragImageTable> cragImageRepository;
        private readonly IRepository<CragSectorMapTable> cragSectorMapRepository;
        private readonly IRepository<BucketTable> bucketRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<GradeTable> gradeRepository;
        private readonly IRepository<AreaTable> areaRepository;

        private MapListModel selectedSector;
        private CragTable currentCrag;
        private bool loaded = false;
        private bool isCragsListEnabled;
        IEnumerable<SectorTable> sectorEntries;
        ObservableCollection<MapListModel> sectorImageList;
        ObservableCollection<MapListModel> sectorImages;
        bool isLoadingMore;
        double sectorimageheight;

        public CragSectorsViewModel(
            INavigationService navigationService,
            IRepository<SectorTable> sectorRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<CragImageTable> cragImageRepository,
            IRepository<CragSectorMapTable> cragSectorMapRepository,
            IRepository<BucketTable> bucketRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<GradeTable> gradeRepository,
            IRepository<AreaTable> areaRepository,
            IExceptionSynchronizationManager exceptionManager) : base(navigationService, exceptionManager)
        {
            this.sectorRepository = sectorRepository;
            this.cragRepository = cragRepository;
            this.cragImageRepository = cragImageRepository;
            this.cragSectorMapRepository = cragSectorMapRepository;
            this.bucketRepository = bucketRepository;
            this.topoRepository = topoRepository;
            this.gradeRepository = gradeRepository;
            this.areaRepository = areaRepository;

            SectorImageHeight = (Application.Current.MainPage.Height) / 3;
            sectorImageList = new ObservableCollection<MapListModel>();

            IsShowFooter = true;
            sectorImages = new ObservableCollection<MapListModel>();
            Offset = Offsets.Header;
        }

        public Command<BucketsSegmentModel> FilterCommand => new Command<BucketsSegmentModel>((b) => ApplyFilter());

        public double SectorImageHeight
        {
            get { return sectorimageheight; }
            set { sectorimageheight = value; RaisePropertyChanged(); }
        }
        public ObservableCollection<MapListModel> SectorImageList
        {
            get
            {
                return sectorImageList;
            }
            set 
            { 
               if( SetProperty(ref sectorImageList, value))
               {
                   RaisePropertyChanged(nameof(ShowEmptyOverlay));
               }
            }
        }

        public MapListModel SelectedSector
        {
            get { return selectedSector; }
            set
            {
                selectedSector = value;
                GoToSeletedSector();
            }
        }
        private List<BucketsSegmentModel> legendsDataSource;
        public List<BucketsSegmentModel> LegendsDataSource
        {
            get { return legendsDataSource; }
            set { SetProperty(ref legendsDataSource, value); }
        }

        public bool ShowEmptyOverlay => loaded && (!SectorImageList?.Any() ?? false);

        public bool IsLoadingMore
        {
            get { return isLoadingMore; }
            set
            {
                isLoadingMore = value;
                RaisePropertyChanged();
            }
        }
 
        public bool IsCragsListEnabled
        {
            get
            {
                return isCragsListEnabled;
            }
            set
            {
                SetProperty(ref isCragsListEnabled, value);
            }
        }

        public async Task InitialLoad()
        {
            UserDialogs.Instance.ShowLoading("Loading...");
            try
            {
                LegendsDataSource = await DataTemplateHelper.GetBucketsSourceAsync(Settings.ActiveCrag, exceptionManager: exceptionManager);

                sectorEntries = (await sectorRepository.GetAsync(sector => sector.crag_id == currentCrag.crag_id && sector.is_enabled))
                                                    .OrderBy(x => x.sort_order);
                await LoadSectorImages();
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.InitialLoad),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        private async Task LoadSectorImages()
        {
            try
            {
                if (sectorEntries.Count() == 0)
                {
                    return;
                }

                var bucketsColors = new List<string>();
                var totalbuckets = await bucketRepository.ExecuteScalarAsync<int>("SELECT Count(*) FROM T_BUCKET GROUP BY grade_type_id");
                if (totalbuckets != 0)
                {
                    for (int i = 1; i <= totalbuckets; i++)
                    {
                        var bucketColor = await bucketRepository.ExecuteScalarAsync<string>($"SELECT hex_code FROM T_BUCKET Where grade_bucket_id={i}");
                        bucketsColors.Add(bucketColor);
                    }
                }

                foreach (var sector in sectorEntries)
                {
                    var mapListModel = await MapModelHelper.GetFromSectorIdAsync(sector.sector_id, 
                                                                                 sectorRepository, topoRepository, cragImageRepository);
                    if (totalbuckets != 0)
                    {
                        var bucketsCount = new List<int>();
                        for (int i = 1; i <= totalbuckets; i++)
                        {
                            var bucketCount = await gradeRepository.FindAsync(grade => grade.sector_id == sector.sector_id && grade.grade_bucket_id == i);
                            bucketsCount.Add(bucketCount?.grade_bucket_id_count ?? 0);
                        }

                        mapListModel.BucketsCount = bucketsCount;
                        mapListModel.BucketCountTemplate = new DataTemplate(() =>
                            {
                                var slBucketFrame = new StackLayout();
                                slBucketFrame.Orientation = StackOrientation.Horizontal;
                                slBucketFrame.HorizontalOptions = LayoutOptions.EndAndExpand;
                                slBucketFrame.VerticalOptions = LayoutOptions.Center;
                                slBucketFrame.Spacing = 4;

                                //foreach (T_GRADE tgrd in tgrades)
                                for (int i = 0; i < totalbuckets; i++)
                                {
                                    var grid = new Grid { HeightRequest = 25, WidthRequest = 25 };

                                    //var contentFrameBorder = new Frame();
                                    //contentFrameBorder.CornerRadius = 0;
                                    //contentFrameBorder.HasShadow = false;
                                    //contentFrameBorder.Padding = 0;
                                    //contentFrameBorder.WidthRequest = 25;
                                    //contentFrameBorder.HeightRequest = 25;
                                    //contentFrameBorder.OutlineColor = Color.FromHex(string.IsNullOrEmpty(bucketsColors[i])
                                    //    ? "cccccc"
                                    //    : bucketsColors[i]);
                                    ////set the background color to fully transparent
                                    //contentFrameBorder.BackgroundColor = Color.FromHex("00FFFF00");

                                    var contentFrame = new Frame();
                                    contentFrame.CornerRadius = 0;
                                    contentFrame.HasShadow = false;
                                    contentFrame.Padding = 0;
                                    var hexCol = string.IsNullOrEmpty(bucketsColors[i]) ? "#cccccc" : bucketsColors[i];
                                    contentFrame.BorderColor = Color.FromHex(hexCol);
                                    contentFrame.BackgroundColor = Color.FromHex($"#8c{hexCol.Substring(1)}");
                                    var lblcount = new Label();
                                    lblcount.Text = bucketsCount[i].ToString();
                                    lblcount.HorizontalOptions = LayoutOptions.Center;
                                    lblcount.VerticalOptions = LayoutOptions.Center;
                                    lblcount.TextColor = Color.White;
                                    lblcount.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));

                                    //grid.Children.Add(contentFrameBorder);
                                    grid.Children.Add(contentFrame);
                                    grid.Children.Add(lblcount);

                                    slBucketFrame.Children.Add(grid);
                                }

                                return slBucketFrame;
                        });
                    }

                    //TODO: Check if this could cause the issue
                    sectorImages.Add(mapListModel);
                }
                //Sectors = new ReadOnlyObservableCollection<MapListModel>(SectorImageList);

                SectorImageList = sectorImages;
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.LoadSectorImages),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
            finally
            {
                IsLoadingMore = false;
                UserDialogs.Instance.HideLoading();
            }
        }

		private async void GoToSeletedSector()
		{
			try
			{
				if (SelectedSector == null)
				{
					return;
				}

		        UserDialogs.Instance.ShowLoading("Loading...");
                var navigationParameters = new NavigationParameters();
                navigationParameters.Add("selectedSectorObject", SelectedSector);

                var selectedIndexes = Enumerable.Range(0, LegendsDataSource.Count)
                        .Where(i => LegendsDataSource[i].Selected)
                        .ToList();
                navigationParameters.Add(NavigationParametersConstants.BucketsFilterIndexes, selectedIndexes);

                await navigationService.NavigateAsync<SectorRoutesViewModel>(navigationParameters);
	        }
	        catch (Exception ex)
	        {
		        await exceptionManager.LogException(new ExceptionTable
		        {
			        Method = nameof(this.GoToSeletedSector),
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

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
			try
			{
				if (!loaded)
				{
					var cragid = Settings.ActiveCrag;
					currentCrag = await cragRepository.FindAsync(c => c.crag_id == cragid);
					PageHeaderText = currentCrag?.crag_name;
					PageSubHeaderText = (await areaRepository.FindAsync(a => a.area_id == currentCrag.area_id)).area_name;
					await InitialLoad();
					loaded = true;
                    RaisePropertyChanged(nameof(ShowEmptyOverlay));
                }
				Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
				IsCragsListEnabled = true;
				App.IsNavigating = false;

			}
			catch (Exception exception)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatedTo),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data = JsonConvert.SerializeObject(exception.Data)
				});
			}
        }

        void ApplyFilter()
        {
            var filters = LegendsDataSource.Where(f => f.Selected).ToList();
            var filteredBucketIds = filters.Select(x => x.Buckets.FirstOrDefault().grade_bucket_id).ToList();
            if (!filters?.Any() ?? true)
                SectorImageList = sectorImages;
            else
                SectorImageList = sectorImages.Where(s => filteredBucketIds.Any(f => s.BucketsCount[f - 1] > 0)).ToObservableCollection();
            RaisePropertyChanged(nameof(ShowEmptyOverlay));
        }
    }
}
