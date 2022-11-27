using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SloperMobile.Common.NotifyProperties
{
	public class ObservableCollectionFast<T> : ObservableCollection<T>
	{
		public ObservableCollectionFast() : base() { }

		public ObservableCollectionFast(IEnumerable<T> collection) : base(collection) { }

		public ObservableCollectionFast(List<T> list) : base(list) { }

		public void AddRange(IEnumerable<T> range)
		{
			foreach (var item in range)
			{
				Items.Add(item);
			}

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
			this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

        public void AddRangeToTop(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Insert(0, item);
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
		{
			this.Items.Clear();

			AddRange(range);
		}
	}
}
