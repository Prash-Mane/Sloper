using SloperMobile.Common.Constants;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.SectorPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SectorRoutesPage : ContentPage
    {  
	    public SectorRoutesPage()
	    {
		    InitializeComponent();
		    Cache.SendBackArrowCount = 2;
		    NavigationPage.SetHasNavigationBar(this, false);	   
	    }
    }
}
