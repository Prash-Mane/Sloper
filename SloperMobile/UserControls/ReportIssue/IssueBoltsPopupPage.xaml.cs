using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SloperMobile.Model;
using SloperMobile.ViewModel;
using SloperMobile.ViewModel.ReportedIssueViewModels;
using Xamarin.Forms;

namespace SloperMobile.UserControls.ReportIssue
{
	public partial class IssueBoltsPopupPage : PopupPage
    {
        ReportIssueViewModel reportVM;
        List<Bolts> selectedboltstoremoveoncancel;
        List<Bolts> selectedboltstoaddoncancel;
        public IssueBoltsPopupPage(ReportIssueViewModel objBolts)
        {
            InitializeComponent();
            selectedboltstoremoveoncancel = new List<Bolts>();
            selectedboltstoaddoncancel = new List<Bolts>();
            reportVM = objBolts;
            BindingContext = reportVM;
            if (!string.IsNullOrEmpty(reportVM.SelectedBoltNumber))
            {
                foreach (var strbolt in reportVM.IssueBolts.Where(item => item.IsSelected))
                {
					strbolt.Color = "#FFFFFF";
					strbolt.BackgroundItemColor = "#333333";
                    synf_listView.SelectedItems.Add(strbolt);
                }
            }
        }
        
        private async void OnClose(object sender, EventArgs e)
        {
            if (PopupNavigation.PopupStack.Count > 0)
                await PopupNavigation.PopAllAsync();
			foreach (var selectedItem in reportVM.IssueBolts.Where(item => !item.IsSelected))
			{
				selectedItem.BackgroundItemColor = "#FFFFFF";
				selectedItem.Color = "#333333";
				synf_listView.SelectedItems.Remove(selectedItem);
			}
            foreach(var item in selectedboltstoremoveoncancel)
            {
                item.BackgroundItemColor = "#FFFFFF";
                item.Color = "#333333";
                item.IsSelected = false;
                synf_listView.SelectedItems.Remove(item);
            }

            foreach (var item in selectedboltstoaddoncancel)
            {
                item.Color = "#FFFFFF";
                item.BackgroundItemColor = "#333333";
                item.IsSelected = true;
                synf_listView.SelectedItems.Add(item);
            }
        }

        protected override Task OnAppearingAnimationEndAsync()
        {
            return Content.FadeTo(1);
        }

        protected override Task OnDisappearingAnimationBeginAsync()
        {
            return Content.FadeTo(1);
        }
        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row
            if (e.Item == null)
            {
                return;
            }
            //reportVM.SelectedBoltNumber = ((Bolts)e.Item).Bolt;
            // do something with e.SelectedItem
            //((ListView)sender).SelectedItem = null;
            // de-select the row after ripple effect

        }

        private async void Ok_Clicked(object sender, EventArgs e)
        {
			if (synf_listView.SelectedItems.Count == 0 && reportVM.IssueBolts.Count(item => item.IsSelected) == 0)
			{
				await UserDialogs.Instance.AlertAsync("Please select at least 1 bolt.", "Bolt Selection Error", "Continue");
				return;
			}
            if (PopupNavigation.PopupStack.Count > 0)
                await PopupNavigation.PopAllAsync();
            string nums = string.Empty;
			foreach(var ordered in synf_listView.SelectedItems.Cast<Bolts>().OrderBy(item => item.Bolt))
			{
				nums = $"{nums}{ordered.Bolt}|";
				reportVM.IssueBolts.FirstOrDefault(item => item.Bolt == ordered.Bolt).IsSelected = true;
			}

			reportVM.SelectedBoltNumber = nums.TrimEnd('|');
            reportVM.OkBoltCommand.Execute(null);
        }

        private void synf_listView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            for (int i = 0; i < e.AddedItems.Count; i++)
            {
                var item = e.AddedItems[i];
                (item as Bolts).Color = "#FFFFFF";
				(item as Bolts).BackgroundItemColor = "#333333";
                (item as Bolts).IsSelected = true;
                selectedboltstoremoveoncancel.Add(item as Bolts);
            }

            for (int i = 0; i < e.RemovedItems.Count; i++)
            {
                var item = e.RemovedItems[i];
                (item as Bolts).Color = "#333333";
				(item as Bolts).BackgroundItemColor = "#FFFFFF";
                (item as Bolts).IsSelected = false;
                selectedboltstoaddoncancel.Add(item as Bolts);
            }
        }
    }
}
