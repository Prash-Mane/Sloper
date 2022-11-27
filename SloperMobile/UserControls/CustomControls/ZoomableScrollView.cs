using System;
using Xamarin.Forms;

namespace SloperMobile.ViewModel
{
    public class ZoomableScrollView : ScrollView
    {
        public static readonly BindableProperty MinimumZoomScaleProperty = BindableProperty.Create("MinimumZoomScale", typeof(float), typeof(ZoomableScrollView), default(float));

        public float MinimumZoomScale
        {
            get { return (float)GetValue(MinimumZoomScaleProperty); }
            set { SetValue(MinimumZoomScaleProperty, value); }
        }

		public double InitialHeight
		{
			get;
			set;
		}

		public static readonly BindableProperty MaximumZoomScaleProperty = BindableProperty.Create("MaximumZoomScale", typeof(float), typeof(ZoomableScrollView), default(float));

        public float MaximumZoomScale
        {
            get { return (float)GetValue(MaximumZoomScaleProperty); }
            set { SetValue(MaximumZoomScaleProperty, value); }
        }

        public bool IsScalingUp { get; set; }
        public bool IsScalingDown { get; set; }
		public bool IsScrolling { get; set; }

        public Action RescaleOniOS { get; set; }
		public Action RescaleOnAndroid { get; set; }

		public float ScaleFactor { get; set; }

		public double DeviceScaleFactor { get; set; }
	    public double InitialWidth { get; set; }

	    public void LayoutChildrenFromAndroid(double x, double y, double width, double height)
	    {
		    LayoutChildren(x, y, width, height);
	    }
    }
}
