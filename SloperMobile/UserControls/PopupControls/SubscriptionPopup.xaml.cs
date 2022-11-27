using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;

namespace SloperMobile.UserControls.PopupControls
{
	public partial class SubscriptionPopup : PopupPage
	{														 
        public SubscriptionPopup(string description)
		{
			InitializeComponent();
            label.Text = description;
        }

		private async void OnClose(object sender, EventArgs e)
		{
            if (PopupNavigation.PopupStack.Count > 0)
			    await PopupNavigation.PopAllAsync();
		}
	}
}