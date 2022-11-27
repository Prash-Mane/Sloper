using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.UserControls.PopupControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShowWrapTextPopoup : PopupPage
	{
		public ShowWrapTextPopoup(string title,string Text)
		{
			InitializeComponent();
			Title.Text = title;
			label.Text = Text;				
		}

		private async void OnClose(object sender, EventArgs e)
		{
			if (PopupNavigation.PopupStack.Count > 0)
				await PopupNavigation.PopAllAsync();
		}
	}
}