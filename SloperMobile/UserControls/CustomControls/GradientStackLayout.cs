using System.Collections.Generic;
using Xamarin.Forms;
namespace SloperMobile.UserControls
{
    public class GradientStackLayout : StackLayout
    {
        public GradientStackLayout()
        {
            Colors = new List<GradientColor>();
        }

        public static readonly BindableProperty HasFadeBackgroundProperty = BindableProperty.Create(
            nameof(HasFadeBackground),
            typeof(bool), 
            typeof(GradientStackLayout),
            true);

        public bool HasFadeBackground
        {
            get { return (bool)GetValue(HasFadeBackgroundProperty); }
            set { SetValue(HasFadeBackgroundProperty, value); }
        }

        public static readonly BindableProperty ColorsProperty = BindableProperty.Create(
            nameof(Colors),
            typeof(List<GradientColor>),
            typeof(GradientStackLayout),
            null);

        public List<GradientColor> Colors 
        {
            get => (List<GradientColor>)GetValue(ColorsProperty);
            set => SetValue(ColorsProperty, value);
        }
    }

    public class GradientColor
    { 
        public Color Color { get; set; }
        public float Position { get; set; }
    }
}
