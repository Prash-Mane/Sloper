using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.NotifyProperties;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.GuideBookModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using SloperMobile.Common.Extentions;
using System.Diagnostics;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using System.Globalization;
using SloperMobile.Model.CragModels;
using SloperMobile.UserControls;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace SloperMobile.ViewModel.GuideBookViewModels
{
    public class GuideBookViewModel : BaseViewModel
    {
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<RouteTable> routeRepository;

        readonly IUserDialogs userDialogs;
        bool isLoaded;

        public GuideBookViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IRepository<GuideBookTable> guideBookRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<RouteTable> routeRepository,
            IHttpHelper httpHelper,
            IUserDialogs userDialogs) : base(navigationService, exceptionManager, httpHelper)
        {
            this.guideBookRepository = guideBookRepository;
            this.cragRepository = cragRepository;
            this.routeRepository = routeRepository;
            this.userDialogs = userDialogs;

            IsMenuVisible = true;
            PageHeaderText = "GUIDEBOOKS";
            PageSubHeaderText = "SLOPER COLLECTION";
            OpenGuideBookCommand = new Command<GuideBook>(GoToGuideBook);

            GeneralHelper.CragModified += CragModified;
            GeneralHelper.AppOrGuidebookPurchased += AppOrGuidebookPurchased;

            HasFade = true;
            GradientHeaderHeight = 100;
            HeaderColors = new List<GradientColor> {
                new GradientColor { Color = Color.Black, Position = 0.3f },
                new GradientColor { Color = Color.Transparent, Position = 0.9f }
            };
            IsShowFooter = true;
        }
        #region Properties


        public bool IsVisibleDownloaded => DownloadedGuideBook?.Any() ?? false;
        public bool IsVisibleFree => FreeGBs?.Any() ?? false;

        public Command OpenGuideBookCommand { get; set; }

        private List<GuideBook> _dwnloaded_GB;
        public List<GuideBook> DownloadedGuideBook
        {
            get { return _dwnloaded_GB; }
            set
            {
                SetProperty(ref _dwnloaded_GB, value);
            }
        }

        private List<GuideBook> freeGBs;
        public List<GuideBook> FreeGBs
        {
            get { return freeGBs; }
            set
            {
                SetProperty(ref freeGBs, value);
            }
        }

        private List<GuideBook> _available_GB;
        public List<GuideBook> NearestAvailableGuideBook
        {
            get { return _available_GB; }
            set
            {
                SetProperty(ref _available_GB, value);
            }
        }


        private List<GuideBook> _all_GB;
        public List<GuideBook> AllAvailableGuideBook
        {
            get { return _all_GB; }
            set
            {
                SetProperty(ref _all_GB, value);
            }
        }

        //private List<GuideBook> _popular_GB;
        //public List<GuideBook> PopularGuideBook
        //{
        //    get { return _popular_GB; }
        //    set
        //    {
        //        SetProperty(ref _popular_GB, value);
        //    }
        //}

        //private List<GuideBook> _new_GB;
        //public List<GuideBook> NewGuideBook
        //{
        //    get { return _new_GB; }
        //    set
        //    {
        //        SetProperty(ref _new_GB, value);
        //    }
        //}

        #endregion
        #region Private methods
        private async Task BindDownloadedGuideBooks()
        {
            var tempList = new List<GuideBook>();
            var guideBooks = await guideBookRepository.GetAsync(g => g.is_app_store_ready);
            var downloadedCrags = await cragRepository.GetAsync(c => c.is_downloaded);

            if (downloadedCrags.Any())
            {
                foreach (var gb in guideBooks)
                {
                    var downloadedGBCrags = downloadedCrags.Where(i => i.crag_guide_book == gb.GuideBookId);
                    if (!downloadedGBCrags.Any())
                        continue;

                    //var rating = await GetAverageRatingAsync(downloadedGBCrags);
                    var gbModel = new GuideBook(gb)
                    {
                        //GBType = GuideBook.GuideBookType.Downloaded
                    };
                    tempList.Add(gbModel);
                }
            }

            DownloadedGuideBook = tempList;
            RaisePropertyChanged(nameof(IsVisibleDownloaded));
        }

        private async Task BindGuideBooks()
        {
            var guideBooks = await guideBookRepository.GetAsync(g => g.is_app_store_ready);
            var myLocation = await GeneralHelper.GetMyPositionAsync();
            var tempnearestList = new List<GuideBook>();
            var crags = await cragRepository.GetAsync();

            foreach (var agb in guideBooks)
            {
                var gb_crags = crags.Where(cr => cr.crag_guide_book == agb.GuideBookId);
                if (!gb_crags.Any())
                    continue;

                var sortedCrag = gb_crags.OrderBy(fcr => fcr.crag_sort_order).FirstOrDefault();

                var gbModel = new GuideBook(agb)
                {
                    //GBType = GuideBook.GuideBookType.Nearest,
                    GBPosition = new Position(Convert.ToDouble(sortedCrag.crag_latitude, CultureInfo.InvariantCulture), Convert.ToDouble(sortedCrag.crag_longitude, CultureInfo.InvariantCulture))
                };
                tempnearestList.Add(gbModel);

            }

            AllAvailableGuideBook = new List<GuideBook>(tempnearestList.OrderBy(gb => GeolocatorUtils.CalculateDistance(myLocation.Latitude, myLocation.Longitude, gb.GBPosition.Latitude, gb.GBPosition.Longitude)));
            NearestAvailableGuideBook = AllAvailableGuideBook.Take(3).ToList();

            FreeGBs = guideBooks.Where(g => g.is_free).Select(g => new GuideBook(g)).ToList();
            RaisePropertyChanged(nameof(IsVisibleFree));

            /*
            var popularGBIdsResponse = await httpHelper.GetAsync<IList<long>>(ApiUrls.GetTopNPopularGuidebooks());
            if (!popularGBIdsResponse.ValidateResponse())
                return;
            var tempPopularGBList = new List<GuideBook>();
            var gbIds = popularGBIdsResponse.Result;

            var popularGbs = await guideBookRepository.GetAsync(x => gbIds.Contains(x.GuideBookId));

            foreach (var gb in popularGbs)
            {
                var popularGBCrags = await cragRepository.GetAsync(c => c.is_enabled && c.crag_guide_book == gb.GuideBookId);
                if (!popularGBCrags.Any())
                    continue;

                //var rating = await GetAverageRatingAsync(popularGBCrags);
                var gbModel = new GuideBook(gb)
                {
                    Rating = gb.AverageRating,
                    GBType = GuideBook.GuideBookType.Popular
                };

                tempPopularGBList.Add(gbModel);
            }
            PopularGuideBook = tempPopularGBList;

            var newGBIdResponse = await httpHelper.GetAsync<List<long>>(ApiUrls.Url_M_GetTopNNewGuideBooks());
            if (!newGBIdResponse.ValidateResponse())
                return;
            var tempNewGBList = new List<GuideBook>();
            var newGBs = await guideBookRepository.GetAsync(x => newGBIdResponse.Result.Contains(x.GuideBookId));
            foreach (var gb in newGBs)
            {
                var newGBCrags = await cragRepository.GetAsync(c => c.is_enabled && c.crag_guide_book == gb.GuideBookId);
                if (!newGBCrags.Any())
                    continue;

                //var rating = await GetAverageRatingAsync(newGBCrags);
                var gbModel = new GuideBook(gb)
                {
                    Rating = gb.AverageRating,
                    GBType = GuideBook.GuideBookType.New
                };
                tempNewGBList.Add(gbModel);
            }
            NewGuideBook = tempNewGBList;

            */
        }

        #endregion

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            RaisePropertyChanged(nameof(IsShowFooter));

            if (isLoaded)
                return;
            isLoaded = true;

            await BindDownloadedGuideBooks();

            try
            {
                await BindGuideBooks();
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = JsonConvert.SerializeObject(exception.Data)
                });
            }
            finally
            {
                App.IsNavigating = false;
            }
        }
        async void GoToGuideBook(GuideBook item)
        {
            // Code removed for XAM-1341
            /*  
            var navigationParameter = new NavigationParameters();
            if (item.GBType == GuideBook.GuideBookType.Downloaded)
            {
                DownloadedGuideBook.Remove(item);
                DownloadedGuideBook.Insert(0, item);
                navigationParameter.Add("SelectedGuideBook", DownloadedGuideBook);
            }
            if (item.GBType == GuideBook.GuideBookType.Popular)
            {
                PopularGuideBook.Remove(item);
                PopularGuideBook.Insert(0, item);
                navigationParameter.Add("SelectedGuideBook", PopularGuideBook);
            }
            if (item.GBType == GuideBook.GuideBookType.New)
            {
                NewGuideBook.Remove(item);
                NewGuideBook.Insert(0, item);
                navigationParameter.Add("SelectedGuideBook", NewGuideBook);
            }
            navigationParameter.Add("GBType", item.GBType);
            await this.navigationService.NavigateAsync<GuidebookDetailsListViewModel>(navigationParameter);
            */
            var navigationParameter = new NavigationParameters();
            navigationParameter.Add("CurrentGuideBook", item);
            await this.navigationService.NavigateAsync<GuideBookDetailViewModel>(navigationParameter);
        }

        #region Services 
        //private async Task SetupActionVisibility(GuideBook guideBook)
        //{
        //    var crags = await cragRepository.GetAsync(c => c.is_enabled && c.crag_guide_book == guideBook.GuideBookId);
        //    crags = crags.OrderBy(c => c.crag_name).ToList();
        //    var downloadedcrags = await cragRepository.GetAsync(c => c.is_enabled && c.is_downloaded && c.crag_guide_book == guideBook.GuideBookId);
        //    if (crags.Count == downloadedcrags.Count && crags.Count != 0 && downloadedcrags.Count != 0)
        //    {
        //        guideBook.IsVisibleDownloadAll = false;
        //        guideBook.IsVisibleRemoveAll = true;
        //    }
        //    else if (downloadedcrags.Count > 0)
        //    {
        //        guideBook.IsVisibleDownloadAll = true;
        //        guideBook.IsVisibleRemoveAll = true;
        //    }
        //    else if (downloadedcrags.Count == 0)
        //    {
        //        guideBook.IsVisibleDownloadAll = true;
        //        guideBook.IsVisibleRemoveAll = false;
        //    }
        //}

        private async void CragModified(object sender, CragExtended crag)
        {
            await BindDownloadedGuideBooks();
            //PopularGuideBook.Select(async p => { await SetupActionVisibility(p); return p; }).ToList();
            //NewGuideBook.Select(async n => { await SetupActionVisibility(n); return n; }).ToList();
        }

        void AppOrGuidebookPurchased(object sender, AppProductTable e)
        {
            var allGBs = AllAvailableGuideBook.Union(DownloadedGuideBook);
            if (e.AppProductTypeId == (int)ProductTypes.App)
            {
                foreach (var gb in allGBs)
                    gb.Unlocked = true;
            }

            if (e.AppProductTypeId == (int)ProductTypes.Guidebook)
            {
                var purchasedGBs = allGBs.Where(g => g.GuideBookId == e.SloperId);
                foreach (var gb in purchasedGBs)
                    gb.Unlocked = true;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            GeneralHelper.CragModified -= CragModified;
            GeneralHelper.AppOrGuidebookPurchased -= AppOrGuidebookPurchased;
        }

        #endregion
    }
}
