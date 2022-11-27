using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Navigation;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.CragModels;
using Xamarin.Forms;
using System.Diagnostics;

namespace SloperMobile.ViewModel.GuideBookViewModels
{
    public class ManageDownloadsViewModel : BaseViewModel
    {
        bool isLoaded;
        private readonly IDownloadManager downloadManager;
        public ManageDownloadsViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IDownloadManager downloadManager)
            : base(navigationService, exceptionManager)
        {
            this.downloadManager = downloadManager;
            GeneralHelper.CragModified += CragModified;
            DownloadCommand = new Command<CragInfoModel>(ExecuteOnCragDownload);
            IsMenuVisible = true;
            Offset = Common.Enumerators.Offsets.Both;
            IsShowFooter = true;
        }
        public Command DownloadCommand { get; set; }

        public Command StopDownloads => new Command(StopDownloadingAll);

        public int DeviceWidth { get => App.DeviceScreenWidth; }
        public List<CragInfoModel> GuideBookCrags => downloadManager.DownloadingQueue.Where(c=>c.State==CragInfoModel.CragStatus.Downloading || c.State == CragInfoModel.CragStatus.DownloadQueued).ToList();
        public bool ShowEmptyOverlay
        {
            get => (GuideBookCrags == null || GuideBookCrags.Count == 0);
        }

        public bool IsStopDownloadVisible => GuideBookCrags.Any(c => c.State == CragInfoModel.CragStatus.Downloading || c.State == CragInfoModel.CragStatus.DownloadQueued);

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;
            PageHeaderText = "MY DOWNLOADS";
            PageSubHeaderText = "";
            isLoaded = true;
        }
        public void ExecuteOnCragDownload(CragInfoModel gbCrag)
        {
            if (gbCrag.State == CragInfoModel.CragStatus.DownloadQueued)
            {
                GuideBookCrags.Find(gbcrg => gbcrg.CragID == gbCrag.CragID).State = CragInfoModel.CragStatus.Default;
                RaisePropertyChanged(nameof(GuideBookCrags));
                RaisePropertyChanged(nameof(ShowEmptyOverlay)); 
            }
            else if (gbCrag.State == CragInfoModel.CragStatus.Downloading)
            {
                downloadManager.StopDownloadingCrag(gbCrag.CragID);
                RaisePropertyChanged(nameof(GuideBookCrags));
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
            }
            else if (gbCrag.State == CragInfoModel.CragStatus.Default)
            {
                downloadManager.AddCragsToDownload(new[] { gbCrag });
            }

            RaisePropertyChanged(nameof(IsStopDownloadVisible));
        }

        void CragModified(object sender, CragExtended crag)
        {
            if (!crag.is_downloaded || GuideBookCrags == null)
                return;

            var cragInfo = GuideBookCrags.FirstOrDefault(gbcrg => gbcrg.CragID == crag.crag_id);
            if (cragInfo != null)
                cragInfo.State = CragInfoModel.CragStatus.Downloaded;

            RaisePropertyChanged(nameof(GuideBookCrags));
            RaisePropertyChanged(nameof(ShowEmptyOverlay));
            RaisePropertyChanged(nameof(IsStopDownloadVisible));
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            downloadManager.IsDisplayToast(false);
        }

        public override void Destroy()
        {
            base.Destroy();
            downloadManager.IsDisplayToast(true);
            GeneralHelper.CragModified -= CragModified;
        }

        void StopDownloadingAll()
        {
            downloadManager.StopAll();

            RaisePropertyChanged(nameof(IsStopDownloadVisible));
            RaisePropertyChanged(nameof(GuideBookCrags));
            RaisePropertyChanged(nameof(ShowEmptyOverlay));
        }
    }
}
