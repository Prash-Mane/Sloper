using System;
using System.Collections.Generic;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace SloperMobile
{
    public partial class ProfilePointsView : ContentView
    {
        public ProfilePointsView()
        {
            InitializeComponent();
        }

        private void SfChart_SelectionChanging(object sender, ChartSelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}