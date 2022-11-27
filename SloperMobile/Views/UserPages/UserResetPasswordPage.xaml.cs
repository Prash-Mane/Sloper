using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.UserPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserResetPasswordPage : ContentPage
    {
        public UserResetPasswordPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}