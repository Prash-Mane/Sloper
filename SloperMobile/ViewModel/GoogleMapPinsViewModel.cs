using System.Windows.Input;
using Prism.Navigation;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Extentions;
using Xamarin.Forms;
using SloperMobile.DataBase;
using System;
using Acr.UserDialogs;
using SloperMobile.Common.Constants;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Interfaces;

namespace SloperMobile.ViewModel
{
	public class GoogleMapPinsViewModel : BaseViewModel
    {
		private readonly IRepository<CragExtended> cragRepository;
	    private readonly IUserDialogs userDialogs;

        public GoogleMapPinsViewModel(
			INavigationService navigationService,
			IRepository<CragExtended> cragRepository,
			IUserDialogs userDialogs,
            IExceptionSynchronizationManager exceptionManager) : base(navigationService, exceptionManager)
		{
			PageHeaderText = "Crag Map";
            PageSubHeaderText = "Climbing Locations";
			this.cragRepository = cragRepository;
			this.userDialogs = userDialogs;
            IsMenuVisible = true;
            HasFade = true;
            IsShowFooter = true;
        }

		public ICommand OnGoCommand
		{
			get
			{
				return new Command(() =>
				{
					Settings.IsMapInstructionInit = false;
                    RaisePropertyChanged(nameof(Is_InstructionInit));
				});
			}
		}

		public async void NavigateToCragDetails(int selectedCragId)
		{
			userDialogs.ShowLoading("Loading...");
			var navigationParameters = new NavigationParameters();
            Settings.MapSelectedCrag = selectedCragId;
			navigationParameters.Add(NavigationParametersConstants.SelectedCragIdParameter, selectedCragId);
            navigationParameters.Add(NavigationParametersConstants.NavigatonServiceParameter, navigationService);
			await navigationService.NavigateAsync<CragDetailsViewModel>(navigationParameters);
		}

        public bool Is_InstructionInit
        {
            get => Settings.IsMapInstructionInit;
        }

        bool isLegendVisible;
        public bool IsLegendVisible
        {
            get => isLegendVisible;
            set { 
                SetProperty(ref isLegendVisible, value); 
                RaisePropertyChanged(nameof(BulbImage)); 
            }
        }

        public ICommand LegendCommand { get => new Command(OnLegendClicked); }

        public string BulbImage { get => IsLegendVisible ? "icon_x_close_white.png" : "icon_info.png"; }


        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            //Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
            base.OnNavigatedFrom(parameters);
            App.IsNavigating = false;
		}

		public override async void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
            try
            {
               // IsShowFooter = GeneralHelper.IsCragsDownloaded;

				userDialogs.HideLoading();
			}
			catch(Exception exception)
			{


				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatedTo),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message
				});
			}
		}

        void OnLegendClicked()
        {
            IsLegendVisible = !IsLegendVisible;
        }
	}
}
