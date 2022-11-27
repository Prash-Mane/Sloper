using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.CragPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CragDetailsPage : ContentPage
	{
		
        bool isLoaded;
        public CragDetailsPage(IRepository<AppSettingTable> appSetingsRepository)
		{
            InitializeComponent();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (isLoaded)
                return;
            isLoaded = true;

            if (BindingContext is BaseViewModel vm)
            { 
                vm.IsMenuVisible = Navigation.NavigationStack.Count == 1;
                vm.IsBackButtonVisible = !vm.IsMenuVisible;
            }
            
        }       

    }
}
