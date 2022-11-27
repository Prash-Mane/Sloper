using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.UserPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UserRegistrationPage : ContentPage
    {
        public UserRegistrationPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
