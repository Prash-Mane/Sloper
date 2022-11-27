using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.Views.FlyoutPages;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using SloperMobile.Common.Helpers;
using SloperMobile.UserControls.PopupControls;
using Rg.Plugins.Popup.Services;

namespace SloperMobile.ViewModel
{
    public class CheckForUpdatesViewModel : BaseViewModel
    {															
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IRepository<AppSettingTable> appSettingsRepository;
		private readonly IRepository<SectorTable> sectorRepository;
	    private readonly ICheckForUpdateService checkForUpdateService;
		private readonly IUserDialogs userDialogs;
		private readonly IConnectivity connectivity;

		private string displayupdatemessage;
	    private string lastUpdatedText;
	    private bool isContinueDisplay;

		public CheckForUpdatesViewModel(
		    INavigationService navigationService,
		    IRepository<CragExtended> cragRepository,
		    IRepository<AppSettingTable> appSettingsRepository,
		    IRepository<SectorTable> sectorRepository,
		    ICheckForUpdateService checkForUpdateService,
			IExceptionSynchronizationManager exceptionManager,
			IUserDialogs userDialogs,
			IConnectivity connectivity)
		    : base(navigationService, exceptionManager)
	    {												
		    this.cragRepository = cragRepository;
		    this.appSettingsRepository = appSettingsRepository;
		    this.sectorRepository = sectorRepository;
		    this.checkForUpdateService = checkForUpdateService;
			this.userDialogs = userDialogs;
			this.connectivity = connectivity;
	    }

        public ICommand ContinueCommand => new Command(OnContinueExecute);

        public string DisplayUpdateMessage
        {
            get { return displayupdatemessage; }
            set { displayupdatemessage = value; RaisePropertyChanged(); }
        }

        public bool IsContinueDisplay
        {
            get { return isContinueDisplay; }
            set { isContinueDisplay = value; RaisePropertyChanged(); }
        }

		public string LastUpdatedText
        {
            get => lastUpdatedText;
            set => SetProperty(ref lastUpdatedText, value);
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public async override void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
            LastUpdatedText = $"Last Updated: {await GetLastUpdateDate()}"; 

            await CheckAndUpdateData();
		}

		public override void OnNavigatingTo(NavigationParameters parameters)
		{
            base.OnNavigatingTo(parameters);
        }

        private async Task<DateTime> GetLastUpdateDate()
        {
            var lastUpdate = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");

            var format = lastUpdate.Length == 8 ? AppConstant.DateParseString : AppConstant.ExtendedDateParseString;

            DateTime.TryParseExact(lastUpdate, format, null, DateTimeStyles.None, out var date);

            return date;
        }

        private async Task CheckAndUpdateData()
        {
            IsRunningTasks = true;
            IsContinueDisplay = false;
            userDialogs.ShowLoading("Checking for updates.", MaskType.Black);

            if (connectivity.IsConnected)
            {
                try
                {
                    DisplayUpdateMessage = await checkForUpdateService.RunCheckForUpdates();

                    LastUpdatedText = $"Last Updated: {await GetLastUpdateDate()}";
                }
                catch (Exception ex)
                {
                    await exceptionManager.LogException(new ExceptionTable
                    {
                        Method = nameof(this.CheckAndUpdateData),
                        Page = this.GetType().Name,
                        StackTrace = ex.StackTrace,
                        Exception = ex.Message,
                    });
                }
            }
            else
                await PopupNavigation.PushAsync(new NetworkErrorPopup(), true);

            userDialogs.HideLoading();
            IsContinueDisplay = true;
            IsRunningTasks = false;
        }

        private async void OnContinueExecute()
        {
            await ((App)Application.Current).LoadLoggedInPage();
            //if (GeneralHelper.IsCragsDownloaded)
            //{
            //    await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, HomeViewModel>();
            //}
            //else
            //{
            //    await navigationService.ResetNavigation<MainMasterDetailViewModel, MasterNavigationViewModel, CragSectorsViewModel>();
            //}
        }
	}
}
