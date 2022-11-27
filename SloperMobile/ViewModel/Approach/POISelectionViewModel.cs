using System;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using SloperMobile.ViewModel;
using System.Windows.Input;
using Xamarin.Forms;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using SloperMobile.Common.NotifyProperties;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Acr.UserDialogs;

namespace SloperMobile
{
    public class POISelectionViewModel : BaseViewModel
    {
        readonly IRepository<SectorTable> sectorRepository;
        readonly IRepository<CragExtended> cragRepository;

        bool isRecordInProgress => TrailCollector.Instance.ActiveRecord != null;

        public ICommand CloseCommand => new Command(() => navigationService.GoBackAsync());

        ObservableCollectionFast<string> listItems = new ObservableCollectionFast<string>();
        List<SectorTable> sectors;
        Action<int, bool> poiSelected;

        public ObservableCollectionFast<string> ListItems {
            get => listItems;
            set => SetProperty(ref listItems, value);
        }

        public string SelectedItem {
            set => OnItemSelected(value);
        }


        public POISelectionViewModel(
            IRepository<SectorTable> sectorRepository,
            IRepository<CragExtended> cragRepository,
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager) : base(navigationService, exceptionManager)
        {
            this.sectorRepository = sectorRepository;
            this.cragRepository = cragRepository;
        }

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            parameters.TryGetValue("poiSelected", out poiSelected);

            var activeCrag = await cragRepository.GetAsync(Settings.ActiveCrag);
            PageHeaderText = activeCrag.crag_name.ToUpper();
            PageSubHeaderText = isRecordInProgress ? "ARRIVE POINT" : "STARTING POINT";

            sectors = (await sectorRepository.GetAsync(s => s.crag_id == activeCrag.crag_id)).OrderBy(s => s.sort_order).ToList();

            if (TrailCollector.Instance.ActiveRecord?.sector_start_id != null)
            {
                sectors.RemoveAll(s => s.sector_id == TrailCollector.Instance.ActiveRecord?.sector_start_id);
            }

            if (TrailCollector.Instance.ActiveRecord?.parking_start_id == null)
            {
                ListItems.Add("PARKING");
            }
            ListItems.AddRange(sectors.Select(s => s.sector_name.ToUpper()));
            RaisePropertyChanged(nameof(ListItems));
        }

        async void OnItemSelected(string item) 
        {
            if (item == null)
                return;
                
            if (item == "PARKING") 
            {
                int id;
                using (UserDialogs.Instance.Loading())
                {
                    id = await GetCurrentParkingIdAsync();
                }
                poiSelected?.Invoke(id, true);
                navigationService.GoBackAsync();
                return;
            }

            var sector = sectors.First(s => s.sector_name.ToUpper() == item);
            poiSelected?.Invoke(sector.sector_id, false);
            navigationService.GoBackAsync();
        }

        async Task<int> GetCurrentParkingIdAsync()
        {
            try
            {
                var activeCrag = Settings.ActiveCrag;
                var myLocation = await CrossGeolocator.Current.GetPositionAsync(TimeSpan.FromSeconds(1));
                var parkings = await new Repository<ParkingTable>().GetAsync(p => p.crag_id == activeCrag);
                var ordered = parkings.OrderBy(p => GeolocatorUtils.CalculateDistance(myLocation, new Position(p.latitude, p.longitude)));
                var closestParking = ordered.First();
                var minDistance = GeolocatorUtils.CalculateDistance(myLocation, new Position(closestParking.latitude, closestParking.longitude));
                if (minDistance * 1600 < 100) //parking is found closer than 100 meters
                    return closestParking.id;
                return -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
