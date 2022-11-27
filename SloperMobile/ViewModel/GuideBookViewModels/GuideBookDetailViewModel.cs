using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SloperMobile.Common.Extentions;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.NotifyProperties;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.CragModels;
using SloperMobile.Model.GuideBookModels;
using SloperMobile.ViewModel.SubscriptionViewModels;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using SloperMobile.UserControls;
using Microsoft.AppCenter.Analytics;

namespace SloperMobile.ViewModel.GuideBookViewModels
{
    public class GuideBookDetailViewModel : BaseViewModel
    {
        private readonly IRepository<AppProductTable> appProductRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IDownloadCragService downloadCragService;
        private readonly IUserDialogs userDialogs;
        private readonly IDownloadManager downloadManager;
        private readonly IRemoveCragManager removeManager;
        bool isLoaded;

        public GuideBookDetailViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IRepository<AppProductTable> appProductRepository,
            IRepository<GuideBookTable> guideBookRepository,
            IRepository<CragExtended> cragRepository,
            IDownloadCragService downloadCragService,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper,
            IDownloadManager downloadManager,
            IRemoveCragManager removeManager
            )
            : base(navigationService, exceptionManager, httpHelper)
        {
            this.guideBookRepository = guideBookRepository;
            this.cragRepository = cragRepository;
            this.appProductRepository = appProductRepository;
            this.downloadCragService = downloadCragService;
            this.userDialogs = userDialogs;
            this.downloadManager = downloadManager;
            this.removeManager = removeManager;

            DownloadCommand = new Command<CragInfoModel>(ExecuteOnCragDownload);
            DownloadAllCommand = new Command(ExecuteOnDownloadAll);
            RemoveAllCommand = new Command(ExecuteOnRemoveAll);

            GeneralHelper.CragModified += CragModified;
            GeneralHelper.AppOrGuidebookPurchased += AppOrGuidebookPurchased;

            PageHeaderText = "GUIDEBOOKS";
            PageSubHeaderText = "";
            IsBackButtonVisible = true;
            Offset = Common.Enumerators.Offsets.None;
            HasFade = true;
            GradientHeaderHeight = 100;
            HeaderColors = new List<GradientColor> {
                new GradientColor { Color = Color.Black, Position = 0.3f },
                new GradientColor { Color = Color.Transparent, Position = 0.9f }
            };
        }
        private GuideBook _curr_GB;
        public GuideBook CurrentGuideBook
        {
            get { return _curr_GB ?? (_curr_GB = new GuideBook()); }
            set
            {
                SetProperty(ref _curr_GB, value);
                RaisePropertyChanged("CurrentGuideBook");
            }
        }

        private List<CragInfoModel> _gb_Crags = new List<CragInfoModel>();
        public List<CragInfoModel> GuideBookCrags
        {
            get => _gb_Crags;
            set
            {
                SetProperty(ref _gb_Crags, value);
                RaisePropertyChanged("GuideBookCrags");
            }
        }

        public bool IsVisibleDownloadAll => !GuideBookCrags.All(c => c.State == CragInfoModel.CragStatus.Downloaded);
        public bool IsVisibleRemoveAll => GuideBookCrags.Any(c => c.State == CragInfoModel.CragStatus.Downloaded);
        public bool IsStopDownloadVisible => GuideBookCrags.Any(c => c.State == CragInfoModel.CragStatus.Downloading || c.State == CragInfoModel.CragStatus.DownloadQueued);

        private CragInfoModel _selectedgbcrag;
        public CragInfoModel SelectedGBCrag
        {
            get { return _selectedgbcrag; }
            set
            {
                if(SetProperty(ref _selectedgbcrag, value))
                {
                    NavigateToCragDetails(_selectedgbcrag);
                }
            }
        }

        public string DownloadAllIcon => GuideBookCrags.All(c => c.State == CragInfoModel.CragStatus.Downloading || c.State == CragInfoModel.CragStatus.DownloadQueued || c.State == CragInfoModel.CragStatus.Downloaded) ? "icon_guidebook_queue" : "icon_guidebook_download";
   


        public int DeviceWidth { get => App.DeviceScreenWidth; }
        #region Commands
        public Command DownloadCommand { get; set; }
        public Command DownloadAllCommand { get; set; }
        public Command RemoveAllCommand { get; set; }
        public Command StopDownloads => new Command(StopDownloadingAll);

        #endregion
        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;

            isLoaded = true;
            try
            {
                var currGuideBook = (GuideBook)parameters["CurrentGuideBook"];
                CurrentGuideBook = currGuideBook;

                var dict = new Dictionary<string, string>() { { "GuideBook", $"id: {CurrentGuideBook.GuideBookId}, name: {CurrentGuideBook.GuideBookName}" } };
                Analytics.TrackEvent(GetType().Name.TruncateVMName(), dict);

                var tempList = new List<CragInfoModel>();
                var crags = await cragRepository.GetAsync(c => c.is_enabled && c.is_app_store_ready && c.crag_guide_book == currGuideBook.GuideBookId);
                crags = crags.OrderBy(c => c.crag_sort_order).ToList();
                var freeCrags = Settings.FreeCragIds;

                foreach (CragExtended ce in crags)
                {
                    CragInfoModel cragInfo = new CragInfoModel();
                    cragInfo.CragID = ce.crag_id;
                    cragInfo.CragName = ce.crag_name;
                    cragInfo.Unlocked = ce.Unlocked;
                    cragInfo.IsFree = freeCrags.Contains(ce.crag_id);
                    cragInfo.State = ce.is_downloaded ? CragInfoModel.CragStatus.Downloaded : CragInfoModel.CragStatus.Default;
                    tempList.Add(cragInfo);
                }
                if (downloadManager.DownloadingQueue.Any()) //to have elements with dynamically changed progress from manager
                {
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        var ci = tempList[i];
                        var match = downloadManager.DownloadingQueue.FirstOrDefault(c => c.CragID == ci.CragID);
                        if (match != null)
                            tempList[i] = match;
                    }
                }

                if (removeManager.RemovingQueue.Any())
                {
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        var ci = tempList[i];
                        var match = removeManager.RemovingQueue.FirstOrDefault(c => c.CragID == ci.CragID);
                        if (match != null)
                            tempList[i] = match;
                    }
                }

                GuideBookCrags = tempList;

                RaisePropertyChanged(nameof(IsVisibleRemoveAll));
                RaisePropertyChanged(nameof(IsVisibleDownloadAll));
                RaisePropertyChanged(nameof(IsStopDownloadVisible));
                RaisePropertyChanged(nameof(DownloadAllIcon));
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

        public async void ExecuteOnCragDownload(CragInfoModel gbCrag)
        {
            if (gbCrag.State == CragInfoModel.CragStatus.Default)
            {
                if (!gbCrag.Unlocked)
                {
                    NavigateToCragDetails(gbCrag);
                    return;
                }

                downloadManager.AddCragsToDownload(new[] { gbCrag });
            }
            else if (gbCrag.State == CragInfoModel.CragStatus.DownloadQueued)
            {
                GuideBookCrags.Find(gbcrg => gbcrg.CragID == gbCrag.CragID).State = CragInfoModel.CragStatus.Default;
            }
            //else if (gbCrag.State == CragInfoModel.CragStatus.RemoveQueued)
            //{
            //    GuideBookCrags.Find(gbcrg => gbcrg.CragID == gbCrag.CragID).State = CragInfoModel.CragStatus.Downloaded;
            //}

            else if (gbCrag.State == CragInfoModel.CragStatus.Downloaded)
            {	 				
				if (!GuideBookCrags.Any(c => c.State == CragInfoModel.CragStatus.Removing || c.State == CragInfoModel.CragStatus.RemoveQueued
					                    || c.State == CragInfoModel.CragStatus.DownloadQueued))
				{
					if (await userDialogs.ConfirmAsync("Are you sure want to remove this crag?", "Confirm", "Ok", "Cancel"))
					{
						removeManager.AddCragsToRemove(new[] { gbCrag });
					}
				}
            }
			else if (gbCrag.State == CragInfoModel.CragStatus.Downloading)
				downloadManager.StopDownloadingCrag(gbCrag.CragID);
            //else if (gbCrag.State == CragInfoModel.CragStatus.Removing)
            //{
            //    removeManager.StopRemovingCrag(gbCrag.CragID);
            //}	 	
            RaisePropertyChanged(nameof(IsStopDownloadVisible));
            RaisePropertyChanged(nameof(DownloadAllIcon));
        }

        public async void ExecuteOnDownloadAll()
        {
            if (!CurrentGuideBook.Unlocked)
            {
                var guideBook = await guideBookRepository.FindAsync(guide => guide.GuideBookId == CurrentGuideBook.GuideBookId);
                navigationService.NavigateAsync<PremiumSubscriptionViewModel>();
            }
            else
            {
                var notDownloaded = GuideBookCrags.Where(c => c.State == CragInfoModel.CragStatus.Default);
                downloadManager.AddCragsToDownload(notDownloaded);
                RaisePropertyChanged(nameof(IsStopDownloadVisible));
                RaisePropertyChanged(nameof(DownloadAllIcon));
            }
        }

        public async void ExecuteOnRemoveAll()
        {
            StopDownloadingAll();
			
			if (await userDialogs.ConfirmAsync("Are you sure want to remove all crags from this guidebook?", "Confirm", "Ok", "Cancel"))
			{
				var downloadedCrags = GuideBookCrags.Where(c => c.State == CragInfoModel.CragStatus.Downloaded);
				removeManager.AddCragsToRemove(downloadedCrags);
			}

            RaisePropertyChanged(nameof(IsStopDownloadVisible));
            RaisePropertyChanged(nameof(DownloadAllIcon));
        }

        void StopDownloadingAll()
        {
            var pendingCrags = downloadManager.DownloadingQueue.Where(c => GuideBookCrags.Any(gc => gc.CragID == c.CragID));
            foreach (var crag in pendingCrags)
                downloadManager.StopDownloadingCrag(crag.CragID);

            RaisePropertyChanged(nameof(IsStopDownloadVisible));
            RaisePropertyChanged(nameof(DownloadAllIcon));
        }

        public async void NavigateToCragDetails(CragInfoModel GBCrag)
        {
            if (GBCrag == null || IsRunningTasks)
            {
                return;
            }

            IsRunningTasks = true;
            userDialogs.ShowLoading("Loading...");
            var navigationParameters = new NavigationParameters();
            Settings.MapSelectedCrag = GBCrag.CragID;
            navigationParameters.Add(NavigationParametersConstants.SelectedCragIdParameter, GBCrag.CragID);
            navigationParameters.Add(NavigationParametersConstants.NavigatonServiceParameter, navigationService);
            await navigationService.NavigateAsync<CragDetailsViewModel>(navigationParameters);
            IsRunningTasks = false;
        }

        #region Services 

        void CragModified(object sender, CragExtended crag)
        {
            var cragInfo = GuideBookCrags.FirstOrDefault(ci => ci.CragID == crag.crag_id);
            if (cragInfo == null)
                return;

            var freeCrags = Settings.FreeCragIds;
            cragInfo.State = crag.is_downloaded ? CragInfoModel.CragStatus.Downloaded : CragInfoModel.CragStatus.Default;
            cragInfo.Unlocked = crag.Unlocked;
            cragInfo.IsFree = freeCrags.Contains(crag.crag_id);

            RaisePropertyChanged(nameof(IsVisibleRemoveAll));
            RaisePropertyChanged(nameof(IsVisibleDownloadAll));
            RaisePropertyChanged(nameof(IsStopDownloadVisible));
            RaisePropertyChanged(nameof(DownloadAllIcon));
        }

        void AppOrGuidebookPurchased(object sender, AppProductTable e)
        {
            if (e.AppProductTypeId == (int)ProductTypes.App
                || (e.AppProductTypeId == (int)ProductTypes.Guidebook) && e.SloperId == CurrentGuideBook.GuideBookId)
            {
                CurrentGuideBook.Unlocked = true;
                foreach (var cragInfo in GuideBookCrags)
                    cragInfo.Unlocked = true;
            }
        }


        public override void Destroy()
        {
            base.Destroy();
            GeneralHelper.CragModified -= CragModified;
            GeneralHelper.AppOrGuidebookPurchased -= AppOrGuidebookPurchased;
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            downloadManager.IsDisplayToast(true);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            downloadManager.IsDisplayToast(false);
        }
        #endregion
    }
}
