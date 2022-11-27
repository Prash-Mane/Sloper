using System;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using Xamarin.Forms;
using SloperMobile.Common.Extentions;
using SloperMobile.ViewModel.GuideBookViewModels;
using Rg.Plugins.Popup.Services;

namespace SloperMobile.ViewModel
{
    public class HomeViewModel : BaseViewModel
	{
		public HomeViewModel(
			INavigationService navigationService,
			IUserDialogs userDialogs,
			IExceptionSynchronizationManager exceptionManager) : base(navigationService, exceptionManager)
		{
			IsMenuVisible = true;
            IsShowFooter = false;

			ClimbingYear = DateTime.Now.Year.ToString();
			ClimbDaysCount = Settings.ClimbingDays.ToString();
			ClimbingYearFontSize = ((Application.Current.MainPage.Height) / (50)).ToString();
			ClimbingDaysFontSize = ((Application.Current.MainPage.Height) / (30)).ToString();
		}

        private string climbyearfontsize;
		public string ClimbingYearFontSize
		{
			get { return climbyearfontsize; }
			set { climbyearfontsize = value; RaisePropertyChanged(); }
		}

		private string climbdaysfontsize;
		public string ClimbingDaysFontSize
		{
			get { return climbdaysfontsize; }
			set { climbdaysfontsize = value; RaisePropertyChanged(); }
		}

		private string climbyear;
		public string ClimbingYear
		{
			get { return climbyear; }
			set { climbyear = value; RaisePropertyChanged(); }
		}
		private string climbcount;
		public string ClimbDaysCount
		{
			get { return climbcount; }
			set { climbcount = value; RaisePropertyChanged(); }
		}

		public ICommand IconPressedCommand
		{
			get
			{
				return new Command<string>(OnClickSend);
			}
		}

        public ICommand SearchCommand => new Command(OnSearchClick);

        private async void OnSearchClick(object obj)
        {
            await navigationService.NavigateAsync<SearchViewModel>();
        }

        public Command CalendarCommand => new Command(OnCalendarCommand);

        private async void OnClickSend(string param)
		{
			try
			{
				if (string.IsNullOrEmpty(param))
					return;

				var pageType = (ApplicationActivity)Enum.Parse(typeof(ApplicationActivity), param);
				await PageNavigation(pageType);
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnClickSend),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(param)
				});
			}
		}

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            Settings.MapSelectedCrag = 0;
            App.IsNavigating = false;

            Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
        }

        async void OnCalendarCommand() 
        {
            var navParams = new NavigationParameters();
            navParams.Add(NavigationParametersConstants.ApplicationActivity, ProfileViews.ProfileCalendar);
            await navigationService.NavigateAsync<ProfileViewModel>(navParams);
        }
    }
}
