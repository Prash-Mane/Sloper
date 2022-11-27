using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SloperMobile.Common.Extentions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MicrosoftLiveSignIn : ContentPage, INavigationAware
    {
        private INavigationService navigationService;
        public bool gotomodel;

        public MicrosoftLiveSignIn(INavigationService navigationService)
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
                await navigationService.NavigateAsync<MicrosoftLiveSignInViewModel>();
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