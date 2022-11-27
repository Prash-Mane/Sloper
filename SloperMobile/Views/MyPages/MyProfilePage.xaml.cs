using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SloperMobile.Common.Helpers;

namespace SloperMobile.Views.MyPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MyProfilePage : ContentPage
    {
        readonly IRepository<CragExtended> cragRepository;
        bool isLoaded;

        public MyProfilePage(IRepository<CragExtended> cragRepository)
        {
            this.cragRepository = cragRepository;

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
                //vm.IsShowFooter = vm.IsMenuVisible && GeneralHelper.IsCragsDownloaded;
            }
        }
    }
}
