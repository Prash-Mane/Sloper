using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.UserControls.CustomControls;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;
using System.Text;
using System.Diagnostics;
using Plugin.Geolocator;
using SloperMobile.Model.CragModels;

namespace SloperMobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoogleMapPinPage : ContentPage, INavigationAware, IDestructible
    {
        bool isLoaded;

        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<CragImageTable> cragImageRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IExceptionSynchronizationManager exceptionSynchronizationManager;
        private readonly IPurchasedCheckService purchasedCheckService;

        public GoogleMapPinPage(
            IRepository<CragImageTable> cragImageRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IExceptionSynchronizationManager exceptionSynchronizationManager,
            IPurchasedCheckService purchasedCheckService)
        {
            this.cragRepository = cragRepository;
            this.cragImageRepository = cragImageRepository;
            this.userInfoRepository = userInfoRepository;
            this.exceptionSynchronizationManager = exceptionSynchronizationManager;
            this.purchasedCheckService = purchasedCheckService;
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            GeneralHelper.CragModified += CragModified;
            GeneralHelper.AppOrGuidebookPurchased += AppOrGBPurchased;
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        { }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            if (isLoaded)
                return;

            isLoaded = true;
            InitializeMap();
        }

        private async Task InitializeMap()
        {
            try
            {
                map.UiSettings.ZoomControlsEnabled = false;
                var mapSpan = GetMapSpanForPoint(GeneralHelper.DefaultPosition.Latitude, GeneralHelper.DefaultPosition.Longitude);

                if (Device.RuntimePlatform == Device.Android)
                    mapHolder.Children.Remove(map);
                else
                    map.MoveToRegion(mapSpan, false);

                map.MyLocationEnabled = CrossGeolocator.Current.IsGeolocationEnabled;

                map.Pins.Clear();

                var activeCragId = Settings.ActiveCrag;

                var cragList = await cragRepository.GetAsync(c => 
                    c.is_app_store_ready 
                    && c.is_enabled 
                    && c.crag_latitude != null
                    && c.crag_latitude != 0
                    && c.crag_longitude != null
                    && c.crag_longitude != 0);

                SetMapStyle();

                if (activeCragId > 0)
                {
                    var cragToCenter = cragList.FirstOrDefault(c => c.crag_id == activeCragId);
                    if (cragToCenter != null)
                    {
                        mapSpan = GetMapSpanForPoint(cragToCenter.crag_latitude.Value, cragToCenter.crag_longitude.Value);
                    }
                }
                else
                {
                    var myLocation = await GeneralHelper.GetMyPositionAsync();

                    if (myLocation != GeneralHelper.DefaultPosition)
                    {
                        var clossestCrag = GetClosestCrag(cragList, myLocation);
                        if (clossestCrag != null)
                        {
                            mapSpan = MapSpan.FromPositions(
                            new List<Position> {
                                    new Position(clossestCrag.crag_latitude.Value, clossestCrag.crag_longitude.Value),
                                    new Position(myLocation.Latitude, myLocation.Longitude)
                            });
                            mapSpan = mapSpan.WithZoom(0.8);
                        }
                    }
                }

                //if (mapSpan == null)
                //{
                //    var cragToCenter = cragList.Where(c => c.HasLocation && c.IsGoogleEnabled && c.IsiTunesEnabled && c.is_enabled)
                //                           .OrderBy(c => c.crag_sort_order)
                //                           .FirstOrDefault();

                //    if (cragToCenter != null)
                //    {
                //        mapSpan = GetMapSpanForPoint(cragToCenter.crag_latitude.Value, cragToCenter.crag_longitude.Value);
                //    }
                //    else
                //        Debug.WriteLine("No crags with location found!");
                //}


                Device.BeginInvokeOnMainThread(() =>
                {
                    map?.MoveToRegion(mapSpan);

                    SetCragPins(cragList);

                    if (map != null)
                    {
                        map.InfoWindowClicked -= Map_InfoWindowClicked;
                        map.InfoWindowClicked += Map_InfoWindowClicked;
                    }
                });

                if (Device.RuntimePlatform == Device.Android)
                {
                    await Task.Delay(700);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (mapHolder != null && map != null)
                            mapHolder.Children.Insert(0, map);
                    });
                }
                else
                {
                    await Task.Delay(100);
                    if (map.VisibleRegion.Center != mapSpan.Center)
                        map.MoveToRegion(mapSpan);
                }
            }
            catch (Exception exception)
            {
                await exceptionSynchronizationManager.LogException(new ExceptionTable
                {
                    Method = nameof(OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = JsonConvert.SerializeObject(Settings.MapSelectedCrag)
                });
            }
        }

        private void SetMapStyle()
        {
            var appassembly = typeof(App).GetTypeInfo().Assembly;
            Stream stream = appassembly.GetManifestResourceStream($"SloperMobile.mapStyle.json");
            string myJson;
            using (var reader = new StreamReader(stream))
            {
                myJson = reader.ReadToEnd();
            }
            map.MapStyle = MapStyle.FromJson(myJson);
        }

        private async Task SetCragPins(IEnumerable<CragExtended> crags)
        {
            foreach (var tcrag in crags)
            {
                var filename = GetIconName(tcrag);

                var pinIcon = BitmapDescriptorFactory.FromBundle(filename);

                var cragImage = await cragImageRepository.FindAsync(tci => tci.crag_id == tcrag.crag_id);
                tcrag.crag_image = cragImage?.crag_image;

                string address = "";
                if (AppSetting.APP_TYPE == "indoor")
                    address = tcrag.crag_parking_info;
                else
                {
                    address = tcrag.area_name;
                }

                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = new Position(Convert.ToDouble(tcrag.crag_latitude, CultureInfo.InvariantCulture), Convert.ToDouble(tcrag.crag_longitude, CultureInfo.InvariantCulture)),
                    Label = tcrag.crag_name,
                    Address = address,
                    Tag = tcrag,
                    Icon = pinIcon,
                    ZIndex = tcrag.is_downloaded ? 5 : 1,

                };
                map.Pins.Add(pin);
            }
        }

        private MapSpan GetMapSpanForPoint(double latitude, double longitude)
                        => MapSpan.FromCenterAndRadius(new Position(latitude, longitude), Distance.FromMiles(10));

        void Map_InfoWindowClicked(object sender, InfoWindowClickedEventArgs e)
        {
            InfoWindowClickedAsync(e);
        }

        async Task InfoWindowClickedAsync(InfoWindowClickedEventArgs e)
        {
            try
            {
                var cragid = (e.Pin.Tag as CragExtended).crag_id;

                Settings.MapSelectedCrag = cragid;

                // check if crag is downloaded, if so set it as Current Crag - Steve
                var isDownloaded = await cragRepository.FindAsync(x => x.crag_id == cragid && x.is_downloaded);
                if (isDownloaded != null)
                {
                    Settings.ActiveCrag = cragid;
                    //MessagingCenter.SendModel<GoogleMapPinPage>(this, "ChangeSelectedItemColor");
                }

                (BindingContext as GoogleMapPinsViewModel).NavigateToCragDetails(cragid);
            }
            catch (Exception exception)
            {
                await exceptionSynchronizationManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.Map_InfoWindowClicked),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = JsonConvert.SerializeObject(e.Pin)
                });
            }
        }

        CragExtended GetClosestCrag(IEnumerable<CragExtended> crags, Plugin.Geolocator.Abstractions.Position myLocation)
        {
            try
            {
                var closest = crags.OrderBy(c => Plugin.Geolocator.Abstractions.GeolocatorUtils.CalculateDistance(myLocation.Latitude, myLocation.Longitude, c.crag_latitude.Value, c.crag_longitude.Value))
                                   .FirstOrDefault();

                return closest;

            }
            catch (Exception exception)
            {
                //await exceptionSynchronizationManager.LogException(new ExceptionTable
                //{
                //    Method = nameof(this.GetClosestCrag),
                //    Page = this.GetType().Name,
                //    StackTrace = exception.StackTrace,
                //    Exception = exception.Message,
                //    Data = $"crags = {JsonConvert.SerializeObject(crags)}, location = {myLocation}"
                //});
            }

            return null;
        }

        private void CragModified(object sender, CragExtended obj)
        {
            UpdatePin(obj);
        }

        private void UpdatePin(CragExtended crag)
        {
            var pin = map.Pins?.FirstOrDefault(p => (p.Tag as CragExtended)?.crag_id == crag.crag_id);
            if (pin == null)
                return;

            var filename = GetIconName(crag);
            var pinIcon = BitmapDescriptorFactory.FromBundle(filename);
            pin.Icon = pinIcon;
            pin.Tag = crag;
        }

        private string GetIconName(CragExtended crag)
        {
            string filename;
            var builder = new StringBuilder("icon_pin_");
            if (crag.Unlocked)
            {
                switch (crag.status)
                {
                    case ("In Season"):
                        builder.Append("green_");
                        break;
                    case ("Out of Season"):
                        builder.Append("yellow_");
                        break;
                    case ("Closed"):
                        builder.Append("red_");
                        break;
                }
                if (!crag.is_downloaded)
                    builder.Append("faded_");
                builder.Append("30h.png");
                filename = builder.ToString();
            }
            else
            {
                builder.Append("black_");
                if (!crag.is_downloaded)
                    builder.Append("faded_");
                builder.Append("30h.png");
                filename = builder.ToString();
            }

            return filename;
        }

        void AppOrGBPurchased(object sender, AppProductTable product) {
            AppOrGBPurchasedAsyncWrapper();
        }

        async Task AppOrGBPurchasedAsyncWrapper()
        {
            map.Pins.Clear();
            var cragList = await cragRepository.GetAsync();
            SetCragPins(cragList);
        }

        public void Destroy()
        {
            GeneralHelper.CragModified -= CragModified;
            GeneralHelper.AppOrGuidebookPurchased -= AppOrGBPurchased;
        }
    }
}