using System;
using System.Collections.Generic;
using System.ComponentModel;
using Acr.UserDialogs;
using SloperMobile.Model.SendsModels;
using SloperMobile.ViewModel.ProfileViewModels;
using Syncfusion.ListView.XForms;
using Xamarin.Forms;

namespace SloperMobile
{
    public partial class ProfileSendsView : ContentView
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private double pointerValue = 60;
        public double PointerValue
        {
            get { return pointerValue; }
            set
            {
                pointerValue = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs("PointerValue"));
                }
            }
        }

        public ProfileSendsView()
        {
            InitializeComponent();

        }

        private void listView_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            var data = e.ItemData as SendModel;
            if (BindingContext is ProfileSendsViewModel vm)
                vm.ShowDetails(data);
        }

        private async void listView_ItemHolding(object sender, ItemHoldingEventArgs e)
        {
            var data = e.ItemData as SendModel;
            if (BindingContext is ProfileSendsViewModel vm)
            {
                if (!vm.IsMe)
                    return;

                string message = $"Date: {data.DateClimbed} \nClimb: {data.route_name}";
                var result = await UserDialogs.Instance.ConfirmAsync(message, "Delete Ascent?", "YES", "NO");
                if (result)
                    vm.DeleteItem(data);
            }
        }
    }
}
