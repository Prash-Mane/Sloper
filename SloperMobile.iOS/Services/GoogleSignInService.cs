using Acr.UserDialogs;
using Foundation;
using Google.SignIn;
using SloperMobile.Common.Interfaces;
using SloperMobile.Model;
using System;
using UIKit;

namespace SloperMobile.iOS.Services
{
    public class GoogleSignInService : NSObject, IGoogleSignIn, ISignInDelegate, ISignInUIDelegate 
    {        
        public Action<GooglePlusUser, string> _onGoogleLoginComplete;
        public IUserDialogs _userDialogs;

        UIViewController parent;

        public GoogleSignInService()
        {
            SignIn.SharedInstance.UIDelegate = this;
            SignIn.SharedInstance.Delegate = this;
        }
        public void Login(Action<GooglePlusUser, string> onGoogleLoginComplete, IUserDialogs userDialogs)
        {
            _onGoogleLoginComplete = onGoogleLoginComplete;
            _userDialogs = userDialogs;
            _userDialogs.ShowLoading("Authenticating...");
            Logout();
            var window = UIApplication.SharedApplication.KeyWindow;
            parent = window.RootViewController;
            while (parent.PresentedViewController != null)
            {
                parent = parent.PresentedViewController;
            }
            SignIn.SharedInstance.SignInUser();
            _userDialogs.HideLoading();
        }

        public void Logout()
        {
            SignIn.SharedInstance.SignOutUser();
        }       
        public void DidSignIn(SignIn signIn, Google.SignIn.GoogleUser user, NSError error)
        {
            _userDialogs.ShowLoading("Authenticating...");
            if (user != null && error == null)
            {
                var picture = user.Profile.HasImage ? user.Profile.GetImageUrl(1000).ToString() : "";
                _onGoogleLoginComplete?.Invoke(new GooglePlusUser
                                               (user.UserID, user.Profile.GivenName, user.Profile.FamilyName, user.Profile.Email, picture, "", user.Profile.Name), string.Empty);
            }           
            else
                _onGoogleLoginComplete?.Invoke(null, error.LocalizedDescription);
        }

        [Export("signIn:didDisconnectWithUser:withError:")]
        public void DidDisconnect(SignIn signIn, GoogleUser user, NSError error)
        {
            // Perform any operations when the user disconnects from app here.
        }

        [Export("signInWillDispatch:error:")]
        public void WillDispatch(SignIn signIn, NSError error)
        {
            //myActivityIndicator.StopAnimating();
        }

        [Export("signIn:presentViewController:")]
        public void PresentViewController(SignIn signIn, UIViewController viewController)
        {
            parent.PresentViewController(viewController, true, null);
        }

        [Export("signIn:dismissViewController:")]
        public void DismissViewController(SignIn signIn, UIViewController viewController)
        {
            viewController.DismissViewController(true, null);
        }
    }

    public class GoogleSignInUIDelegate : SignInUIDelegate
    {
        public override void WillDispatch(SignIn signIn, NSError error)
        {
        }
        public override void PresentViewController(SignIn signIn, UIViewController viewController)
        {
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(viewController, true, null);
        }

        public override void DismissViewController(SignIn signIn, UIViewController viewController)
        {
            UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController(true, null);
        }
    }
}
