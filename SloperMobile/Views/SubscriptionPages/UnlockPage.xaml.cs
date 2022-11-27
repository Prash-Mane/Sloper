using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.SubscriptionPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UnlockPage : ContentPage
	{
		public UnlockPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}
	}
}