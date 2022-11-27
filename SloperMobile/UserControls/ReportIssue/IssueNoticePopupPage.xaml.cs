using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SloperMobile.Common.Extentions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.Navigation;
using SloperMobile.ViewModel;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.ViewModel.ReportedIssueViewModels;

namespace SloperMobile.UserControls.ReportIssue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IssueNoticePopupPage : PopupPage
    {
        private INavigationService navigationService;
        private IUserDialogs userDialogs;
        private int? curr_routeid;
        public int? CurrentRouteID
        {
            get { return curr_routeid; }
            set
            {
                curr_routeid = value;
                OnPropertyChanged();
            }
        }
        public IssueNoticePopupPage(INavigationService navigationService,IUserDialogs userDialogs, int? routeid,string routenotice)
        {
            InitializeComponent();
            lbl_notice.Text = routenotice;
            this.navigationService = navigationService;
            this.userDialogs = userDialogs;
            CurrentRouteID = routeid;
        }

        private async void btn_viewdetails_Clicked(object sender, EventArgs e)
        {
            userDialogs.ShowLoading();
            var navigationParameter = new NavigationParameters();
            navigationParameter.Add(NavigationParametersConstants.CurrentRouteIdParameter, CurrentRouteID);
            if (PopupNavigation.PopupStack.Count > 0)
                await PopupNavigation.PopAllAsync();
            await navigationService.NavigateAsync<ReportedIssueListViewModel>(navigationParameter);
        }
        private async void OnClose(object sender, EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
                await PopupNavigation.PopAllAsync();
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return Content.FadeTo(1);
        }

        protected override Task OnDisappearingAnimationBeginAsync()
        {
            return Content.FadeTo(1);
        }
    }
}
