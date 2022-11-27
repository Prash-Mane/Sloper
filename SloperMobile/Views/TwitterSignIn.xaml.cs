using System;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TwitterSignIn : ContentPage, INavigationAware
    {
        private INavigationService navigationService;
        public bool gotomodel;

        public TwitterSignIn(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            InitializeComponent();           
        }

		public async void GotoModel()
        {
            if (navigationService == null)
                navigationService = App.Navigation;
            
            if (!string.IsNullOrEmpty(Cache.accessToken))
            {
                Cache.isModel = true;
                await navigationService.NavigateAsync<TwitterSignInViewModel>();
            }
        }
        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            try
            {
                if (!Cache.isModel)
                    GotoModel();
            }
            catch (Exception ex)
            {
                var a = ex;
            }
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            this.navigationService = (INavigationService)parameters[NavigationParametersConstants.NavigatonServiceParameter];
        }        
    }
}