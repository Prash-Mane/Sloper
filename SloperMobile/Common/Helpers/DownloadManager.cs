using System;
using SloperMobile.Common.Interfaces;
using System.Collections.Generic;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.CragModels;
using System.Threading.Tasks;
using System.Linq;
using Acr.UserDialogs;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using SloperMobile.UserControls.PopupControls;
using System.Threading;

namespace SloperMobile
{
    public class DownloadManager : IDownloadManager
    {
        IDownloadCragService downloadCragService;
        INotificationService notificationService;
        IUserDialogs userDialogs;

        Task<bool> downloadingTask;
        Queue<CragInfoModel> downloadingQueue = new Queue<CragInfoModel>();
        CancellationTokenSource cts = new CancellationTokenSource();

        public IEnumerable<CragInfoModel> DownloadingQueue => downloadingQueue;

        /// <summary>
        /// To handle ProgressView on CragDetails Page.
        /// </summary>
        public event EventHandler<(string message, decimal percents)> PopupProgressChanged;

        bool isdisplayToast;

        public DownloadManager(IDownloadCragService downloadCragService, IUserDialogs userDialogs)
        {
            this.downloadCragService = downloadCragService;
            this.userDialogs = userDialogs;
            notificationService = DependencyService.Get<INotificationService>(DependencyFetchTarget.NewInstance);
            if (notificationService != null)
                notificationService.NotificationType = NotificationTypes.DMProgress;
            downloadCragService.ProgressChanged += OnProgressChanged;
            CrossConnectivity.Current.ConnectivityChanged += ConnectivityChanged;
        }

        public void AddCragsToDownload(IEnumerable<CragInfoModel> crags)
        {
            foreach (var cragInfo in crags)
            {
                if (downloadingQueue.Any(ci => ci.CragID == cragInfo.CragID))
                {
                    cragInfo.State = CragInfoModel.CragStatus.DownloadQueued;
                    continue;
                }

                cragInfo.State = CragInfoModel.CragStatus.DownloadQueued;
                downloadingQueue.Enqueue(cragInfo);
            }

            DownloadItemsInQueue();
        }

        public void StopDownloadingCrag(int cragId)
        {
            var cragInfo = downloadingQueue.FirstOrDefault(ci => ci.CragID == cragId);
            if (cragInfo == null)
                return;
            if (cragInfo.State == CragInfoModel.CragStatus.DownloadQueued)
                cragInfo.State = CragInfoModel.CragStatus.Default;
			if (cragInfo.State == CragInfoModel.CragStatus.Downloading)
			{
				cragInfo.State = CragInfoModel.CragStatus.Cancellation;
				cragInfo.ProgressValue = 0;
				cts.Cancel();
				cts = new CancellationTokenSource();
			}
		}

        public void StopAll()
        {
            foreach (var cragInfo in downloadingQueue)
            {
                if (cragInfo.State == CragInfoModel.CragStatus.DownloadQueued)
                    cragInfo.State = CragInfoModel.CragStatus.Default;
                if (cragInfo.State == CragInfoModel.CragStatus.Downloading)
                {
                    cragInfo.State = CragInfoModel.CragStatus.Cancellation;
                    cragInfo.ProgressValue = 0;
                }
            }
            cts.Cancel();
            cts = new CancellationTokenSource();
            downloadingQueue.Clear();
            notificationService?.EndProgress();
        }

        public async Task DownloadItemsInQueue()
        {
            if (downloadingTask != null)
                return;

            if (!CrossConnectivity.Current.IsConnected)
            {
                NavigateToErrorPage();
                return;
            }

            while (downloadingQueue.Count > 0)
            {
                var currentCrag = downloadingQueue.Peek();
                if (currentCrag.State != CragInfoModel.CragStatus.DownloadQueued) //it may be pending for removal or is downloaded already
                {
                    if (downloadingQueue.Contains(currentCrag))
                        downloadingQueue.Dequeue();
                    continue;
                }

                currentCrag.State = CragInfoModel.CragStatus.Downloading;
                downloadingTask = downloadCragService.DownloadAsync(currentCrag.CragID, cts.Token);
                var success = await downloadingTask;

				if (currentCrag.State == CragInfoModel.CragStatus.Cancellation) //not success, but it's manually cancelled
				{
					currentCrag.State = CragInfoModel.CragStatus.Default;
					currentCrag.ProgressValue = 0;
                    if (downloadingQueue.Contains(currentCrag))
                        downloadingQueue.Dequeue();
					continue;
				}

				if (!success)
                {
                    currentCrag.State = CragInfoModel.CragStatus.DownloadQueued;
                    currentCrag.ProgressValue = 0;
                    downloadingTask = null;
                    notificationService?.EndProgress();
                    return;
                }

                currentCrag.State = CragInfoModel.CragStatus.Downloaded;
                if (downloadingQueue.Contains(currentCrag))
                    downloadingQueue.Dequeue();

                if (isdisplayToast)
                {
                    UserDialogs.Instance.Toast(new ToastConfig("")
                    {
                        Message = $" Download {currentCrag.CragName} finished!",
                        BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                        MessageTextColor = System.Drawing.Color.White,
                        Duration = TimeSpan.FromSeconds(3)
                    });
                }
            }
            notificationService?.EndProgress();
            downloadingTask = null;
        }

        void OnProgressChanged(object sender, (string message, decimal percents) args)
        {
            var dbCrag = sender as CragExtended;
            var cragInfo = downloadingQueue.FirstOrDefault(ci => ci.CragID == dbCrag.crag_id);

            if (cragInfo == null)
                return;

            cragInfo.ProgressValue = args.percents;
            cragInfo.ProgressStatus = args.message;

            if (notificationService != null)
            {
                var queuedCount = DownloadingQueue.Count(c => c.State == CragInfoModel.CragStatus.DownloadQueued || c.State == CragInfoModel.CragStatus.Downloading);
                var notificationTitle = $"Downloading {dbCrag.crag_name} ({queuedCount} left)";
                notificationService.UpdateProgress(args.message, (int)(args.percents * 100), notificationTitle); 
            }

            PopupProgressChanged?.Invoke(sender, args);
        }

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

        void ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected && DownloadingQueue.Any())
                DownloadItemsInQueue();
        }

        public void IsDisplayToast(bool value)
        {
            isdisplayToast = value;
        }
    }
}
