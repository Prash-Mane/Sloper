using System;
using Foundation;
using UIKit;
using CoreGraphics;
using Tools;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using SloperMobile.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(KeyboardHandledPageRenderer))]
namespace SloperMobile.iOS
{
    public class KeyboardHandledPageRenderer : ContentPageExRenderer
    {
        NSObject keyboardShowObserver;
        NSObject keyboardHideObserver;

        /// <summary>
        /// Call this method from constructor, ViewDidLoad or ViewWillAppear to enable keyboard handling in the main partial class
        /// </summary>
        internal void InitKeyboardHandling()
        {
            //Only do this if required
            if (HandlesKeyboardNotifications())
            {
                //RegisterForKeyboardNotifications();
                SetDoneToolbars();
            }
        }

        /// <summary>
        /// Set this field to any view inside the textfield to center this view instead of the current responder
        /// </summary>
        protected UIView ViewToCenterOnKeyboardShown;
        protected UIScrollView ScrollToCenterOnKeyboardShown;

        /// <summary>
        /// Override point for subclasses, return true if you want to handle keyboard notifications
        /// to center the active responder in the scroll above the keyboard when it appears
        /// </summary>
        public virtual bool HandlesKeyboardNotifications()
        {
            return true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitKeyboardHandling();
            DismissKeyboardOnBackgroundTap();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            RegisterForKeyboardNotifications();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UnregisterForKeyboardNotifications();
        }

        protected virtual void RegisterForKeyboardNotifications()
        {
            if (keyboardShowObserver == null)
                keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
            if (keyboardHideObserver == null)
                keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
        }

        public virtual void UnregisterForKeyboardNotifications()
        {
            if (keyboardShowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardShowObserver);
                keyboardShowObserver.Dispose();
                keyboardShowObserver = null;
            }

            if (keyboardHideObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardHideObserver);
                keyboardHideObserver.Dispose();
                keyboardHideObserver = null;
            }
        }

        /// <summary>
        /// Gets the UIView that represents the "active" user input control (e.g. textfield, or button under a text field)
        /// </summary>
        /// <returns>
        /// A <see cref="UIView"/>
        /// </returns>
        protected virtual UIView KeyboardGetActiveView()
        {
            return View.FindFirstResponder();
        }

        private void OnKeyboardNotification(NSNotification notification)
        {
            try
            {
                if (!IsViewLoaded)
                    return;

                var activeView = ViewToCenterOnKeyboardShown ?? KeyboardGetActiveView();
                if (activeView == null)
                    return;

                var scrollView = ScrollToCenterOnKeyboardShown ??
                    activeView.FindTopSuperviewOfType(View, typeof(UIScrollView)) as UIScrollView;
                if (scrollView == null)
                {
                    MoveInputView(notification);
                    return;
                }

                //Check if the keyboard is becoming visible
                var visible = notification.Name == UIKeyboard.WillShowNotification;

                //Start an animation, using values from the keyboard
                UIView.BeginAnimations("AnimateForKeyboard");
                UIView.SetAnimationBeginsFromCurrentState(true);
                UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
                UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

                //Pass the notification, calculating keyboard height, etc.
                var keyboardFrame = visible
                    ? UIKeyboard.FrameEndFromNotification(notification)
                    : UIKeyboard.FrameBeginFromNotification(notification);

                if (!visible)
                    scrollView.RestoreScrollPosition();
                else
                    scrollView.CenterView(activeView, keyboardFrame);

                //Commit the animation
                UIView.CommitAnimations();
            }
            catch { }
        }

        /// <summary>
        /// Override this method to apply custom logic when the keyboard is shown/hidden
        /// </summary>
        /// <param name='visible'>
        /// If the keyboard is visible
        /// </param>
        /// <param name='keyboardFrame'>
        /// Frame of the keyboard
        /// </param>
        protected virtual void OnKeyboardChanged(bool visible, CGRect keyboardFrame)
        {
            var activeView = ViewToCenterOnKeyboardShown ?? KeyboardGetActiveView();
            if (activeView == null)
                return;

            var scrollView = ScrollToCenterOnKeyboardShown ??
                activeView.FindTopSuperviewOfType(View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;

            if (!visible)
                scrollView.RestoreScrollPosition();
            else
                scrollView.CenterView(activeView, keyboardFrame);
        }

        /// <summary>
        /// Call it to force dismiss keyboard when background is tapped
        /// </summary>
        protected void DismissKeyboardOnBackgroundTap()
        {
            // Add gesture recognizer to hide keyboard
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            //tap.ShouldReceiveTouch = (recognizer, touch) =>
                //!(touch.View is UIControl || touch.View.FindSuperviewOfType(View, typeof(UITableViewCell)) != null);
            View.AddGestureRecognizer(tap);
        }

        void MoveInputView(NSNotification notification) 
        { 
            var frameBegin = UIKeyboard.FrameBeginFromNotification(notification);
            var frameEnd = UIKeyboard.FrameEndFromNotification(notification);

            var page = Element as ContentPage;
            if (page != null && !(page.Content is ScrollView))
            {
                var padding = page.Padding;
                page.Padding = new Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom + frameBegin.Top - frameEnd.Top);
            }
        }

        void SetDoneToolbars()
        {
            RecursiveHelperFunc(View);
        }

        void RecursiveHelperFunc(UIView parent)
        {
            foreach (var subview in parent.Subviews)
            {
                if (subview.Subviews != null && subview.Subviews.Length > 0)
                {
                    RecursiveHelperFunc(subview);
                }

                var txtField = subview as UITextField;
                if (txtField != null
                    && (txtField.KeyboardType == UIKeyboardType.NumberPad
                        || txtField.KeyboardType == UIKeyboardType.NumbersAndPunctuation
                        || txtField.KeyboardType == UIKeyboardType.DecimalPad
                        || txtField.KeyboardType == UIKeyboardType.PhonePad))
                {
                    AddDoneToolbar(txtField);
                    continue;
                }

                var txtView = subview as UITextView;
                if (txtView != null)
                    AddDoneToolbar(txtView);
            }
        }

        void AddDoneToolbar(UITextField txtField)
        {
            var toolbar = new UIToolbar()
            {
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                BarTintColor = UIColor.GroupTableViewBackgroundColor
            };
            toolbar.SizeToFit();
            var flexBtn = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneBtn = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, e) => txtField.EndEditing(true));
            toolbar.Items = new[] { flexBtn, doneBtn };
            txtField.InputAccessoryView = toolbar;
        }

        void AddDoneToolbar(UITextView txtField)
        {
            var toolbar = new UIToolbar()
            {
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                BarTintColor = UIColor.GroupTableViewBackgroundColor
            };
            toolbar.SizeToFit();
            var flexBtn = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            var doneBtn = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, e) => txtField.EndEditing(true));
            toolbar.Items = new[] { flexBtn, doneBtn };
            txtField.InputAccessoryView = toolbar;
        }
    }
}
