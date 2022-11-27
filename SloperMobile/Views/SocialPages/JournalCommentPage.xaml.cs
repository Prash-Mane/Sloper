using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Model.SocialModels;
using SloperMobile.ViewModel.SocialModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration;

namespace SloperMobile.Views.SocialPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JournalCommentPage : ContentPage
    {
        private bool _autoScroll = true;
        bool isLoaded;

        public JournalCommentPage()
        {
            InitializeComponent();
            listViewcomment.ItemAppearing += OnItemAppearing;
            listViewcomment.ItemDisappearing += OnItemDisappearing;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isLoaded)
                (BindingContext as JournalCommentViewModel).OnPostAdded += ScrollToLastRow;

            isLoaded = true;
        }

        private void OnItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (_autoScroll)
                ScrollToLastRow();
        }

        private void OnItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            if (e.Item == (BindingContext as JournalCommentViewModel).CommentList.Last())
                _autoScroll = false;
        }

        void ScrollToLastRow()
        { 
            listViewcomment.ScrollTo((BindingContext as JournalCommentViewModel).CommentList.Last(), ScrollToPosition.MakeVisible, true);
        }
    }
}
