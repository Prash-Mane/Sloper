using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.SubscriptionPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SubscriptionPage : ContentPage
	{
		public SubscriptionPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}
	}
}