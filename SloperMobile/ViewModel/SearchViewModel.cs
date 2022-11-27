using System;
using System.Diagnostics;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using System.Collections.Generic;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using System.Threading.Tasks;
using System.Linq;
using Acr.UserDialogs;
using System.Collections.ObjectModel;
using SloperMobile.Common.Helpers;
using SloperMobile.ViewModel.SectorViewModels;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Constants;
using SloperMobile.Model;
using SloperMobile.Model.GuideBookModels;
using SloperMobile.ViewModel.GuideBookViewModels;

namespace SloperMobile.ViewModel
{
    public class SearchViewModel : BaseViewModel
    {
        int cragId;
        Task initTask;
        bool isLoaded;

        readonly IRepository<CragExtended> cragRepository;
        readonly IRepository<SectorTable> sectorRepository;
        readonly IRepository<RouteTable> routeRepository;
        readonly IRepository<GuideBookTable> guidebookRepository;

        private static List<IGrouping<string, SearchResult>> staticAllItems;

        static SearchViewModel() {
            staticAllItems = new List<IGrouping<string, SearchResult>>();
        }

        public SearchViewModel(
            IRepository<CragExtended> cragRepository,
            IRepository<SectorTable> sectorRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<GuideBookTable> guidebookRepository,
            INavigationService navigationService, 
            IExceptionSynchronizationManager exceptionManager, 
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {
            this.cragRepository = cragRepository;
            this.sectorRepository = sectorRepository;
            this.routeRepository = routeRepository;
            this.guidebookRepository = guidebookRepository;
            PageHeaderText = "SEARCH";
            Offset = Common.Enumerators.Offsets.Header;
            IsBackButtonVisible = true;
        }

        List<IGrouping<string, SearchResult>> filteredItems;
        public List<IGrouping<string, SearchResult>> FilteredItems
        {
            get => filteredItems;
            set => SetProperty(ref filteredItems, value);
        }

        List<IGrouping<string, SearchResult>> allItemsByCrag = new List<IGrouping<string, SearchResult>>();
        public List<IGrouping<string, SearchResult>> AllItemsByCrag
        {
            get => allItemsByCrag;
            set => SetProperty(ref allItemsByCrag, value);
        }

        string searchText;
        public string SearchText
        {
            get => searchText;
            set { 
                if (SetProperty(ref searchText, value))
                    Search();
            }
        }

        SearchResult selectedItem;
        public SearchResult SelectedItem
        {
            get => selectedItem;
            set {
                if (value == null)
                    return;

                SetProperty(ref selectedItem, value);
                ItemSelected();
            }
        }

        bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        public Command CloseCommand => new Command(Close);

        public string SearchPlaceholder => cragId == 0 ? "Guidebook, Area, Crag, Sector, Route" : "Sector, Route";


        public override bool IsShowHeader => true;

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);

            if (isLoaded)
                return;

            isLoaded = true;

            parameters.TryGetValue("CragId", out cragId);
            RaisePropertyChanged(nameof(SearchPlaceholder));

            initTask = LoadAllItems();
        }

        void Close(object obj)
        {
            navigationService.GoBackAsync();
        }

        async Task LoadAllItems()
        {
            if (cragId != 0)
            {
                var sectors = await sectorRepository.GetAsync(s => s.is_enabled && s.crag_id == cragId);
                var sectorItems = sectors.OrderBy(s => s.sector_name).Select(s => new SearchResult
                {
                    Id = s.sector_id,
                    Title = s.sector_name,
                    EntityType = typeof(SectorTable)
                }).ToLookup(s => "Sectors", s => s)
                .FirstOrDefault();
                if (sectorItems != null)
                {
                    AllItemsByCrag.Add(sectorItems);
                }

                var routes = await routeRepository.GetAsync(r => r.is_enabled && r.crag_id == cragId);
                var routeItems = routes.OrderBy(r => r.route_name).Select(r => new SearchResult
                {
                    Id = r.route_id,
                    Title = r.route_name,
                    EntityType = typeof(RouteTable)
                }).ToLookup(s => "Routes", r => r)
                .FirstOrDefault();
                if (routeItems != null)
                {
                    AllItemsByCrag.Add(routeItems);
                }

            }
            else {
                if (!staticAllItems.Any(s => s.Key == "Guidebooks"))
                {
                    var guidebooks = await guidebookRepository.GetAsync(r => r.is_app_store_ready);
                    var dict = new Dictionary<long, string>();
                    foreach (var guidebook in guidebooks)
                    {
                        var gbCrags = await cragRepository.GetAsync(c => c.crag_guide_book == guidebook.GuideBookId);
                        var areas = gbCrags.OrderBy(c => c.area_name).Select(c => c.area_name);
                        areas = areas.Distinct();
                        var areasString = string.Join("|", areas);
                        dict.Add(guidebook.GuideBookId, areasString);
                    }
                    var guidebookItems = guidebooks.OrderBy(g => g.GuideBookName).Select(g => new SearchResult
                    {
                        Id = (int)g.GuideBookId,
                        Title = g.GuideBookName,
                        EntityType = typeof(GuideBookTable),
                        Additional = dict[g.GuideBookId]
                    }).ToLookup(s => "Guidebooks", r => r)
                    .FirstOrDefault();
                    if (guidebookItems != null)
                    {
                        staticAllItems.Add(guidebookItems);
                    }

                    var crags = await cragRepository.GetAsync(c => c.is_app_store_ready && c.is_enabled);
                    var cragItems = crags.OrderBy(c => c.crag_name).Select(c => new SearchResult
                    {
                        Id = c.crag_id,
                        Title = c.crag_name,
                        EntityType = typeof(CragExtended)
                    }).ToLookup(s => "Crags", r => r)
                    .FirstOrDefault();
                    if (cragItems != null)
                    {
                        staticAllItems.Add(cragItems);
                    }

                    var sectors = await sectorRepository.GetAsync(s => s.is_enabled);
                    var sectorItems = sectors.OrderBy(s => s.sector_name).Select(s => new SearchResult
                    {
                        Id = s.sector_id,
                        Title = s.sector_name,
                        EntityType = typeof(SectorTable)
                    }).ToLookup(s => "Sectors", s => s)
                    .FirstOrDefault();
                    if (sectorItems != null)
                    {
                        staticAllItems.Add(sectorItems);
                    }

                    var routes = await routeRepository.GetAsync(r => r.is_enabled);
                    var routeItems = routes.OrderBy(r => r.route_name).Select(r => new SearchResult
                    {
                        Id = r.route_id,
                        Title = r.route_name,
                        EntityType = typeof(RouteTable)
                    }).ToLookup(s => "Routes", r => r)
                    .FirstOrDefault();
                    if (routeItems != null)
                    {
                        staticAllItems.Add(routeItems);
                    }
                }
            }
        }

        async Task Search()
        {
            if (string.IsNullOrEmpty(searchText) || searchText.Length < 3)
            {
                FilteredItems = new List<IGrouping<string, SearchResult>>();
                return;
            }

            if (IsLoading)
                return;

            if (initTask != null && !initTask.IsCompleted)
            {
                IsLoading = true;
                await initTask;
                initTask = null;
                IsLoading = false;
            }

            var tempList = new List<IGrouping<string, SearchResult>>();
            if (cragId != 0)
            {
                foreach (var group in AllItemsByCrag)
                {
                    var filteredGroup = group.Where(i => i.Title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                            || i.Additional.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                        .ToLookup(i => group.Key, i => i)
                        .FirstOrDefault();

                    if (filteredGroup != null)
                        tempList.Add(filteredGroup);
                }
                FilteredItems = tempList;
            }
            else {
                foreach (var group in staticAllItems)
                {
                    var filteredGroup = group.Where(i => i.Title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                            || i.Additional.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
                        .ToLookup(i => group.Key, i => i)
                        .FirstOrDefault();

                    if (filteredGroup != null)
                        tempList.Add(filteredGroup);
                }
                FilteredItems = tempList;
            }
        }

        async Task ItemSelected()
        {
            if (selectedItem.EntityType == typeof(SectorTable))
            {
                var sector = await sectorRepository.GetAsync(selectedItem.Id);
                if (!await GeneralHelper.HandleDownloadedCrag(sector.crag_id, sector.sector_name, cragRepository, navigationService))
                    return;


                var mapModel = await MapModelHelper.GetFromSectorIdAsync(selectedItem.Id);
                var navParams = new NavigationParameters();
                navParams.Add(NavigationParametersConstants.SelectedSectorObjectParameter, mapModel);
                await navigationService.NavigateAsync<SectorRoutesViewModel>(navParams);
                return;
            }

            if (selectedItem.EntityType == typeof(GuideBookTable))
            {
                var guideBook = await guidebookRepository.GetAsync(selectedItem.Id);
                var navParams = new NavigationParameters();
                var guideBookModel = new GuideBook(guideBook);
                navParams.Add(NavigationParametersConstants.CurrentGuideBook, guideBookModel);
                await navigationService.NavigateAsync<GuideBookDetailViewModel>(navParams);
                return;
            }

            if (selectedItem.EntityType == typeof(CragExtended))
            {
                var crag = await cragRepository.GetAsync(selectedItem.Id);
                var navParams = new NavigationParameters();
                Settings.MapSelectedCrag = crag.crag_id;
                navParams.Add(NavigationParametersConstants.SelectedCragIdParameter, crag.crag_id);
                navParams.Add(NavigationParametersConstants.NavigatonServiceParameter, navigationService);
                await navigationService.NavigateAsync<CragDetailsViewModel>(navParams);
                return;
            }

            if (selectedItem.EntityType == typeof(RouteTable))
            {
                var route = await routeRepository.GetAsync(selectedItem.Id);
                if (!await GeneralHelper.HandleDownloadedCrag(route.crag_id, route.route_name, cragRepository, navigationService))
                    return;

                var sector = await sectorRepository.GetAsync(route.sector_id);
                var crag = await cragRepository.GetAsync(sector.crag_id);

                var mappedSector = new MapListModel
                {
                    SectorId = sector.sector_id,
                    SectorName = sector.sector_name
                };

                Cache.SelctedCurrentSector = mappedSector;
                Settings.ActiveCrag = sector.crag_id;

                var guideBook = await guidebookRepository.FindAsync(guide => guide.GuideBookId == crag.crag_guide_book);
                var navParams = new NavigationParameters();
                navParams.Add(NavigationParametersConstants.SelectedSectorObjectParameter, mappedSector);
                navParams.Add(NavigationParametersConstants.RouteIdParameter, selectedItem.Id);
                navParams.Add(NavigationParametersConstants.SingleRouteParameter, true);
                navParams.Add(NavigationParametersConstants.IsNavigatedFromSectorImageParameter, false);
                navParams.Add(NavigationParametersConstants.IsUnlockedParameter, Settings.AppPurchased || guideBook.Unlocked || crag.Unlocked);
                navParams.Add(NavigationParametersConstants.GuideBookIdParameter, guideBook.GuideBookId);
                await navigationService.NavigateAsync<SectorToposViewModel>(navParams);
                return;
            }
        }
    }
}
