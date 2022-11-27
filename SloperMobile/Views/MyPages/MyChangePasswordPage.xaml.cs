using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.MyPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MyChangePasswordPage : ContentPage
	{
		public MyChangePasswordPage()
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
		}
	}
}