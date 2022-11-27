using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.GuideBooks
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GuideBookDetailPage : ContentPage
    {
        public GuideBookDetailPage()
        {
            InitializeComponent();
        }

        private void listvw_GBCrags_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row
            if (e.Item == null) return;
            // do something with e.SelectedItem
            ((ListView)sender).SelectedItem = null; // de-select the row after ripple effect
        }
    }
}