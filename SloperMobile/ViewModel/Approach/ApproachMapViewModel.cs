using System;
using SloperMobile.ViewModel;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using System.Collections.ObjectModel;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms;
using System.Threading.Tasks;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Helpers;
using System.Linq;
using System.Reflection;
using System.IO;
using SloperMobile.UserControls;
using SloperMobile.Common.Constants;
using SloperMobile.ViewModel.SectorViewModels;
using SloperMobile.Common.Extentions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Syncfusion.DataSource.Extensions;
using System.Diagnostics;
using Acr.UserDialogs;
using SloperMobile.UserControls.PopupControls;
using Rg.Plugins.Popup.Services;

namespace SloperMobile.ViewModel
{
    public class ApproachMapViewModel : BaseViewModel
    {
        readonly IRepository<SectorTable> sectorRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<TrailTable> trailRepository;
        readonly IRepository<CragExtended> cragRepository;

        bool isLoaded;
        MapSpan region;
        ObservableCollection<Polyline> polylines = new ObservableCollection<Polyline>();
        List<Polyline> allPolylines = new List<Polyline>();

        //temp, for testing. TODO: remove this logic before production
        List<Polyline> recordsPolylines = new List<Polyline>();
        bool isShowingRecords;

        public ApproachMapViewModel(
            INavigationService navigationService,
            IRepository<SectorTable> sectorRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IRepository<CragExtended> cragRepository,
            IExceptionSynchronizationManager exceptionManager) : base(navigationService, exceptionManager)
        { 
            IsBackButtonVisible = true;

            this.sectorRepository = sectorRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
            this.cragRepository = cragRepository;

            HasFade = true;
        }

        public ObservableCollection<Pin> Pins { get; set; } = new ObservableCollection<Pin>();

        public MapSpan Region
        {
            get => region;
            set => SetProperty(ref region, value);
        }

        public ObservableCollection<Polyline> Polylines
        {
            get => polylines;
            set => SetProperty(ref polylines, value);
        }

        public string RecordBtnImg
        {
            get => isRecordInProgress ? "stop" : "rec";
        }

        public bool IsCancelRecordVisible => isRecordInProgress;


        bool isRecordInProgress => TrailCollector.Instance.ActiveRecord != null;


        public Command<MapClickedEventArgs> MapClickedCommand => new Command<MapClickedEventArgs>(e => ShowAllTrails());

        public Command<PinClickedEventArgs> PinClickedCommand => new Command<PinClickedEventArgs>(PinClicked);

        public Command<EventArgs> RecordClickedCommand => new Command<EventArgs>(RecordClicked);

        public Command<EventArgs> CancelRecordCommand => new Command<EventArgs>(CancelClicked);

        public Command<InfoWindowClickedEventArgs> CalloutClickedCommand => new Command<InfoWindowClickedEventArgs>(e => 
            {
                if (e.Pin.Tag is SectorMapModel sectorMapModel)
                    GoToSectorPage(sectorMapModel.SectorId);
            });

        //todo: remove
        public Command<EventArgs> ShowRecordsClicked => new Command<EventArgs>(e => {
            if (isRecordInProgress)
                return;
            isShowingRecords = !isShowingRecords;
            if (isShowingRecords)
            {
                foreach (var record in recordsPolylines)
                {
                    Polylines.Add(record);
                }
            }
            else {
                foreach (var record in recordsPolylines)
                {
                    Polylines.Remove(record);
                }
            }
        });


        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;

            isLoaded = true;

            var activeCrag = await cragRepository.GetAsync(Settings.ActiveCrag);
            var position = activeCrag.crag_latitude.HasValue && activeCrag.crag_longitude.HasValue ?
                                     new Position(activeCrag.crag_latitude.Value, activeCrag.crag_longitude.Value)
                                     : new Position(51.047, -115.36);//Canmore
            Region = MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(10));

            PageHeaderText = activeCrag.crag_name;

            if (!(TrailCollector.Instance.InitTask?.IsCompleted ?? true))
                await TrailCollector.Instance.InitTask;

            TrailCollector.Instance.LocationAdded += ShowUserTrail;

            RaisePropertyChanged(nameof(RecordBtnImg));
            RaisePropertyChanged(nameof(IsCancelRecordVisible));

            if (Device.RuntimePlatform == Device.Android)
            {
                await Task.Delay(1000); //hack for map to work properly on Android, from GoogleMaps package author
            }

            if (isRecordInProgress)
                ShowUserTrail();

            await GetMapDataAsync();

            //center the map on all pins
            var positions = Pins.Select(p => p.Position);
            if (positions.Any())
                Region = MapSpan.FromPositions(positions).WithZoom(0.8);
        }

        async Task GetMapDataAsync() 
        {
            var activeCragId = Settings.ActiveCrag;
            var sectors = await sectorRepository.GetAsync(s => s.crag_id == activeCragId);
            sectors = sectors.OrderBy(s => s.sort_order).ToList();


            for (int i = 0; i < sectors.Count; i++) 
            {
                var sector = sectors[i];
                if (sector.latitude == null || sector.longitude == null) {
                    continue;
                }

                var sectorCustomModel = new SectorMapModel { 
                    SectorId = sector.sector_id,
                    SectorName = sector.sector_name
                };
                AssignImageBytesAsync(sectorCustomModel);//to run in BG

                var pin = new Pin
                {
                    Position = new Position(sector.latitude.Value, sector.longitude.Value),
                    Label = sector.sector_name,
                    Icon = BitmapDescriptorFactory.FromBundle("icon_pin_sector.png"),
                    Tag = sectorCustomModel
                };
                Pins.Add(pin);
            }

            var parkings = await parkingRepository.GetAsync(p => p.crag_id == activeCragId);
            foreach (var parking in parkings)
            {
                var parkingPin = new Pin
                {
                    Position = new Position(parking.latitude, parking.longitude),
                    Label = "",
                    Icon = BitmapDescriptorFactory.FromBundle("parking.png")
                };
                Pins.Add(parkingPin);
            }


            var trails = await trailRepository.GetAsync(t => t.crag_id == activeCragId);

            foreach (var trail in trails) {
                var polyline = new Polyline();
                polyline.StrokeWidth = 4;
                polyline.StrokeColor = Color.FromHex(trail.HexColor);
                polyline.Tag = trail;
                var coordinates = JsonConvert.DeserializeObject<List<LocationModel>>(trail.LocationsJson);
                if (coordinates.Count < 2)
                    continue;
                foreach (var coordinate in coordinates)
                    polyline.Positions.Add(coordinate.ToPosition());

                if (trail.sector_start_id.HasValue)
                {
                    var sectorStart = await sectorRepository.GetAsync(trail.sector_start_id.Value);
                    if (sectorStart.latitude.HasValue && sectorStart.longitude.HasValue)
                    {
                        var sectorStartPos = new Position(sectorStart.latitude.Value, sectorStart.longitude.Value);
                        polyline.Positions.Insert(0, sectorStartPos);
                    }
                }
                else if (trail.parking_start_id.HasValue)
                {
                    var parkingStart = await parkingRepository.GetAsync(trail.parking_start_id.Value);
                    var parkingStartPos = new Position(parkingStart.latitude, parkingStart.longitude);
                    polyline.Positions.Insert(0, parkingStartPos);
                }

                if (trail.sector_end_id.HasValue)
                {
                    var sectorEnd = await sectorRepository.GetAsync(trail.sector_end_id.Value);
                    if (sectorEnd.latitude.HasValue && sectorEnd.longitude.HasValue)
                    {
                        var sectorEndPos = new Position(sectorEnd.latitude.Value, sectorEnd.longitude.Value);
                        polyline.Positions.Add(sectorEndPos);
                    }
                }
                else if (trail.parking_end_id.HasValue)
                {
                    var parkingEnd = await parkingRepository.GetAsync(trail.parking_end_id.Value);
                    var parkingEndPos = new Position(parkingEnd.latitude, parkingEnd.longitude);
                    polyline.Positions.Add(parkingEndPos);
                }

                if (!isRecordInProgress)
                    Polylines.Add(polyline);
                allPolylines.Add(polyline);
            }

            //testing block. TODO: Remove
            var records = await new HttpHelper().GetAsync<ServerTrailRecords[]>(ApiUrls.Url_M_GetTrailRecords(activeCragId));
            if (!records.ValidateResponse())
                return;
            foreach (var record in records.Result)
            {
                var polyline = new Polyline();
                polyline.StrokeWidth = 2;
                switch (record.status)
                {
                    case (int)TrailStatus.Confirmed:
                        polyline.StrokeColor = Color.Green;
                        break;
                    case (int)TrailStatus.Pending:
                        polyline.StrokeColor = Color.Gray;
                        break;
                    default:
                        polyline.StrokeColor = Color.Red;
                        break;
                }
                polyline.Tag = record;
                var coordinates = JsonConvert.DeserializeObject<List<LocationModel>>(record.locations);
                if (coordinates.Count < 2)
                    continue;
                foreach (var coordinate in coordinates)
                    polyline.Positions.Add(coordinate.ToPosition());
                recordsPolylines.Add(polyline);
            }
            //
        }

        async void AssignImageBytesAsync(SectorMapModel sectorMap) {
            sectorMap.SectorImageBytes = await MapModelHelper.GetSectorImageBytes(sectorMap.SectorId);
        }

        async void GoToSectorPage(int sectorId)
        {
            var sectorModel = await MapModelHelper.GetFromSectorIdAsync(sectorId);

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, sectorModel);
            navigationService.NavigateAsync<SectorRoutesViewModel>(navigationParameters);
        }

        void ShowAllTrails() {
            if (isRecordInProgress)
                return;

            Polylines.Clear();
            foreach (var poly in allPolylines)
                Polylines.Add(poly);

            //todo: remove
            if (isShowingRecords)
                foreach (var poly in recordsPolylines)
                    Polylines.Add(poly);
        }

        async void ShowUserTrail(object sender = null, Plugin.Geolocator.Abstractions.Position p = null) {
            if (!isRecordInProgress)
                return;

            Polylines.Clear();
            var polyline = new Polyline();
            polyline.StrokeWidth = 2;
            polyline.StrokeColor = Color.Blue;
            var coordinates = await new Repository<UserLocationTable>().GetAsync(l => l.user_trail_id == TrailCollector.Instance.ActiveRecord.id);
            if (coordinates.Count < 2)
                return;
            foreach (var coordinate in coordinates)
                polyline.Positions.Add(new Position(coordinate.latitude, coordinate.longitude));
            Polylines.Add(polyline);
        }

        void PinClicked(PinClickedEventArgs e)
        {
            if (isRecordInProgress)
                return;

            if (e.Pin.Tag == null) {
                e.Handled = true;
                ShowAllTrails();
                return;
            }

            var sectorId = (e.Pin.Tag as SectorMapModel).SectorId;
            var filtered = allPolylines.Where(p => TouchesSector((ITrail)p.Tag, sectorId));
            Polylines.Clear();
            foreach (var poly in filtered)
            {
                Polylines.Add(poly);
            }

            if (isShowingRecords) { 
                var filteredRecords = recordsPolylines.Where(p => TouchesSector((ITrail)p.Tag, sectorId));
                foreach (var poly in filteredRecords)
                {
                    Polylines.Add(poly);
                }
            }
        }

        async void RecordClicked(EventArgs obj)
        {
            var parameters = new NavigationParameters();
            parameters.Add("poiSelected", new Action<int, bool>(OnPOISelectorOkClicked));
            await navigationService.NavigateAsync<POISelectionViewModel>(parameters, useModalNavigation: true);
        }

        private async void CancelClicked(EventArgs obj)
        {
            try
            {
                await TrailCollector.Instance.CancelCurrentRecordingAsync();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, "Error");
                return;
            }
            RaisePropertyChanged(nameof(RecordBtnImg));
            RaisePropertyChanged(nameof(IsCancelRecordVisible));
            ShowAllTrails();
        }

        async void OnPOISelectorOkClicked(int id, bool fromParking) {
            if (!isRecordInProgress)
            {
                try
                {
                    await TrailCollector.Instance.StartUpdatesAsync(id, fromParking);
                }
                catch
                {
                    return;
                }
                RaisePropertyChanged(nameof(RecordBtnImg));
                RaisePropertyChanged(nameof(IsCancelRecordVisible));
                ShowUserTrail();
            }
            else
            {
                try
                {
                    await TrailCollector.Instance.FinishUpdatesAsync(id, fromParking);
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Alert(ex.Message, "Error");
                    return;
                }
                RaisePropertyChanged(nameof(RecordBtnImg));
                RaisePropertyChanged(nameof(IsCancelRecordVisible));
                ShowAllTrails();
            }
        }

        bool TouchesSector(ITrail trail, int sectorId) => trail.sector_start_id == sectorId || trail.sector_end_id == sectorId;

        public override void Destroy()
        {
            base.Destroy();
            TrailCollector.Instance.LocationAdded -= ShowUserTrail;
        }



        //todo: remove
        class ServerTrailRecords : ITrail
        {
            public int Id { get; set; }
            public int user_id { get; set; }
            public long crag_id { get; set; }
            public string locations { get; set; }
            public DateTime? start_time { get; set; }
            public DateTime? end_time { get; set; }
            public int? sector_start_id { get; set; }
            public int? sector_end_id { get; set; }
            public int? parking_start_id { get; set; } //if -1 -> starts from not existing parking. We should insert new entry to TPARKING with first coordinate from locations
            public int? parking_end_id { get; set; } //if -1 -> ends at not existing parking. We should insert new entry to TPARKING with last coordinate from locations
            public byte status { get; set; } //is one of TrailStatus enum
            public int? trail_id { get; set; }
        }
        public enum TrailStatus { Pending, Confirmed, RejectedTrailCreate, RejectedTrailAdjust, RejectedFarFromStart, RejectedFarFromEnd, RejectedByModerator }
    }
}
