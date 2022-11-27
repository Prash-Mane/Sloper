using System;
using Android.Views;

namespace SloperMobile.Droid.Listeners
{
    public class CustomGestureListener : GestureDetector.SimpleOnGestureListener
    {
        private static int SWIPE_THRESHOLD = 100;
        private static int SWIPE_VELOCITY_THRESHOLD = 100;

        public event EventHandler OnSwipeDown;
        public event EventHandler OnSwipeTop;
        public event EventHandler OnSwipeLeft;
        public event EventHandler OnSwipeRight;
	    public event EventHandler OnTap;

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            Console.WriteLine("OnFling");

            var diffY = e2.GetY() - e1.GetY();
            var diffX = e2.GetX() - e1.GetX();
            if (Math.Abs(diffX) > Math.Abs(diffY))
            {
	            if (Math.Abs(diffX) > SWIPE_THRESHOLD && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
	            {
		            if (diffX > 0)
		            {
			            OnSwipeRight?.Invoke(this, null);
		            }
		            else
		            {
			            OnSwipeLeft?.Invoke(this, null);
		            }
	            }
            }
            else if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
	        {
		        if (diffY > 0)
		        {
			        OnSwipeDown?.Invoke(this, null);
		        }
		        else
		        {
			        OnSwipeTop?.Invoke(this, null);
		        }
	        }

	        return base.OnFling(e1, e2, velocityX, velocityY);
        }

	    public override bool OnSingleTapUp(MotionEvent e)
	    {
		    OnTap?.Invoke(this, null);
			return base.OnSingleTapUp(e);
	    }
    }
}