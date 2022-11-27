using Prism.Navigation;

namespace SloperMobile.ViewModel.ReportedIssueViewModels
{
	public class ReportedIssueImageViewModel : BaseViewModel
	{
		public ReportedIssueImageViewModel(INavigationService navigationService) : base(navigationService)
        {
            IsBackButtonVisible = true;
		}

		public override void OnNavigatingTo(NavigationParameters parameters)
		{
            base.OnNavigatingTo(parameters);
            PageSubHeaderText = (string) parameters["PageSubHeaderText"];
			PageHeaderText = (string) parameters["PageHeaderText"];
		}
	}
}