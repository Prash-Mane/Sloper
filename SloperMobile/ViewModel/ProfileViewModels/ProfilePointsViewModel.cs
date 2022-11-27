using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.PointModels;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.ProfileViewModels
{
	public class ProfilePointsViewModel : BaseViewModel
    {
        private IList<PointDailyModel> dailyPointsList;
        public int maxpoint;
        public bool Flag = true;
        bool isLoaded;
        int userId;
        string userName;
        private IList<DailyPointChartModel> fullDailyList;
        private readonly IUserDialogs userDialogs;

        public ProfilePointsViewModel(
            INavigationService navigationService,
            IUserDialogs userDialogs,
			IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {
            this.userDialogs = userDialogs;
            DailyPointsList = new List<DailyPointChartModel>();
            dailyPointsList = new List<PointDailyModel>();
            PointDailyCommand = new Command<PointModel>(OnPointDailyClicked);

            GaugeThickness = ((Application.Current.MainPage.Width) / (40));
            CenterGaugeMargin = 0;

            RimThickness = (Device.RuntimePlatform == Device.iOS) ? GaugeThickness * 2 : GaugeThickness;

            //GaugeFontSize = ((Application.Current.MainPage.Width) / (20));
            //GaugeFontSizeSub = GaugeFontSize * .7;
            Offset = Common.Enumerators.Offsets.Header;
        }

        private int centergaugemargin;
        public int CenterGaugeMargin
        {
            get { return centergaugemargin; }
            set { centergaugemargin = value; RaisePropertyChanged(); }
        }

        private double rimThickness;
        public double RimThickness
        {
            get { return rimThickness; }
            set { rimThickness = value; RaisePropertyChanged(); }
        }

        private double gaugethickness;
        public double GaugeThickness
        {
            get { return gaugethickness; }
            set { gaugethickness = value; RaisePropertyChanged(); }
        }

        //private double gaugefontsize;
        //public double GaugeFontSize
        //{
        //    get { return gaugefontsize; }
        //    set { gaugefontsize = value; RaisePropertyChanged(); }
        //}

        //private double gaugefontsizesub;
        //public double GaugeFontSizeSub
        //{
        //    get { return gaugefontsizesub; }
        //    set { gaugefontsizesub = value; RaisePropertyChanged(); }
        //}

        #region Properties
        private string pointsDate;
        public string PointsDate
        {
            get { return pointsDate; }
            set { pointsDate = value; RaisePropertyChanged(); }
        }

        private DateTime dateForgraph;
        public DateTime DateForgraph
        {
            get { return dateForgraph; }
            set { dateForgraph = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ObservableGroupCollection<string, PointModel>> pointsList;
        public ObservableCollection<ObservableGroupCollection<string, PointModel>> PointsList
        {
            get { return pointsList; }
            set { 
                pointsList = value; 
                RaisePropertyChanged(); 
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
            }
        }

        private int _personalBest;
        public int PersonalBest
        {
            get { return _personalBest; }
            set { _personalBest = value; RaisePropertyChanged(); }
        }

        DateTime climbDate = DateTime.Now;
        public DateTime ClimbDate {
            get => climbDate;
            set => SetProperty(ref climbDate, value);
        }

        private string _totalPoints;
        public string TotalPoints
        {
            get { return _totalPoints; }
            set { _totalPoints = value; RaisePropertyChanged(); }
        }

        private string _totalRoutes;
        public string TotalRoutes
        {
            get { return _totalRoutes; }
            set { _totalRoutes = value; RaisePropertyChanged(); }
        }

        //private IList<DailyPointChartModel> _dailyPointsList;
        //public IList<DailyPointChartModel> DailyPointsList
        //{
        //    get { return _dailyPointsList; }
        //    set { _dailyPointsList = value; RaisePropertyChanged("DailyPointsList"); }
        //}
        private IList<DailyPointChartModel> _dailyPointsList;
        public IList<DailyPointChartModel> DailyPointsList
        {
            get { return _dailyPointsList; }
            set { SetProperty(ref _dailyPointsList, value); }
        }

        private int _selectedPointIndex;
        public int SelectedPointIndex
        {
            get
            {
                return Device.RuntimePlatform == Device.iOS ? (DailyPointsList?.Count - 1).Value : 0;
            }
            set
            {
                if (_selectedPointIndex != value && value != -1)
                {
                    _selectedPointIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PointModel selectedItem;
        public PointModel SelectedItem
        {
            get { return selectedItem; }
            set { SetProperty(ref selectedItem, value); }
        }

        public bool ShowEmptyOverlay
        {
            get => (PointsList == null || PointsList.Count == 0) && !IsRunningTasks && isLoaded;
        }
        #endregion

        #region Delegate Command
        public Command<PointModel> PointDailyCommand { get; set; }
        public ICommand ItemTappedCommand
        {
            get
            {
                return new Command<PointModel>(OnPointDailyClicked);
            }
        }
		public ICommand DateHeaderTappedCommand
		{
			get
			{
				return new Command(OnHeaderClick);
			}
		}
		private async void OnHeaderClick(object obj)
		{
			IList iList = (IList)obj;
			var selectedPoint = iList.Cast<PointModel>();
			OnPointDailyClicked(selectedPoint.Select(s => s).FirstOrDefault());
		}
		#endregion

        public async void OnPagePrepration(int id, DateTime dateForgraph)
        {
            try
            {
                userDialogs.ShowLoading();

                userId = id;
                DateForgraph = dateForgraph;
                if (dateForgraph != default(DateTime))
                    PointsDate = dateForgraph.ToString("yyyyMMdd");

                if (!GuestHelper.IsGuest)
                {
                    await GetDailyPointsData();
                    await InvokeServiceGetPointsData();
                }
                else
                {
                    isLoaded = true;
                    RaisePropertyChanged(nameof(ShowEmptyOverlay));
                }
                InvokeServiceGetPointsDailyData();
                this.PointDailyCommand.Execute(null);
                if (DateForgraph != default(DateTime))
                    BindGraphicsOnCalendarSelectedDate(DateForgraph);
               
	            userDialogs.HideLoading();

                PointDailyCommand.Execute(null);
            }
            catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnPagePrepration),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
        }

        private async Task InvokeServiceGetPointsData()
        {
            var response = await httpHelper.GetAsync<IEnumerable<PointModel>>(ApiUrls.Url_M_GetPoints(PointsDate, userId));

            isLoaded = true;

            if(response.ValidateResponse() && response.Result.Any())
            {
                var sorted = from point in response.Result
                             orderby point.date_climbed descending
                             group point by (point.date_climbed) into pointGroup
                             select new ObservableGroupCollection<string, PointModel>(pointGroup.Key.ToString("MMM dd, yyyy"), pointGroup);

                var reslist = sorted.ToList();

                for (int i = 0; i < reslist.Count; i++)
                {
                    var obj = reslist[i];
                    var newpoint = new PointModel();
                    var pts = obj.Sum(t => Convert.ToInt32(t.points));
                    newpoint.date_climbed = obj[0].date_climbed;
                    newpoint.route_name = "";
                    newpoint.tech_grade = "Total";
                    newpoint.points = pts;
                    obj.Add(newpoint);
                    reslist[i] = obj;
                }
                PointsList = new ObservableCollection<ObservableGroupCollection<string, PointModel>>(reslist);
            }
            else
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
        }

        private async void InvokeServiceGetPointsDailyData()
        {
            try
            {
                DailyPointsList = dailyPointsList
                    .Select(singlePoint =>
                    new DailyPointChartModel(singlePoint.ascent_count,
                        (singlePoint.points.HasValue ? singlePoint.points.Value : 0 / (double)maxpoint) * 100,
                        singlePoint.date_climbed.ToString()))
                    .ToList();

                fullDailyList = DailyPointsList;
                RaisePropertyChanged("SelectedPointIndex");
            }
            catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.InvokeServiceGetPointsDailyData),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
        }

        private async void OnPointDailyClicked(PointModel obj)
        {
            try
            {
                if (obj != null)
                {
                    var selectedPoint = obj as PointModel;
                    if (!selectedPoint.date_climbed.Equals(ClimbDate))
                    {
                        DailyPointsList = fullDailyList.Where(item => Convert.ToDateTime(item.ClimbDate) <= selectedPoint.date_climbed).ToList();
                        RaisePropertyChanged("SelectedPointIndex");
                        FillGraphs(selectedPoint.date_climbed);
                    }
                }
                else
                {
                    if(dailyPointsList.Any())
                        FillGraphs(dailyPointsList.LastOrDefault().date_climbed);
                }

                SelectedItem = null;
            }
            catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnPointDailyClicked),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
				});
			}
        }

        private async void FillGraphs(DateTime selectedClimbDate)
        {
            try
            {
                foreach (var findpoint in dailyPointsList)
                {
                    if (findpoint.date_climbed == selectedClimbDate)
                    {  						
						PersonalBest = Convert.ToInt32((Convert.ToInt32(findpoint.points) / (double)maxpoint) * 100);
                        TotalPoints = findpoint.points.ToString();
                        TotalRoutes = findpoint.ascent_count;
                        ClimbDate = findpoint.date_climbed;
                    }
                }

            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.FillGraphs),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(selectedClimbDate)
				});
			}
        }

        private async Task GetDailyPointsData()
        {
            try
            {
                if (Flag)
                {
                    Flag = false;

                    var response = await httpHelper.GetAsync<IEnumerable<PointDailyModel>>(ApiUrls.Url_M_GetPointsDaily(userId));

                    if (response.ValidateResponse() && response.Result.Any())
                    {
                        foreach (var item in response.Result)
                        {
                            dailyPointsList.Add(item);
                        }

                        maxpoint = dailyPointsList.Max(t => t.points.HasValue ? t.points.Value : 0);
                        dailyPointsList = new ObservableCollection<PointDailyModel>(dailyPointsList.Reverse());
                    }
                }
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.GetDailyPointsData),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
        }

        private async void BindGraphicsOnCalendarSelectedDate(DateTime pointsDate)
        {
            try
            {
                DailyPointsList = fullDailyList.Where(item => Convert.ToDateTime(item.ClimbDate) <= pointsDate).ToList();
                RaisePropertyChanged("SelectedPointIndex");
                FillGraphs(pointsDate);
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.BindGraphicsOnCalendarSelectedDate),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(pointsDate)
				});
			}
        }

        //public void OnNavigatingTo(NavigationParameters parameters)
        //{
        //    if (isLoaded)
        //        return;
            
        //    parameters.TryGetValue(NavigationParametersConstants.MemberProfileId, out userId);
        //    if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileName, out userName))
        //        PageHeaderText = userName;
        //    else
        //        PageHeaderText = Settings.DisplayName;

        //    if (parameters.TryGetValue(NavigationParametersConstants.PointsDayParameter, out DateTime dateForgraph))
        //    {
        //        DateForgraph = dateForgraph;
        //        PointsDate = dateForgraph.ToString("yyyyMMdd");
        //    }
        //    else
        //        DateForgraph = DateTime.Now;

        //    OnPagePrepration();
        //    PointDailyCommand.Execute(null);
        //}

	 //   public void OnNavigatedFrom(NavigationParameters parameters)
		//{
		//}

	 //   public void OnNavigatedTo(NavigationParameters parameters)
  //      {
  //          Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
  //          App.IsNavigating = false;
		//}

        //protected override void OnNavigation(string param, NavigationParameters parameters = null)
        //{
        //    if (parameters == null)
        //        parameters = new NavigationParameters();

        //    if (userId != 0) {
        //        parameters.Add(NavigationParametersConstants.MemberProfileId, userId);
        //        parameters.Add(NavigationParametersConstants.MemberProfileName, userName);
        //    }
        //    base.OnNavigation(param, parameters);
        //}
    }
}
