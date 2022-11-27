using SloperMobile.CustomControls;
using SloperMobile.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SwipeGestureFrame), typeof(GestureFrameRenderer))]
namespace SloperMobile.iOS.Renderers
{
	public class GestureFrameRenderer : FrameRenderer
	{
		private SwipeGestureFrame frameElement;
		private UISwipeGestureRecognizer swipeDown;
        private UISwipeGestureRecognizer swipeUp;
        private UISwipeGestureRecognizer swipeLeft;
        private UISwipeGestureRecognizer swipeRight;
		private UITapGestureRecognizer tap;

		public GestureFrameRenderer()
        {
            BackgroundColor = UIColor.Black;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            BackgroundColor = UIColor.Black;
            base.OnElementChanged(e);
	        frameElement = (SwipeGestureFrame) this.Element;

	        swipeDown = new UISwipeGestureRecognizer(() =>
		        frameElement.SwipeDownCommand?.Execute(frameElement.BindingContext))
	        {
		        Direction = UISwipeGestureRecognizerDirection.Down,
	        };

            swipeUp = new UISwipeGestureRecognizer(() =>
	                frameElement.SwipeUpCommand?.Execute(frameElement.BindingContext))
            {
                Direction = UISwipeGestureRecognizerDirection.Up,
            };

	        swipeLeft = new UISwipeGestureRecognizer(() =>
		        frameElement.SwipeLeftCommand?.Execute(frameElement.BindingContext))
	        {
		        Direction = UISwipeGestureRecognizerDirection.Left,
	        };

	        swipeRight = new UISwipeGestureRecognizer(() =>
		        frameElement.SwipeRightCommand?.Execute(frameElement.BindingContext))
	        {
		        Direction = UISwipeGestureRecognizerDirection.Right,
	        };

            tap = new UITapGestureRecognizer(() =>
            {
                frameElement.TapCommand?.Execute(frameElement.BindingContext);
            });

			if (e.NewElement == null)
            {
                if (swipeDown != null)
                {
                    this.RemoveGestureRecognizer(swipeDown);
                }

                if (swipeUp != null)
                {
                    this.RemoveGestureRecognizer(swipeUp);
                }

                if (swipeLeft != null)
                {
                    this.RemoveGestureRecognizer(swipeLeft);
                }

                if (swipeRight != null)
                {
                    this.RemoveGestureRecognizer(swipeRight);
                }

	            if (tap != null)
	            {
		            this.RemoveGestureRecognizer(tap);
	            }
			}

	        if (e.OldElement != null)
	        {
		        return;
	        }

	        this.AddGestureRecognizer(swipeDown);
	        this.AddGestureRecognizer(swipeUp);
	        this.AddGestureRecognizer(swipeLeft);
	        this.AddGestureRecognizer(swipeRight);
	        this.AddGestureRecognizer(tap);
        }
    }
}