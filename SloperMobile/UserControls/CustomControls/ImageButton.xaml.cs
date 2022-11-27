using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SloperMobile.CustomControls
{
    public partial class ImageButton
    {
        public static BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource),
                                                                                     typeof(string),
                                                                                     typeof(ImageButton),
                                                                                     string.Empty);

        public static BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand),
                                                                                    typeof(Command),
                                                                                    typeof(ImageButton),
                                                                                    null);

        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter),
                                                                                    typeof(object),
                                                                                    typeof(ImageButton),
                                                                                    null);

        public static BindableProperty ImageSizeProperty = BindableProperty.Create(nameof(ImageSize),
                                                                                    typeof(double),
                                                                                    typeof(ImageButton),
                                                                                    default(double));

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public Command TapCommand
        {
            get => (Command)GetValue(TapCommandProperty);
            set => SetValue(TapCommandProperty, value);
        }

        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public double ImageSize
        {
            get
            {
                var newSize = (double)GetValue(ImageSizeProperty);
                var coef = SizeHelper.GetResizeCoeficientsAsync().Result;
                var widthCoef = coef.x;
                var heightCoef = coef.y;

                double mediumCoef = 1;
                if (widthCoef >= 1 && heightCoef >= 1)
                    mediumCoef = Math.Min(widthCoef, heightCoef);
                else if (widthCoef <= 1 && heightCoef <= 1)
                    mediumCoef = Math.Max(widthCoef, heightCoef);
                return newSize * mediumCoef;
                //return (double)GetValue(ImageSizeProperty);
            }
            set => SetValue(ImageSizeProperty, value);
        }

        public ImageButton()
        {
            InitializeComponent();
        }
    }
}
