using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.UserControls.ReportIssue;
using SloperMobile.ViewModel;
using SloperMobile.ViewModel.ReportedIssueViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.ReportedIssuePages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportIssuePage : CarouselPage, INavigationAware
	{
		private readonly IExceptionSynchronizationManager exceptionManager;
		private readonly IRepository<BucketTable> bucketRepository;
		private readonly IRepository<CragImageTable> cragImageRepository;
		private readonly IRepository<TopoTable> topoRepository;
		private readonly IRepository<SectorTable> sectorRepository;
		private readonly IGetImageBytes imageBytesService;
		private NavigationParameters parameters;

		public ReportIssuePage(
			IRepository<BucketTable> bucketRepository,
			IRepository<CragImageTable> cragImageRepository,
			IRepository<TopoTable> topoRepository,
			IRepository<SectorTable> sectorRepository,
			IGetImageBytes imageBytesService,
			IExceptionSynchronizationManager exceptionManager)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
			this.bucketRepository = bucketRepository;
			this.cragImageRepository = cragImageRepository;
			this.topoRepository = topoRepository;
			this.sectorRepository = sectorRepository;
			this.imageBytesService = imageBytesService;
			this.bucketRepository = bucketRepository;
			this.cragImageRepository = cragImageRepository;
			this.topoRepository = topoRepository;
			this.sectorRepository = sectorRepository;
			this.exceptionManager = exceptionManager;
		}

        private void AddCarouselChildPage(string viewtoadd)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var index = Children.IndexOf(CurrentPage);
                if (Children.Count > index + 1)
                {
                    for (int i = Children.Count - 1; i >= index + 1; i--)
                    {
                        Children.RemoveAt(i);
                    }
                }

                if (viewtoadd == "issuetype")
                {

                    ContentPage container = new ContentPage();
                    container.BackgroundColor = Color.Black;
                    IssueTypeToReport objIssue = new IssueTypeToReport();
                    container.Content = objIssue;
                    this.Children.Add(container);
                }
                if (viewtoadd == "issuedetail")
                {
                    ContentPage container = new ContentPage();
                    container.BackgroundColor = Color.Black;
                    IssueDetailToReport objIssueDetail = new IssueDetailToReport();
                    container.Content = objIssueDetail;
                    this.Children.Add(container);
                }

                if (viewtoadd == "snap")
                {
                    ContentPage container = new ContentPage();
                    container.BackgroundColor = Color.Black;
                    IssueSnapToReport objSnap = new IssueSnapToReport();
                    container.Content = objSnap;
                    this.Children.Add(container);
                }

                if (viewtoadd == "bolts")
                {
                    ContentPage container = new ContentPage();
                    container.BackgroundColor = Color.Black;
                    IssueBoltsToReports objBolt = new IssueBoltsToReports();
                    container.Content = objBolt;
                    this.Children.Add(container);
                }
                if (viewtoadd == "comments")
                {
                    ContentPage container = new ContentPage();
                    container.BackgroundColor = Color.Black;
                    IssueCommentToReport objCmts = new IssueCommentToReport();
                    container.Content = objCmts;
                    this.Children.Add(container);
                }
                if (viewtoadd == "summary")
                {
                    ContentPage container = new ContentPage();
                    container.BackgroundColor = Color.Black;
                    IssueToReportSummary objSummary = new IssueToReportSummary(bucketRepository, cragImageRepository, topoRepository, sectorRepository,imageBytesService, exceptionManager);
                    container.Content = objSummary;
					objSummary.BindingContext = this.BindingContext;
                    this.Children.Add(container);
					objSummary.OnNavigatingTo(parameters);
					objSummary.OnNavigatedTo(parameters);
				}


                if (index < Children.Count)
                    SelectedItem = Children[index + 1];
            }
            );
        }
        protected override void OnCurrentPageChanged()
        {
            var index = Children.IndexOf(CurrentPage);
            SelectedItem = Children[index];
            base.OnCurrentPageChanged();
            (BindingContext as BaseViewModel).IsBackButtonVisible = index == 0; 
        }
        protected override bool OnBackButtonPressed()
        {
            var index = Children.IndexOf(CurrentPage);
            if (index > 0)
            {
                SelectedItem = Children[index - 1];
                return true;
            }

            return base.OnBackButtonPressed();
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
			this.parameters = parameters;
			(BindingContext as ReportIssueViewModel).OnConditionAddView = AddCarouselChildPage;
        }
    }
}
