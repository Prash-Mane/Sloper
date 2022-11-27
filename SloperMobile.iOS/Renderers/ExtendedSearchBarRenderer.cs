using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using UIKit;

[assembly: ExportRenderer(typeof(SearchBar), typeof(Namespace.iOS.Renderers.ExtendedSearchBarRenderer))]
namespace Namespace.iOS.Renderers
{
    public class ExtendedSearchBarRenderer : SearchBarRenderer
    {
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Text")
            {
                Control.ShowsCancelButton = false;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);
            if (this.Element != null) {
                Element.BackgroundColor = Color.Transparent;
            }
        }
    }
}