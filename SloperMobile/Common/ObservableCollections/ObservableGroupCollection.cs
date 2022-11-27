using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SloperMobile.Common.Helpers
{
	public class ObservableGroupCollection<K, T> : ObservableCollection<T>
    {
        private readonly K _key;

        public ObservableGroupCollection(IGrouping<K, T> group)
            : base(group)
        {
            _key = group.Key;
        }

        public ObservableGroupCollection(K key, IEnumerable<T> items)
        {
            _key = key;
            foreach (var item in items)
                this.Items.Add(item);
        }

        public K Key
        {
            get { return _key; }
        }
    }
}
