using System.Linq;
using Android.Views;
using SkiaSharp.Views.Forms;
using SloperMobile.Droid.Renderers;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ZoomableScrollView), typeof(ZoomableScrollViewRenderer))]

namespace SloperMobile.Droid.Renderers
{
    public class ZoomableScrollViewRenderer : ScrollViewRenderer, ScaleGestureDetector.IOnScaleGestureListener
    {
		private ZoomableScrollView svMain;
        private AbsoluteLayout absoluteLayout;
        private ScaleGestureDetector _scaleDetector;
	    private SKCanvasView canvasView;
	    private float mScale = 1;
        private bool isScaleProcess;
		private bool firstScale = true;
	    private float firstPivotX;
	    private float firstPivotY;

	    private double initialX;
	    private double initialY;
	    private double x;
	    private double y;
	    private double toScrollYRatio;
	    private double toScrollXRatio;
	    private double previousWidth = -1;
	    private double previousHeight = -1;
	    private double actualHeight;
	    private double actualWidth;
	    private float toScale;

		public ZoomableScrollViewRenderer()
		{
			_scaleDetector = new ScaleGestureDetector(Context, this);
		}

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            try
            {
                svMain = ((ZoomableScrollView)e.NewElement);
	            svMain.Scrolled += (sender, args) =>
	            {
		            x = args.ScrollX;
		            y = args.ScrollY;
	            };
	            svMain.ScaleFactor = mScale;
                absoluteLayout = svMain.Content as AbsoluteLayout;
	            canvasView = absoluteLayout.Children.FirstOrDefault() as SKCanvasView;
            }
            catch (System.Exception)
            {
            }
        }

		public override bool DispatchTouchEvent(Android.Views.MotionEvent e)
		{
			if (e.PointerCount == 1)
			{
				svMain.IsScrolling = true;
			}

			if (e.PointerCount == 2)
			{
				return _scaleDetector.OnTouchEvent(e);
			}
			else if (isScaleProcess)
			{
				//HACK:
				//Prevent letting any touch events from moving the scroll view until all fingers are up from zooming...This prevents the jumping and skipping around after user zooms.
				if (e.Action == MotionEventActions.Up)
				{
					isScaleProcess = false;
				}

				if (e.Action == MotionEventActions.Down)
				{
					isScaleProcess = false;
				}

				return false;
			}

			return base.DispatchTouchEvent(e);
		}

		public bool OnScale(ScaleGestureDetector detector)
        {
	        var scale = detector.ScaleFactor;
	        mScale += scale;
	        if (detector.ScaleFactor < 1)
	        {
		        mScale = detector.ScaleFactor;
	        }

	        if (mScale < 0.7f)
		        mScale = 0.7f;

	        if (mScale > 1.5f) 
		        mScale = 1.3f;

			return true;
		}

		private void CalculateScrollRatio()
		{
			actualWidth = absoluteLayout.WidthRequest;
			actualHeight = absoluteLayout.HeightRequest;
			toScrollXRatio = actualWidth / previousWidth;
			toScrollYRatio = actualHeight / previousHeight;
			previousWidth = actualWidth;
			previousHeight = actualHeight;
		}

	    public bool OnScaleBegin(ScaleGestureDetector detector)
        {
	        if (previousWidth == -1)
	        {
		        previousWidth = svMain.InitialWidth;
	        }

	        if (previousHeight == -1)
	        {
		        previousHeight = svMain.InitialHeight;
	        }

	        svMain.IsScrolling = false;
			x = svMain.ScrollX;
	        y = svMain.ScrollY;
	        initialX = x;
	        initialY = y;
			if (y == 0)
	        {
		        y = detector.FocusY / svMain.DeviceScaleFactor;
	        }

	        if (x == 0)
	        {
		        x = detector.FocusX / svMain.DeviceScaleFactor;
	        }

			return true;
        }

        public async void OnScaleEnd(ScaleGestureDetector detector)
		{
			toScale = mScale;
			if ((absoluteLayout.Height * toScale - absoluteLayout.Height * 0.2 < svMain.InitialHeight)
			    || (absoluteLayout.Width * toScale - absoluteLayout.Width * 0.2 < svMain.InitialWidth))
			{
				CalculateScrollRatio();
				x = initialX;
				y = initialY;
				absoluteLayout.HeightRequest = svMain.InitialHeight;
				absoluteLayout.WidthRequest = svMain.InitialWidth;
				svMain.RescaleOnAndroid();
				mScale = 1;
				return;
			}

			if (absoluteLayout.Height * toScale > svMain.InitialHeight * 1.9)
			{
				CalculateScrollRatio();
				x = initialX;
				y = initialY;
				mScale = 1;
				return;
			}

			svMain.ScaleFactor = toScale;
			absoluteLayout.HeightRequest = absoluteLayout.Height * toScale;
			absoluteLayout.WidthRequest = absoluteLayout.Width * toScale;
			svMain.ForceLayout();
			CalculateScrollRatio();
			await svMain.ScrollToAsync(toScrollXRatio * x, toScrollYRatio * y, false);
			mScale = 1;
			svMain.IsScalingDown = true;
            svMain.IsScalingUp = false;
	        svMain.IsScrolling = true;
		}
    }
}