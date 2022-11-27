using Acr.UserDialogs;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.CragModels;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.ViewModel.MasterDetailViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SloperMobile.Common.Helpers
{
    public class RemoveCragManager : IRemoveCragManager
    {
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<IssueTable> tissueRepository;
        private readonly IRepository<BucketTable> bucketRepository;
        private readonly IRepository<MapTable> mapRepository;
        private readonly IRepository<CragSectorMapTable> cragSectorMapRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<TrailTable> trailRepository;
        private readonly IHttpHelper httpHelper;
        private readonly IDownloadCragService downloadCragService;
        private readonly IExceptionSynchronizationManager exceptionManager;

        Task<bool> removingTask;
        Queue<CragInfoModel> removingQueue = new Queue<CragInfoModel>();
        CancellationTokenSource cts = new CancellationTokenSource();

        public IEnumerable<CragInfoModel> RemovingQueue => removingQueue;

        private int progressCounter = 0;
        private int TotalItemsForRemove = 10;

        public RemoveCragManager(
            IRepository<CragExtended> cragRepository,
            IDownloadCragService downloadCragService,
            IUserDialogs userDialogs,
            IRepository<IssueTable> tissueRepository,
            IRepository<BucketTable> bucketRepository,
            IRepository<MapTable> mapRepository,
            IRepository<CragSectorMapTable> cragSectorMapRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IHttpHelper httpHelper,
            IExceptionSynchronizationManager exceptionManager)
        {
            this.cragRepository = cragRepository;
            this.downloadCragService = downloadCragService;
            this.httpHelper = httpHelper;
            this.tissueRepository = tissueRepository;
            this.bucketRepository = bucketRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
            this.mapRepository = mapRepository;
            this.cragSectorMapRepository = cragSectorMapRepository;
            this.exceptionManager = exceptionManager;
        }

        public void AddCragsToRemove(IEnumerable<CragInfoModel> crags)
        {
            foreach (var cragInfo in crags)
            {
                if (removingQueue.Any(ci => ci.CragID == cragInfo.CragID))
                {
                    cragInfo.State = CragInfoModel.CragStatus.RemoveQueued;
                    continue;
                }

                cragInfo.State = CragInfoModel.CragStatus.RemoveQueued;
                removingQueue.Enqueue(cragInfo);
            }
            RemoveItemsInQueue();
        }

        //public void StopRemovingCrag(int cragId)
        //{
            //var cragInfo = removingQueue.FirstOrDefault(ci => ci.CragID == cragId);
            //if (cragInfo == null)
                //return;
            //if (cragInfo.State == CragInfoModel.CragStatus.RemoveQueued)
            //    cragInfo.State = CragInfoModel.CragStatus.Default;
            //if (cragInfo.State == CragInfoModel.CragStatus.Removing)
            //{
            //    cragInfo.State = CragInfoModel.CragStatus.Cancellation;
            //    //cragInfo.ProgressValue = 1;
            //    cts.Cancel();
            //    cts = new CancellationTokenSource();
            //}
        //}

        public async void RemoveItemsInQueue()
        {
            if (removingTask != null)
                return;

            if (!CrossConnectivity.Current.IsConnected)
            {
                NavigateToErrorPage();
                return;
            }

            while (removingQueue.Count > 0)
            {
                var currentCrag = removingQueue.Peek();
                if (currentCrag.State != CragInfoModel.CragStatus.RemoveQueued) //it may be pending for download or is removed already
                {
                    removingQueue.Dequeue();
                    continue;
                }
                currentCrag.State = CragInfoModel.CragStatus.Removing;
                removingTask = RemoveCrag(currentCrag);
                var success = await removingTask;

                //if (currentCrag.State == CragInfoModel.CragStatus.Cancellation) //not success, but it's manually cancelled
                //{
                //    currentCrag.State = CragInfoModel.CragStatus.Downloaded;
                //    currentCrag.ProgressValue = 1;
                //    removingQueue.Dequeue();
                //    continue;
                //}

                if (!success)
                {
                    currentCrag.State = CragInfoModel.CragStatus.RemoveQueued;
                    currentCrag.ProgressValue = 1;
                    removingTask = null;
                    return;
                }
                currentCrag.State = CragInfoModel.CragStatus.Default;
                removingQueue.Dequeue();
            }

            removingTask = null;
        }

        private async Task<bool> RemoveCrag(CragInfoModel SelectedCrag)
        {
            if (!Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UserDialogs.Instance.HideLoading();
                    UserDialogs.Instance.AlertAsync("Removing Requires an Internet Connection.", "Network Error", "Continue");
                });
                return false;
            }

            try
            {
                var cragObject = await cragRepository.FindAsync(c => c.crag_id == SelectedCrag.CragID);

                UpdateProgressOnRemove(SelectedCrag, "Removing\nCrag News");
                var responseRole = await HttpRemoveCragRole(cragObject.crag_role_id);
                if (responseRole.ErrorMessage == AppConstant.CANCELLED)
                    return false;

                UpdateProgressOnRemove(SelectedCrag, "Updating\nDownload Status");
                var responseCH = await HttpRemoveUserCragHistory(SelectedCrag.CragID);
                if (responseCH.ErrorMessage == AppConstant.CANCELLED)
                    return false;

                UpdateProgressOnRemove(SelectedCrag, "Removing\nRoute Issues");
                await tissueRepository.ExecuteAsync($@"DELETE FROM TISSUE WHERE (route_id in (
            SELECT DISTINCT T_ROUTE.route_id FROM T_ROUTE JOIN T_SECTOR ON T_ROUTE.sector_id = T_SECTOR.sector_id
            WHERE T_SECTOR.crag_id = {SelectedCrag.CragID} ))");

                UpdateProgressOnRemove(SelectedCrag, "Removing\nTopos");
                await bucketRepository.ExecuteAsync($"DELETE FROM T_TOPO WHERE sector_id in (SELECT DISTINCT sector_id FROM T_SECTOR WHERE crag_id= {SelectedCrag.CragID} )");

                UpdateProgressOnRemove(SelectedCrag, "Removing\nParking Lots");
                await parkingRepository.RemoveAsync(p => p.crag_id == SelectedCrag.CragID);
                await trailRepository.RemoveAsync(t => t.crag_id == SelectedCrag.CragID);

                UpdateProgressOnRemove(SelectedCrag, "Removing\nMaps");
                await mapRepository.RemoveAsync(m => m.crag_id == SelectedCrag.CragID);

                UpdateProgressOnRemove(SelectedCrag, "Removing\nCrag");
                await cragSectorMapRepository.RemoveAsync(mr => mr.crag_id == SelectedCrag.CragID);

                cragObject.is_downloaded = false;
                await cragRepository.UpdateAsync(cragObject);

                var downloadedCrag = (await cragRepository.GetAsync(c => c.is_enabled
                                                                && c.is_app_store_ready
                                                                && c.is_downloaded
                                                                && c.crag_latitude != null
                                                                && c.crag_longitude != null
                                                                && c.crag_latitude != 0
                                                                && c.crag_longitude != 0)
                                  ).OrderBy(crag => crag.crag_sort_order)
                                   .FirstOrDefault();
                Settings.MapSelectedCrag = Settings.ActiveCrag = downloadedCrag?.crag_id ?? 0;

                UpdateProgressOnRemove(SelectedCrag, "Updating\nGrades");
                await downloadCragService.GetAndSaveGradesAsync();

                UpdateProgressOnRemove(SelectedCrag, "Updating\nGrade Buckets");
                await downloadCragService.GetAndSaveBucketsAsync();

                UpdateProgressOnRemove(SelectedCrag, "Updating\nTech Grades");
                await downloadCragService.GetAndSaveTechGradesAsync();

                UpdateProgressOnRemove(SelectedCrag, "Crag Removal\nFinished!");

                //UpdateMenuList();

                GeneralHelper.CragModified?.Invoke(this, cragObject);
                await GeneralHelper.UpdateCragsDownloadedStateAsync();
                progressCounter = 0;
            }
            catch(Exception ex) {
                if (ex is OperationCanceledException || ex is TaskCanceledException)
                    return false;
                else
                    throw;
            }
            return true;
        }

        private async Task<OperationResult<CragHistoryModel>> HttpRemoveUserCragHistory(int selectedCragId)
            => await httpHelper.GetAsync<CragHistoryModel>(ApiUrls.Url_M_RemoveUserCragHistory(selectedCragId), cancellationToken: cts.Token);

        private async Task<OperationResult<string>> HttpRemoveCragRole(int? crag_role_id)
            => await httpHelper.PostAsync<string>(ApiUrls.Url_SloperUser_RemoveRole(crag_role_id), string.Empty, cancellationToken: cts.Token);


        void UpdateProgressOnRemove(CragInfoModel cragInfo, string message, int increment = 1)
        {
            if (progressCounter > 2) //at this point we're not able to do a rollback and have to continue remove
                cragInfo.State = CragInfoModel.CragStatus.Removing;
            //cts.Token.ThrowIfCancellationRequested(); //we won't be able to rollback/restore state if cancelled
            progressCounter += increment;
            var progress = Decimal.Divide(progressCounter, TotalItemsForRemove);
            cragInfo.ProgressValue = 1 - progress;
        }

        //public async void UpdateMenuList()
        //{
        //    try
        //    {
        //        MainMasterDetailViewModel.Instance?.FillMenuItems();
        //    }
        //    catch (Exception exception)
        //    {
        //        await exceptionManager.LogException(new ExceptionTable
        //        {
        //            Method = nameof(this.UpdateMenuList),
        //            Page = this.GetType().Name,
        //            StackTrace = exception.StackTrace,
        //            Exception = exception.Message,
        //        });
        //    }
        //}


        async void NavigateToErrorPage()
        {
            if (PopupNavigation.PopupStack.Any())
                await PopupNavigation.PopAllAsync(false);


            Device.BeginInvokeOnMainThread(async () =>
            {
                if (Device.RuntimePlatform == Device.iOS) //ios is not able to display page if there's alert visible
                {
                    var helper = DependencyService.Get<IAlertsHelper>();
                    await helper.CloseAll();
                }
                await PopupNavigation.PushAsync(new NetworkErrorPopup(), true);
            });
        }
    }
}
