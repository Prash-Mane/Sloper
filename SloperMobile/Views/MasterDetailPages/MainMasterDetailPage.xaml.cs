using SloperMobile.Common.Constants;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.FlyoutPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainFlyoutPage : FlyoutPage
    {
        public MainFlyoutPage()
		{
			App.AreSectorPages = false;
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Cache.MasterPage = this;
        }
    }
}
