using System.ComponentModel;
using Xamarin.Forms;

namespace SloperMobile
{
    public interface ISelectionItem : INotifyPropertyChanged
    {
        Color Color { get; }

        bool Selected { get; set; }
    }
}
