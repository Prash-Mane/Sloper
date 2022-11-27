using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.ViewModel.SectorViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.ProfileViewModels
{
    //TODO: Inspect and refactor
    public class ProfileTickListViewModel : BaseViewModel
    {														   
        private readonly IRepository<SectorTable> sectorRepository;
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IRepository<GuideBookTable> guideBookRepository;
		private readonly IUserDialogs userDialogs;
        //private ObservableCollection<SendModel> sendsList;
        private ObservableCollection<TickListModel> ticklistsList;

        int userId;

        public ProfileTickListViewModel(
            INavigationService navigationService,
            IRepository<SectorTable> sectorRepository,
			IRepository<CragExtended> cragRepository,
			IRepository<GuideBookTable> guideBookRepository,
			IExceptionSynchronizationManager exceptionManager,
			IUserDialogs userDialogs,
            IHttpHelper httpHelper)
            : base(navigationService, exceptionManager, httpHelper)
        {											   
            this.sectorRepository = sectorRepository;
			this.cragRepository = cragRepository;
			this.guideBookRepository = guideBookRepository;
            this.userDialogs = userDialogs;
        }

        //public ObservableCollection<SendModel> SendsList
        //{
        //    get { return sendsList; }
        //    set { sendsList = value; RaisePropertyChanged(); }
        //}

        public ObservableCollection<TickListModel> TickListsList
        {
            get { return ticklistsList; }
            set { 
                ticklistsList = value; 
                RaisePropertyChanged(); 
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
            }
        }

        public bool ShowEmptyOverlay
        {
            get => (TickListsList == null || TickListsList.Count == 0) && !IsRunningTasks;
        }

        private int onsight;
        public int Onsight
        {
            get { return onsight; }
            set { onsight = value; RaisePropertyChanged(); }
        }

        private int redpoint;
        public int Redpoint
        {
            get { return redpoint; }
            set { redpoint = value; RaisePropertyChanged(); }
        }

        private int projects;
        public int Projects
        {
            get { return projects; }
            set { projects = value; RaisePropertyChanged(); }
        }

        private string tabBackgroundColor;
        public string TabBackgroundColor
        {
            get { return tabBackgroundColor; }
            set { tabBackgroundColor = value; RaisePropertyChanged(); }
        }

        private DateTime date_Created;
        public DateTime Date_Created
        {
            get { return date_Created; }
            set
            {
                date_Created = value;
                DateCreated = value.ToString("MM/dd/yy");
            }
        }

        private string dateCreated;
        public string DateCreated
        {
            get { return dateCreated; }
            set
            {
                dateCreated = value; RaisePropertyChanged();

            }
        }

        private string routeName;
        public string route_name
        {
            get => routeName;
            set
            {
                routeName = value; RaisePropertyChanged();
            }
        }

        private string gradename;
        public string grade_name
        {
            get { return gradename; }
            set { gradename = value; RaisePropertyChanged(); }
        }

        public bool IsMe { get => userId == 0 || userId == Settings.UserID; }

		public ICommand ItemTapCommand => new Command<TickListModel>(OnItemTapped);

		private async void OnItemTapped(TickListModel tickList)
		{
			try
			{
				var sector = await sectorRepository.FindAsync(s => s.is_enabled && s.sector_id == tickList.sector_id);
				var currentCrag = await cragRepository.FindAsync(crag => crag.crag_id == tickList.CragId && crag.is_enabled && crag.is_app_store_ready);
                if (currentCrag == null)
                {
                    await userDialogs.AlertAsync("The Route selected is not available in this app, please select another route.", "ROUTE UNAVAILABLE!", "OK");
                    return;
                }
                if (!await GeneralHelper.HandleDownloadedCrag(currentCrag.crag_id, tickList.route_name, cragRepository, navigationService))
                    return;

                var navigationParameters = new NavigationParameters();
				var mappedSector = new MapListModel
				{
					SectorId = sector.sector_id,
					SectorName = sector.sector_name
				};

				Cache.SelctedCurrentSector = mappedSector;
                Settings.ActiveCrag = tickList.CragId;

				var guideBook = await guideBookRepository.FindAsync(guide => guide.GuideBookId == currentCrag.crag_guide_book);
				//load carousel route detail page when we click on tick list item
				navigationParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, mappedSector);
				navigationParameters.Add(NavigationParametersConstants.RouteIdParameter, tickList.RouteID);
				navigationParameters.Add(NavigationParametersConstants.SingleRouteParameter, true);
				navigationParameters.Add(NavigationParametersConstants.IsNavigatedFromSectorImageParameter, false);
				navigationParameters.Add(NavigationParametersConstants.IsUnlockedParameter, Settings.AppPurchased || guideBook.Unlocked || currentCrag.Unlocked);
				navigationParameters.Add(NavigationParametersConstants.GuideBookIdParameter, guideBook.GuideBookId); 
				await navigationService.NavigateAsync<SectorToposViewModel>(navigationParameters);
			}
			catch (Exception ex)
			{
				userDialogs.HideLoading();
				await userDialogs.AlertAsync("The Route selected is not available in this app, please select another route.", "ROUTE UNAVAILABLE!", "OK");
				await exceptionManager.LogException(new ExceptionTable
				{
                    Method = nameof(this.OnItemTapped),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"tickList = {Newtonsoft.Json.JsonConvert.SerializeObject(tickList)}"
				});
			}
			finally
			{
				userDialogs.HideLoading();
			}
		}

        public async void OnPagePrepration(int id)
        {
            try
            {
                userId = id;
                userDialogs.ShowLoading();
                if (!GuestHelper.IsGuest)
                    await InvokeServiceGetTickListData();
                userDialogs.HideLoading();
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

        private void RebuildTickList()
        {
            //MessagingCenter.Subscribe<ProfileTickListPage>(this, "TickListsRebind", async (sender) =>
            //{
            //    await InvokeServiceGetTickListData();
            //});
        }

        private async Task InvokeServiceGetTickListData()
        {
            try
            {
                var response = await httpHelper.GetAsync<IEnumerable<TickListModel>>(ApiUrls.Url_TickList_GetTickList(userId));

                if(response.ValidateResponse())
                {
                    if (response.Result.Any())
                    {
                        TickListsList = new ObservableCollection<TickListModel>(response.Result);
                        if (TickListsList.Any())
                        {
                            foreach (var item in TickListsList)
                            {
                                route_name = item.route_name;
                                grade_name = item.grade_name;
                                DateCreated = item.Date_Created.ToString("MM/dd/yy");
                            }
                        }
                    }
                    else
                        TickListsList = new ObservableCollection<TickListModel>();
                    
                }
            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.InvokeServiceGetTickListData),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
        }

        //public void OnNavigatedFrom(NavigationParameters parameters)
        //{
        //}

        //public void OnNavigatedTo(NavigationParameters parameters)
        //{
        //    //Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
        //    App.IsNavigating = false;
        //}

        public async Task DeleteTick(TickListModel tick)
        {
            var response = await httpHelper.GetAsync<bool>(ApiUrls.Url_TickList_DeleteTickList(tick.RouteID));

            if (response.ValidateResponse() && response.Result)
            {
                UserDialogs.Instance.Toast(new ToastConfig("")
                {
                    Message = " Tick deleted successfully!",
                    BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                    MessageTextColor = System.Drawing.Color.White,
                    Duration = TimeSpan.FromSeconds(3)
                });

                TickListsList.Remove(tick);
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

        //    OnPagePrepration();
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
