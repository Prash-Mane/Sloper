using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace SloperMobile.CustomControls
{
    public class SegmentedControl<T> : StackLayout where T : ISelectionItem
    {
        public IEnumerable<T> ItemsSource
        {
            get => (IEnumerable<T>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand ItemSelectedCommand
        {
            get => (ICommand)GetValue(ItemSelectedCommandProperty);
            set => SetValue(ItemSelectedCommandProperty, value);
        }

        public DataTemplate ItemTemplate { get; set; }

        public static readonly BindableProperty ItemSelectedCommandProperty = BindableProperty.Create(
            nameof(ItemSelectedCommand),
            typeof(ICommand),
            typeof(SegmentedControl<T>),
            default(Command));

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable<T>),
            typeof(SegmentedControl<T>),
            propertyChanged: ItemsSourceChanged);


        static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                var itemsLayout = (SegmentedControl<T>)bindable;

                itemsLayout.SetItems();
            }
        }

        void SetItems()
        {
            Children.Clear();
            if (ItemsSource == null)
            {
                return;
            }

            foreach (var item in ItemsSource)
            {
                var view = CreateItemView(item);

                Children.Add(view);
            }
        }

        View CreateItemView(T item)
        {
            var content = ItemTemplate.CreateContent();

            if (!(content is View view))
            {
                return null;
            }

            var container = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand };

            view.BindingContext = item;

            container.Children.Add(view);
            var indicator = new BoxView
            {
                HeightRequest = 2,
                BackgroundColor = Color.Transparent
            };
            indicator.BindingContext = item;
            indicator.SetBinding(BoxView.ColorProperty, nameof(item.Color));

            container.Children.Add(indicator);

            var gesture = new TapGestureRecognizer((v) => OnTapGesture(item));

            container.GestureRecognizers.Add(gesture);
            SetSelection(indicator, item);

            return container;
        }


        void OnTapGesture(T item)
        {
            item.Selected = !item.Selected;
            ItemSelectedCommand?.Execute(item);
        }

        void SetSelection(BoxView indicator, T item) => indicator.BackgroundColor = item.Selected ? item.Color : Color.Transparent;

        //Uncomment if we want items evenly sized
        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);

        //    if (ItemsSource != null && ItemsSource.Any() && Children.Any())
        //    {
        //        foreach (var v in Children)
        //            v.WidthRequest = width / ItemsSource.Count();
        //    }
        //}
    }
}
