using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.CustomControls
{
	public class SwipeGestureFrame : Frame
	{
		public static readonly BindableProperty SwipeUpCommandProperty = BindableProperty.Create<SwipeGestureFrame, ICommand>(p => p.SwipeUpCommand, null);

		public ICommand SwipeUpCommand
		{
			get { return (ICommand)GetValue(SwipeUpCommandProperty); }
			set { SetValue(SwipeUpCommandProperty, value); }
		}

		public static readonly BindableProperty SwipeDownCommandProperty = BindableProperty.Create<SwipeGestureFrame, ICommand>(p => p.SwipeDownCommand, null);

		public ICommand SwipeDownCommand
		{
			get { return (ICommand)GetValue(SwipeDownCommandProperty); }
			set { SetValue(SwipeDownCommandProperty, value); }
		}

		public static readonly BindableProperty SwipeRightCommandProperty = BindableProperty.Create<SwipeGestureFrame, ICommand>(p => p.SwipeRightCommand, null);

		public ICommand SwipeRightCommand
		{
			get { return (ICommand)GetValue(SwipeRightCommandProperty); }
			set { SetValue(SwipeRightCommandProperty, value); }
		}

		public static readonly BindableProperty SwipeLeftCommandProperty = BindableProperty.Create<SwipeGestureFrame, ICommand>(p => p.SwipeLeftCommand, null);

		public ICommand SwipeLeftCommand
		{
			get { return (ICommand)GetValue(SwipeLeftCommandProperty); }
			set { SetValue(SwipeLeftCommandProperty, value); }
		}

	    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create<SwipeGestureFrame, ICommand>(p => p.SwipeRightCommand, null);

	    public ICommand TapCommand
		{
		    get { return (ICommand)GetValue(TapCommandProperty); }
		    set { SetValue(TapCommandProperty, value); }
	    }


		public event EventHandler Tapped;

	    protected void OnTapped(EventArgs e)
	    {
		    TapCommand?.Execute(this.BindingContext);
		    Tapped?.Invoke(this, e);
	    }

	    public event EventHandler SwipeUP;

        protected void OnSwipeUP(EventArgs e)
        {
			SwipeUpCommand?.Execute(this.BindingContext);
			SwipeUP?.Invoke(this, e);
		}

        public event EventHandler SwipeDown;

        protected void OnSwipeDown(EventArgs e)
		{
			SwipeDownCommand?.Execute(this.BindingContext);
			SwipeDown?.Invoke(this, e);
		}

        public event EventHandler SwipeRight;

        protected void OnSwipeRight(EventArgs e)
		{
			SwipeRightCommand?.Execute(this.BindingContext);
			SwipeRight?.Invoke(this, e);
		}

        public event EventHandler SwipeLeft;

        protected void OnSwipeLeft(EventArgs e)
		{
			SwipeLeftCommand?.Execute(this.BindingContext);
			SwipeLeft?.Invoke(this, e);
		}
    }
}