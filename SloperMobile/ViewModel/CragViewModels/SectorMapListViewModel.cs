using System;
using System.Collections.ObjectModel;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using SloperMobile.Model.CragModels;
using SloperMobile.ViewModel;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using SloperMobile.Common.Extentions;

namespace SloperMobile
{
    public class SectorMapListViewModel : BaseViewModel
    {
        readonly IRepository<CragExtended> cragRepository;
        readonly IRepository<MapTable> mapRepository;
        readonly IRepository<CragSectorMapTable> cragSectorMapRepository;
        readonly IRepository<SectorTable> sectorRepository;

        bool isLoaded;
        CragExtended currentCrag;

        ObservableCollection<CragMapModel> cragMaps = new ObservableCollection<CragMapModel>();
        public ObservableCollection<CragMapModel> CragMaps
        {
            get => cragMaps;
            set { SetProperty(ref cragMaps, value); }
        }


        public Command<CragMapModel> MapCommand => new Command<CragMapModel>(OnMapCommand);


        public SectorMapListViewModel(INavigationService navigationService,
                                      IExceptionSynchronizationManager exceptionManager,
                                     IRepository<CragExtended> cragRepository,
                                     IRepository<MapTable> mapRepository,
                                     IRepository<CragSectorMapTable> cragSectorMapRepository,
                                     IRepository<SectorTable> sectorRepository)
                                    :base(navigationService,exceptionManager)
        {
            this.cragRepository = cragRepository;
            this.mapRepository = mapRepository;
            this.cragSectorMapRepository = cragSectorMapRepository;
            this.sectorRepository = sectorRepository;

            PageHeaderText = "Crag Sector Maps";
            IsBackButtonVisible = true;

            Offset = Common.Enumerators.Offsets.Header;
        }

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;
            isLoaded = true;

            var cragId = Settings.ActiveCrag;
            currentCrag = await cragRepository.GetAsync(cragId);
            PageSubHeaderText = currentCrag.crag_name;

            await LoadCragSectorMapsAsync();
        }

        async Task LoadCragSectorMapsAsync()
        {
            try
            {
                var maps = (await mapRepository.GetAsync(m => m.crag_id == currentCrag.crag_id && m.is_enabled)).OrderBy(m => m.sort_order)?.ToList() ?? new List<MapTable>();
                foreach (var map in maps)
                {
                    var mapRegionEntries = await cragSectorMapRepository.GetAsync(csm =>
                                                                                  csm.crag_id == currentCrag.crag_id && csm.map_id == map.map_id);

                    var cragMapModel = new CragMapModel
                    {
                        Map = map,
                        MapRegions = mapRegionEntries?.ToList() ?? new List<CragSectorMapTable>()
                    };
                    CragMaps.Add(cragMapModel);
                }
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.LoadCragSectorMapsAsync),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
        }

        async void OnMapCommand(CragMapModel cragMapModel)
        {
            if (string.IsNullOrEmpty(cragMapModel.Map?.imagedata))
                return;

            var navParams = new NavigationParameters();

            var sectorEntries = (await sectorRepository.GetAsync(sector => sector.crag_id == currentCrag.crag_id && sector.is_enabled))
                                                    .OrderBy(x => x.sort_order);
            //assign sector names
            if (sectorEntries != null)
            {
                foreach (var region in cragMapModel.MapRegions)
                {
                    var sector = sectorEntries.FirstOrDefault(s => s.sector_id == region.sector_id);
                    region.SectorName = sector?.sector_name ?? "";
                }
            }

            cragMapModel.CragName = currentCrag.crag_name;

            navParams.Add("cragMapModel", cragMapModel);
            await navigationService.NavigateAsync<CragSectorMapDetailViewModel>(navParams);
        }
    }
}
