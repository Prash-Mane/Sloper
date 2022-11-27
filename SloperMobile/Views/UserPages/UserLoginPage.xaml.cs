using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.UserPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UserLoginPage : ContentPage
    {
        public UserLoginPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
