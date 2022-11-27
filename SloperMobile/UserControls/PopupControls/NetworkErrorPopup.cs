using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;

namespace SloperMobile.UserControls.PopupControls
{
	public partial class NetworkErrorPopup : PopupPage
	{														 
        public NetworkErrorPopup()
		{
			InitializeComponent();
        }

		private async void OnContinue(object sender, EventArgs e)
		{
            try
            {
                if (PopupNavigation.PopupStack.Count > 0)
                    await PopupNavigation.PopAllAsync(false);
            }
            catch {
                //(App.Current as App).SetRootPage();
            }
		}
    }
}