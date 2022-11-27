using Xamarin.Forms;
using System.Windows.Input;
using System.Collections;
using SloperMobile.Model.SocialModels;
using System;

namespace SloperMobile.CustomControls
{
    /// <summary>
    /// A simple listview that exposes a bindable command to allow infinite loading behaviour.
    /// </summary>
    public class InfiniteListView : ListView
    {
        /// <summary>
		/// Respresents the command that is fired to ask the view model to load additional data bound collection.
		/// </summary>
        public static readonly BindableProperty LoadMoreCommandProperty = BindableProperty.Create(nameof(LoadMoreCommand), typeof(ICommand), typeof(InfiniteListView), null, BindingMode.OneWay);


        /// <summary>
        /// Gets or sets the command binding that is called whenever the listview is getting near the bottom of the list, and therefore requiress more data to be loaded.
        /// </summary>
        public ICommand LoadMoreCommand
        {
            get { return (ICommand)GetValue(LoadMoreCommandProperty); }
            set { SetValue(LoadMoreCommandProperty, value); }
        }

        /// <summary>
        /// Creates a new instance of a <see cref="InfiniteListView" />
        /// </summary>
        public InfiniteListView()
        {
            ItemAppearing += InfiniteListView_ItemAppearing;
            ItemTapped += InfiniteListView_ItemTapped;
        }


        void InfiniteListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            var items = ItemsSource as IList;
            if (items == null || e.Item != items[items.Count - 1] || items.Count <= 3)
            {
                return;
            }

            if (LoadMoreCommand != null && LoadMoreCommand.CanExecute(null))
            {
                LoadMoreCommand.Execute(null);
            }
        }

        void InfiniteListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row
            if (e.Item == null) return;
            // do something with e.SelectedItem
            ((ListView)sender).SelectedItem = null; // de-select the row after ripple effect
        }
    }
}
