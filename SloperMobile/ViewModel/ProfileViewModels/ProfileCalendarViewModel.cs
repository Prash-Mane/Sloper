using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Model;
using Syncfusion.SfCalendar.XForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Forms;
using Prism.Navigation;
using SloperMobile.Model.ResponseModels;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Extentions;

namespace SloperMobile.ViewModel.ProfileViewModels
{
    public class ProfileCalendarViewModel : BaseViewModel //IActiveAware
    {
        private readonly IUserDialogs userDialogs;

        bool isLoaded;
        int userId;
        string userName;

        public ProfileCalendarViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper) : base(navigationService, httpHelper)
        {
            this.userDialogs = userDialogs;
        }

        #region Properties
        private CalendarEventCollection _showDates;
        public CalendarEventCollection ShowDates
        {
            get { return _showDates; }
            set { _showDates = value; RaisePropertyChanged(); }
        }

        private CalendarModel _calendarModel;
        public CalendarModel CalendarModel
        {
            get { return _calendarModel; }
            set { _calendarModel = value; RaisePropertyChanged(); }
        }

        private List<DateTime> _selectedDates;
        public List<DateTime> SelectedDates
        {
            get { return _selectedDates; }
            set { _selectedDates = value; RaisePropertyChanged(); }
        }

        private string _headerMonth;

        public event EventHandler IsActiveChanged;

        public string HeaderMonth
        {
            get { return _headerMonth; }
            set { _headerMonth = value; RaisePropertyChanged(); }
        }


        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value, RaiseIsActiveChanged); }
        }
        protected virtual void RaiseIsActiveChanged()
        {
            // NOTE: You must either subscribe to the event or handle the logic here.
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public void GoToPointsDate(DateTime dt)
        {
            var navParams = new NavigationParameters();
            navParams.Add(NavigationParametersConstants.ApplicationActivity, ProfileViews.ProfilePoints);
            navParams.Add(NavigationParametersConstants.PointsDayParameter, dt);
            if (userId != 0)
            {
                navParams.Add(NavigationParametersConstants.MemberProfileId, userId);
                navParams.Add(NavigationParametersConstants.MemberProfileName, userName);
            }
            navigationService.NavigateAsync<ProfileViewModel>(navParams);
        }

        public async void OnPagePrepration(int id)
        {
            userId = id;

            HeaderMonth = DateTime.Today.ToString("MMMM yyyy").ToUpper();
            CalendarModel = new CalendarModel();

            await InvokeServiceGetAscentDates();
        }

        //public async void OnNavigatingTo(NavigationParameters parameters)
        //{
        //    if (isLoaded)
        //        return;

        //    isLoaded = true;

        //    parameters.TryGetValue(NavigationParametersConstants.MemberProfileId, out userId);
        //    if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileName, out userName))
        //        PageHeaderText = userName;
        //    else
        //        PageHeaderText = Settings.DisplayName;


        //    //CalendarBackgroundColor = Color.FromHex("#FF8E2D");
        //    //PointsBackgroundColor = Color.FromHex("#000000");
        //    //TickListBackgroundColor = Color.FromHex("#000000");
        //    //SendsBackgroundColor = Color.FromHex("#000000");
        //    //RankingBackgroundColor = Color.FromHex("#000000");
           
        //}
    


        //protected override void OnNavigation(string param, NavigationParameters parameters = null)
        //{
        //    if (parameters == null)
        //        parameters = new NavigationParameters();

        //    if (userId != 0)
        //    {
        //        parameters.Add(NavigationParametersConstants.MemberProfileId, userId);
        //        parameters.Add(NavigationParametersConstants.MemberProfileName, userName);
        //    }
        //    base.OnNavigation(param, parameters);
        //}

        private async Task InvokeServiceGetAscentDates()
        {
            try
            {
                userDialogs.ShowLoading();

                CalendarModel.app_id = Convert.ToInt32(AppSetting.APP_ID);
                CalendarModel.start_date = "20160101";
                CalendarModel.end_date = "20300101";
                if (userId != 0)
                    CalendarModel.user_id = userId;

                var response = await httpHelper.PostAsync<CalendarResponseModel[]>(ApiUrls.Url_M_GetAscentDates, CalendarModel);

                if (response.ValidateResponse())
                {
                    var collection = new CalendarEventCollection();

                    foreach (var singleDate in response.Result)
                    {
                        collection.Add(new CalendarInlineEvent()
                        {
                            StartTime = DateTime.ParseExact(singleDate.date_climbed, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                            EndTime = DateTime.ParseExact(singleDate.date_climbed, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                            Color = Color.DarkOrange
                        });
                    }

                    ShowDates = collection;
                }

                userDialogs.HideLoading();
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Line = 91,
                    Method = nameof(this.InvokeServiceGetAscentDates),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(CalendarModel)
                });

                Acr.UserDialogs.UserDialogs.Instance.Loading().Hide();
            }
        }
	    //public void OnNavigatedFrom(NavigationParameters parameters)
	    //{
	    //}
	    //public void OnNavigatedTo(NavigationParameters parameters)
     //   {
     //       Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
     //       App.IsNavigating = false;
	    //}
	 //   public void OnNavigatingTo(NavigationParameters parameters)
  //      {
  //          if (isLoaded)
  //              return;
  //          isLoaded = true;
  //          parameters.TryGetValue(NavigationParametersConstants.MemberProfileId, out userId);
  //          if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileName, out userName))
  //              PageHeaderText = userName;
  //          else
  //              PageHeaderText = Settings.DisplayName;
  //          //CalendarBackgroundColor = Color.FromHex("#FF8E2D");
  //          //PointsBackgroundColor = Color.FromHex("#000000");
  //          //TickListBackgroundColor = Color.FromHex("#000000");
  //          //SendsBackgroundColor = Color.FromHex("#000000");
  //          //RankingBackgroundColor = Color.FromHex("#000000");
		//	HeaderMonth = DateTime.Today.ToString("MMMM yyyy").ToUpper();
		//	CalendarModel = new CalendarModel();
		//	OnPagePrepration();
		//}
        //protected override void OnNavigation(string param, NavigationParameters parameters = null)
        //{
        //    if (parameters == null)
        //        parameters = new NavigationParameters();
        //    if (userId != 0)
        //    {
        //        parameters.Add(NavigationParametersConstants.MemberProfileId, userId);
        //        parameters.Add(NavigationParametersConstants.MemberProfileName, userName);
        //    }
        //    base.OnNavigation(param, parameters);
        //}
    }
}
