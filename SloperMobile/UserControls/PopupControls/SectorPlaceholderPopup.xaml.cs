using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SloperMobile.ViewModel;

namespace SloperMobile.UserControls.PopupControls
{
    public partial class SectorPlaceholderPopup : PopupPage
    {
        public string PageName { get; set; }

        public SectorPlaceholderPopup(BaseViewModel viewModel, string pageName)
        {
            BindingContext = viewModel;
            PageName = pageName;
            InitializeComponent();
            CloseWhenBackgroundIsClicked = false;
        }

        private async void OnClose(object sender, EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
                await PopupNavigation.PopAllAsync();
        }
    }
}
