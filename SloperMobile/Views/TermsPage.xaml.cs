using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TermsPage : ContentPage
    {
        bool isLoaded;

        public TermsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
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
