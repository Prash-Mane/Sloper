using System;
using Android.Content;
using Android.Views;
using SloperMobile.CustomControls;
using SloperMobile.Droid.Listeners;
using SloperMobile.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SwipeGestureFrame), typeof(GestureFrameRenderer))]
namespace SloperMobile.Droid.Renderers
{
	public class GestureFrameRenderer : FrameRenderer
    {
        private readonly CustomGestureListener listener;
        private readonly GestureDetector detector;
	    private SwipeGestureFrame frameElement;

        public GestureFrameRenderer(Context context) : base(context)
        {
            listener = new CustomGestureListener();
            detector = new GestureDetector(listener);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                this.GenericMotion -= HandleGenericMotion;
                this.Touch -= HandleTouch;
	            listener.OnTap -= HandleOnTap;
				listener.OnSwipeLeft -= HandleOnSwipeLeft;
                listener.OnSwipeRight -= HandleOnSwipeRight;
                listener.OnSwipeTop -= HandleOnSwipeTop;
                listener.OnSwipeDown -= HandleOnSwipeDown;
            }

	        if (e.OldElement == null)
	        {
		        this.GenericMotion += HandleGenericMotion;
		        this.Touch += HandleTouch;
		        listener.OnTap += HandleOnTap;
				listener.OnSwipeLeft += HandleOnSwipeLeft;
		        listener.OnSwipeRight += HandleOnSwipeRight;
		        listener.OnSwipeTop += HandleOnSwipeTop;
		        listener.OnSwipeDown += HandleOnSwipeDown;
	        }

	        frameElement = (SwipeGestureFrame) this.Element;
        }
	    void HandleTouch(object sender, TouchEventArgs e)
        {
            detector.OnTouchEvent(e.Event);
        }

        void HandleGenericMotion(object sender, GenericMotionEventArgs e)
        {
            detector.OnTouchEvent(e.Event);
        }

	    void HandleOnTap(object sender, EventArgs e)
	    {
		    frameElement.TapCommand?.Execute(frameElement.BindingContext);
	    }

	    void HandleOnSwipeLeft(object sender, EventArgs e)
        {
	        frameElement.SwipeLeftCommand?.Execute(frameElement.BindingContext);
		}

        void HandleOnSwipeRight(object sender, EventArgs e)
        {
	        frameElement.SwipeRightCommand?.Execute(frameElement.BindingContext);
        }

        void HandleOnSwipeTop(object sender, EventArgs e)
        {
	        frameElement.SwipeUpCommand?.Execute(frameElement.BindingContext);
        }

        void HandleOnSwipeDown(object sender, EventArgs e)
        {
	        frameElement.SwipeDownCommand?.Execute(frameElement.BindingContext);
        }
    }
}