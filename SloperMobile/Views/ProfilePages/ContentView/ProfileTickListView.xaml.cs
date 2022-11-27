using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using SloperMobile.Model;
using SloperMobile.ViewModel.ProfileViewModels;
using Syncfusion.ListView.XForms;
using Xamarin.Forms;

namespace SloperMobile
{
    public partial class ProfileTickListView : ContentView
    {
        public ProfileTickListView()
        {
            InitializeComponent();

//            var ss = App.ContainerProvider.Resolve(typeof(ProfileTickListViewModel), typeof(ProfileTickListView).FullName);
        }

        private async void listView_ItemHolding(object sender, ItemHoldingEventArgs e)
        {
            if (!(BindingContext is ProfileTickListViewModel vm) || !vm.IsMe)
                return;

            var data = e.ItemData as TickListModel;
            string message = $"Route: {data.route_name}";
            var result = await UserDialogs.Instance.ConfirmAsync(message, "Delete Tick?", "YES", "NO");
            if (result)
                (BindingContext as ProfileTickListViewModel)?.DeleteTick(data);
        }
    }
}
