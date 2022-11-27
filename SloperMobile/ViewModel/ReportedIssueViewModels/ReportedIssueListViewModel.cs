using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using Xamarin.Forms;
using System.Linq;

namespace SloperMobile.ViewModel.ReportedIssueViewModels
{
    public class ReportedIssueListViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private readonly IRepository<IssueTable> issueRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IUserDialogs userDialogs;
        private IList<ReportedIssueModel> reportedIssueList;

		public ReportedIssueListViewModel(
            INavigationService navigationService,
            IRepository<IssueTable> issueRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<CragExtended> cragRepository,
            IUserDialogs userDialogs
            ) : base(navigationService)
        {
            this.navigationService = navigationService;
            this.issueRepository = issueRepository;
            this.routeRepository = routeRepository;
            this.cragRepository = cragRepository;
            this.userDialogs = userDialogs;

            IsBackButtonVisible = true;
        }

        public IList<ReportedIssueModel> ReportedIssueList
        {
            get { return reportedIssueList; }
            set { reportedIssueList = value; RaisePropertyChanged(); }
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

		public async override void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
            if (parameters.Count == 0)
			{
				return;
			}

			var currentRouteId = (int)parameters[NavigationParametersConstants.CurrentRouteIdParameter];
			var currentSector = (MapListModel)parameters[NavigationParametersConstants.CurrentSectorParameter];
			var currentCrag = await cragRepository.FindAsync(c => c.crag_id == Settings.ActiveCrag);
			var routeData = await routeRepository.FindAsync(r => r.route_id == currentRouteId && r.is_enabled);

			await BindReportedIssues(currentRouteId);
			
            PageHeaderText = $"{(routeData.route_name).ToUpper()} {routeData.tech_grade}";
            PageSubHeaderText = $"{Cache.SelctedCurrentSector.SectorName}, {(currentCrag.crag_name).Trim()}";

            userDialogs.HideLoading();
		}

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
        }

        private async Task BindReportedIssues(int routeid)
        {
            //ReportedIssueList = await issueRepository.QueryAsync<ReportedIssueModel>($@"SELECT TISSUE.reported_by AS ReportedBy, TISSUE.date_reported AS DateCreated, (TISSUE_CATEGORY.issue_category_name ||' ' || TISSUE_TYPE.issue_type_name) AS IssueName,TISSUE.route_id AS RouteId, TISSUE.issue_id as IssueId FROM TISSUE JOIN TISSUE_CATEGORY ON
            //                    TISSUE.issue_category_id=TISSUE_CATEGORY.issue_category_id JOIN TISSUE_TYPE ON  
            //                    TISSUE.issue_type_id=TISSUE_TYPE.issue_type_id Where TISSUE.route_id={routeid}");
	        try
	        {
		        var result = await issueRepository.QueryAsync<ReportedIssueModel>(
			        $@"SELECT TISSUE.reported_by AS ReportedBy, TISSUE.date_reported AS DateCreated,TISSUE_CATEGORY.issue_category_name AS IssueCategory ,TISSUE_TYPE.issue_type_name AS IssueType ,TISSUE.route_id AS RouteId, TISSUE.issue_id as IssueId FROM TISSUE JOIN TISSUE_CATEGORY ON
                                TISSUE.issue_category_id=TISSUE_CATEGORY.issue_category_id JOIN TISSUE_TYPE ON  
                                TISSUE.issue_type_id=TISSUE_TYPE.issue_type_id Where TISSUE.route_id={routeid}");
                ReportedIssueList = result.OrderByDescending(x => x.IssueId).ToList();
	        }
	        catch (Exception e)
	        {

	        }
        }

        public ICommand IssueTapCommand
        {
            get
            {
                return new Command<ReportedIssueModel>(async issueList =>
                {
                    var navigationParameters = new NavigationParameters();
					navigationParameters.Add(NavigationParametersConstants.CurrentRouteIdParameter, issueList.RouteId);
					navigationParameters.Add("CurrentIssueID", issueList.IssueId);
					await navigationService.NavigateAsync<ReportedIssueSummaryViewModel>(navigationParameters);
                });
            }
        }
    }
}
