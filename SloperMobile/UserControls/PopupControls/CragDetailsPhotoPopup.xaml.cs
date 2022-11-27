using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SloperMobile.ViewModel;
using System;
using Xamarin.Forms.Xaml;

namespace SloperMobile.UserControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CragSummaryCameraPage : PopupPage
    {
        private CragDetailsViewModel _cragDetailViewModel;
        public CragSummaryCameraPage(CragDetailsViewModel cragDetailViewModel)
        {
            InitializeComponent();
            _cragDetailViewModel = cragDetailViewModel;
            BindingContext = _cragDetailViewModel;
        }
        private void OnClose(object sender, EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
                PopupNavigation.PopAllAsync();
        }
    }
}