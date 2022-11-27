using Xamarin.Forms;

namespace SloperMobile.Views
{
    public partial class ApproachMapPage : ContentPage
    {
        public ApproachMapPage()
        {
            InitializeComponent();
            gMap.UiSettings.ZoomControlsEnabled = false;
        }
    }
}
