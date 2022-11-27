using System;
using System.Linq;
using System.Threading.Tasks;
using SloperMobile.DataBase;
using SloperMobile.Common.Helpers;
using System.Diagnostics;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Connectivity;
using Xamarin.Forms;
using SloperMobile.Common.Interfaces;
using Acr.UserDialogs;
using SloperMobile.Common.Constants;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;

namespace SloperMobile
{
    public class TrailCollector
    {
        readonly IRepository<UserTrailRecordsTable> userTrailRepository = new Repository<UserTrailRecordsTable>();
        readonly IRepository<UserLocationTable> userLocationRepository = new Repository<UserLocationTable>();
        readonly IHttpHelper httpHelper = new HttpHelper();

        public UserTrailRecordsTable ActiveRecord { get; private set; }
        public Task InitTask { get; private set; }

        IBgLocationHelper bgHelper => DependencyService.Get<IBgLocationHelper>();
        ListenerSettings listenerSettings = new ListenerSettings
                    {
                        AllowBackgroundUpdates = true,
                        ActivityType = ActivityType.Fitness,
                        DeferLocationUpdates = true,
                        DeferralDistanceMeters = 10,
                        DeferralTime = TimeSpan.FromSeconds(5),
                        PauseLocationUpdatesAutomatically = false
                    };

        public event EventHandler<Position> LocationAdded;

        DateTime lastPointAddedDate = DateTime.UtcNow;

        static TrailCollector instance;

        public static TrailCollector Instance { 
            get {
                if (instance == null)
                {
                    instance = new TrailCollector();
                    instance.InitTask = instance.CheckActiveRecordAsync();
                }

                return instance;
            }  
        }

        private TrailCollector() { }

        async Task CheckActiveRecordAsync() 
        {
            var currentUserId = Settings.UserID;
            var runningRecords = await userTrailRepository.GetAsync(t => t.end_time == null && t.user_id == currentUserId);
            if (runningRecords.Count > 1)
            {
                Debug.WriteLine("Multiple records are executed at the same time!");
                for (int i = 0; i < runningRecords.Count - 1; i++) {
                    var record = runningRecords[i];
                    record.end_time = DateTime.UtcNow;
                }
                await userTrailRepository.UpdateAllAsync(runningRecords);
            }

            ActiveRecord = runningRecords.LastOrDefault();

            if (ActiveRecord != null)
            {
                try
                {
                    await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, listenerSettings);
                    bgHelper?.StartBGUpdates();
                    CrossGeolocator.Current.PositionChanged += PositionChanged;
                }
                catch { }
            }
        }

        /// <summary>
        /// Creates new UserTrailRecords entry and starts tracking for location changes. Can throw exceptions if somethings wrong with location service
        /// </summary>
        /// <param name="startId">Sector id or parking id for trails start. Use -1 for not existing parking.</param>
        /// <param name="fromParking">Set to <c>true</c> if starts from parking.</param>
        public async Task StartUpdatesAsync(int startId, bool fromParking) 
        {
            try
            {
                bgHelper?.StartBGUpdates();
                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, listenerSettings);
            }
            catch (Exception) {
                throw;
            }

            var newRecord = new UserTrailRecordsTable { 
                user_id = Settings.UserID,
                crag_id = Settings.ActiveCrag,
                start_time = DateTime.UtcNow
            };

            if (fromParking)
                newRecord.parking_start_id = startId;
            else
                newRecord.sector_start_id = startId;

            var id = await userTrailRepository.InsertAsync(newRecord);
            ActiveRecord = newRecord;

            CrossGeolocator.Current.PositionChanged += PositionChanged;
        }

        public async Task FinishUpdatesAsync(int endId, bool isParking)
        {
            CrossGeolocator.Current.StopListeningAsync();
            bgHelper?.StopBGUpdates();

            CrossGeolocator.Current.PositionChanged -= PositionChanged;

            ActiveRecord.end_time = DateTime.UtcNow;
            if (isParking)
                ActiveRecord.parking_end_id = endId;
            else
                ActiveRecord.sector_end_id = endId;

            await userTrailRepository.UpdateAsync(ActiveRecord);

            var tempTrail = ActiveRecord;
            ActiveRecord = null;

            var success = await UploadTrail(tempTrail);
            if (success)
            {
                await userLocationRepository.RemoveAsync(l => l.user_trail_id == tempTrail.id);
                await userTrailRepository.DeleteAsync(tempTrail);
            }
        }

        public async Task CancelCurrentRecordingAsync() 
        {
            CrossGeolocator.Current.StopListeningAsync();
            bgHelper?.StopBGUpdates();

            CrossGeolocator.Current.PositionChanged -= PositionChanged;

            if (ActiveRecord == null)
                return;

            await userLocationRepository.RemoveAsync(l => l.user_trail_id == ActiveRecord.id);
            await userTrailRepository.DeleteAsync(ActiveRecord);
            ActiveRecord = null;
        }

        async void PositionChanged(object sender, PositionEventArgs e)
        {
            if (DateTime.UtcNow - lastPointAddedDate < TimeSpan.FromSeconds(5))
                return;
            lastPointAddedDate = DateTime.UtcNow;

            Debug.WriteLine($"Position changed to: {e.Position.Latitude} : {e.Position.Longitude}");
            //UserDialogs.Instance.Toast($"Posistion changed to: {e.Position.Latitude} : {e.Position.Longitude}");

            if (ActiveRecord == null) //record hasn't been created yet
                return;

            var newPosition = new UserLocationTable { 
                user_trail_id = ActiveRecord.id.Value,
                latitude = e.Position.Latitude,
                longitude = e.Position.Longitude,
                time_utc = DateTime.UtcNow
            };

            userLocationRepository.InsertAsync(newPosition);

            LocationAdded?.Invoke(this, e.Position);
        }

        public async Task UploadAllFinishedToServerAsync() 
        {
            if (!CrossConnectivity.Current.IsConnected)
                return;

            var finishedRecords = await userTrailRepository.GetAsync(t => t.end_time != null);

            foreach (var trail in finishedRecords)
            {
                var success = await UploadTrail(trail);
                if (success)
                {
                    await userLocationRepository.RemoveAsync(l => l.user_trail_id == trail.id);
                    await userTrailRepository.DeleteAsync(trail);
                }
                else
                    continue;
            }
        }

        async Task<bool> UploadTrail(UserTrailRecordsTable trail)
        {
            if (!CrossConnectivity.Current.IsConnected)
                return false;

            var locations = await userLocationRepository.GetAsync(l => l.user_trail_id == trail.id);

            if (locations.Count < 2) //just remove record if not enough points recorded
                return true;

            var trailLocModel = locations.Select(l => new { Latitude = l.latitude, Longitude = l.longitude });
            var locationsString = JsonConvert.SerializeObject(trailLocModel);

            var model = new { 
                trail.user_id,
                trail.crag_id,
                locations = locationsString,
                trail.start_time,
                trail.end_time,
                trail.sector_start_id,
                trail.sector_end_id,
                trail.parking_start_id,
                trail.parking_end_id
            };

            OperationResult<bool> result;
            try
            {
                result = await httpHelper.PostAsync<bool>(ApiUrls.Url_M_UploadTrailRecord, model);
            }
            catch(Exception ex) {
                Debug.WriteLine(ex);
                return false;
            }

            if (!result.ValidateResponse(false))
                return false;

            return result.Result;
        }

        //void PositionError(object sender, PositionErrorEventArgs e)
        //{
        //}
    }
}
