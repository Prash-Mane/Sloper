using Rg.Plugins.Popup.Pages;
using SloperMobile.ViewModel;

namespace SloperMobile.Views.CragPages
{
    public partial class RemoveCragPopup : PopupPage
    {
        public RemoveCragPopup(BaseViewModel viewModel)
        {
            InitializeComponent();
            CloseWhenBackgroundIsClicked = false;
            BindingContext = viewModel;
        }
    }
}
