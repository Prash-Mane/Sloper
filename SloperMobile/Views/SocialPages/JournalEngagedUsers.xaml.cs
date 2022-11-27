using Acr.UserDialogs;
using Prism.Navigation;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Interfaces;
using SloperMobile.ViewModel.SocialModels;
using System;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.SocialPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JournalEngagedUsers : PopupPage
    {
        private JournalEngagedUsersViewModel journalEngagedVM;
        public JournalEngagedUsers(JournalEngagedUsersViewModel journalEngVM)
        {
            InitializeComponent();
            journalEngagedVM = journalEngVM;
            BindingContext = journalEngagedVM;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await journalEngagedVM.LoadUsers();
        }
        private void OnClose(object sender, EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
                PopupNavigation.PopAllAsync();
        }
    }
}