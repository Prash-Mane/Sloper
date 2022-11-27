using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SloperMobile.ViewModel.ProfileViewModels;
using Xamarin.Forms.Xaml;

namespace SloperMobile.UserControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RankingFiltersUC : PopupPage
    {
        private ProfileRankingViewModel _profileRankingViewModel;
        public RankingFiltersUC(ProfileRankingViewModel profileRankingViewModel)
        {
            InitializeComponent();
            _profileRankingViewModel = profileRankingViewModel;
            BindingContext = _profileRankingViewModel;
        }
        private async void OnClose(object sender, System.EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
                await PopupNavigation.PopAllAsync();
        }
    }
}