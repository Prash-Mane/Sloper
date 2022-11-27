using System.Collections.Generic;
using SloperMobile.DataBase.DataTables;
using Xamarin.Forms;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Mvvm;

namespace SloperMobile.Model
{
    public class BucketsSegmentModel : BindableBase,  ISelectionItem
    {
        private bool selected;

        public IEnumerable<BucketTable> Buckets { get; set; }

        public Color Color 
        { 
            get 
            {
                if (!selected)
                    return Color.Transparent;

                try
                {
                    var hexColor = Buckets?.FirstOrDefault()?.hex_code;
                    if (!string.IsNullOrEmpty(hexColor))
                        return Color.FromHex(hexColor);
                }
                catch { }
                return Color.AliceBlue;
            } 
        }

        public bool Selected
        {
            get => selected;
            set {
                if (SetProperty(ref selected, value))
                    RaisePropertyChanged(nameof(Color));
            }
        }
    }
}
