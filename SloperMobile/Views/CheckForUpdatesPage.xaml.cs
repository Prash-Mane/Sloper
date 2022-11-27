using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CheckForUpdatesPage : ContentPage
    {
        public CheckForUpdatesPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
