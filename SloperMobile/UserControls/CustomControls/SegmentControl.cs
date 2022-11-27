using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.CustomControls
{
    public class SegmentControl<T> : StackLayout
    {
        #region Bindable Properties

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
            nameof(SelectedItem),
            typeof(T),
            typeof(SegmentControl<T>),
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: OnSelectedItemChanged);

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable<T>),
            typeof(SegmentControl<T>),
            propertyChanged: ItemsSourceChanged);

        public static readonly BindableProperty ItemSelectedCommandProperty = BindableProperty.Create(
            nameof(ItemSelectedCommand),
            typeof(ICommand),
            typeof(SegmentControl<T>));

        public static readonly BindableProperty SelectedItemsCommandProperty = BindableProperty.Create(
            nameof(SelectedItemsCommand),
            typeof(ICommand),
            typeof(SegmentControl<T>));

        #endregion

        #region Constructor

        public SegmentControl()
        {
            Orientation = StackOrientation.Horizontal;

            DefaultItemSelectedCommand = new Command<T>(DefaultItemSelectedCommandExecute);

            SelectedIndexes = new Dictionary<int, bool>();

            SelectedItems = new List<T>();
        }

        #endregion

        #region Public Properties

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        IEnumerable<T> ItemsSource
        {
            get => (IEnumerable<T>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand ItemSelectedCommand
        {
            get => (ICommand)GetValue(ItemSelectedCommandProperty);
            set => SetValue(ItemSelectedCommandProperty, value);
        }

        public ICommand SelectedItemsCommand
        {
            get => (ICommand)GetValue(SelectedItemsCommandProperty);
            set => SetValue(SelectedItemsCommandProperty, value);
        }

        public DataTemplate ItemTemplate { get; set; }

        public DataTemplate SelectedItemTemplate { get; set;}

        public bool SetMultipleSelection { get; set; }

        public bool FirstAsDefault { get; set; }

        private IDictionary<int, bool> SelectedIndexes { get; set; }

        public IList<T> SelectedItems { get; private set; }

        #endregion

        #region Private Properties

        private ICommand DefaultItemSelectedCommand { get; set; }

        #endregion

        #region Event handler methods

        private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                var itemsLayout = (SegmentControl<T>)bindable;

                itemsLayout.SetItems();
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                var itemsLayout = (SegmentControl<T>)bindable;

                itemsLayout.SetSelectedItem((T)newValue, (T)oldValue);
            }
        }

        #endregion

        #region Private methods

        private void DefaultItemSelectedCommandExecute(T item)
        {
            if (SetMultipleSelection)
            {
                SetSelectedItems(item);

                if (SelectedItemsCommand?.CanExecute(SelectedItems) ?? false)
                {
                    SelectedItemsCommand.Execute(SelectedItems);
                }
            }
            else
            {
                SelectedItem = item;

                if (ItemSelectedCommand?.CanExecute(item) ?? false)
                {
                    ItemSelectedCommand.Execute(item);
                }
            }
        }

        private void SetSelectedItem(T selectedItem, T oldSelectedItem)
        {
            if(selectedItem != null)
            {
                var (view, index) = GetViewAndIndex(selectedItem);

                var selectedView = CreateItemView(selectedItem, SelectedItemTemplate);

                Children[index] = selectedView;

                SelectedItems.Add(selectedItem);
            }

            if(oldSelectedItem != null)
            {
                var (view, index) = GetViewAndIndex(oldSelectedItem);

                var unselectedView = CreateItemView(oldSelectedItem, ItemTemplate);

                Children[index] = unselectedView;

                SelectedItems.Remove(oldSelectedItem);
            }
        }

        private void SetSelectedItems(T item)
        {
            var (view, index) = GetViewAndIndex(item);

            var isSelected = SelectedIndexes[index] = !SelectedIndexes[index];

            if(isSelected)
            {
                var viewSelected = CreateItemView(item, SelectedItemTemplate);
               
                Children[index] = viewSelected;

                SelectedItems.Add(item);
            }
            else
            {
                var viewUnselected = CreateItemView(item, ItemTemplate);

                Children[index] = viewUnselected;

                SelectedItems.Remove(item);
            }
        }

        private void SetItems()
        {
            Children.Clear();
            SelectedItems.Clear();
            var index = 0;

            if (ItemsSource == null)
            {
                return;
            }

            foreach (var item in ItemsSource)
            {
                var view = CreateItemView(item, ItemTemplate);

                Children.Add(view);
                SelectedIndexes.Add(index++, false);
            }

            if (FirstAsDefault && ItemsSource.Any() && SelectedItem == null)
            {
                if (SetMultipleSelection)
                {
                    SetSelectedItems(ItemsSource.First());
                }
                else
                {
                    SelectedItem = ItemsSource.First();
                }
            }
        }

        private View CreateItemView(T item, DataTemplate dataTemplate)
        {
            var content = dataTemplate.CreateContent();

            if (!(content is View view))
            {
                return null;
            }

            view.BindingContext = item;

            if (view.GestureRecognizers.Count == 0)
            {
                var gesture = new TapGestureRecognizer
                {
                    Command = DefaultItemSelectedCommand,
                    CommandParameter = item
                };

                view.GestureRecognizers.Add(gesture);
            }

            return view;
        }

        private (View view, int index) GetViewAndIndex(object item)
        {
            View itemView = null;
            var index = 0;

            foreach (var child in Children)
            {
                if (child.BindingContext == item)
                {
                    itemView = child;

                    return (itemView, index);
                }

                index++;
            }
           
            return (itemView, index);
        }

        #endregion
    }
}
