using Acr.UserDialogs;
using Facebook.LoginKit;
using Foundation;
using SloperMobile.Common.Interfaces;
using SloperMobile.Model;
using System;
using System.Threading.Tasks;
using UIKit;

namespace SloperMobile.iOS.Services
{
    public class FacebookManager : IFacebookManager
    {
        public Action<FacebookUser, string> _onLoginComplete;
        public IUserDialogs _userDialogs;

        public void Login(Action<FacebookUser, string> onLoginComplete, IUserDialogs userDialogs)
        {
            _onLoginComplete = onLoginComplete;
            _userDialogs = userDialogs;
            _userDialogs.ShowLoading("Authenticating...");
            var window = UIApplication.SharedApplication.KeyWindow;
            var vc = window.RootViewController;
            while (vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }

            var tcs = new TaskCompletionSource<FacebookUser>();
            LoginManager manager = new LoginManager();
            manager.LogOut();
            manager.LoginBehavior = LoginBehavior.SystemAccount;
            _userDialogs.HideLoading();
            manager.LogInWithReadPermissions(new string[] { "public_profile", "email" }, vc, (result, error) =>
            {
                _userDialogs.ShowLoading("Authenticating...");
                if (error != null || result == null || result.IsCancelled)
                {
                    if (error != null)
                        _onLoginComplete?.Invoke(null, error.LocalizedDescription);
                    if (result.IsCancelled)
                        _onLoginComplete?.Invoke(null, "User Cancelled!");

                    tcs.TrySetResult(null);
                }
                else
                {
                    var request = new Facebook.CoreKit.GraphRequest("me", new NSDictionary("fields", "id, first_name, email, last_name, picture.height(961)"));
                    request.Start((connection, result1, error1) =>
                    {
                        if (error1 != null || result1 == null)
                        {
                            tcs.TrySetResult(null);
                        }
                        else
                        {
                            var id = string.Empty;
                            var first_name = string.Empty;
                            var email = string.Empty;
                            var last_name = string.Empty;
                            var url = string.Empty;

                            try
                            {
                                id = result1.ValueForKey(new NSString("id"))?.ToString();
                                first_name = result1.ValueForKey(new NSString("first_name"))?.ToString();
                                email = result1.ValueForKey(new NSString("email"))?.ToString();
                                last_name = result1.ValueForKey(new NSString("last_name"))?.ToString();
                                url = ((result1.ValueForKey(new NSString("picture")) as NSDictionary).ValueForKey(new NSString("data")) as NSDictionary).ValueForKey(new NSString("url")).ToString();
                            }
                            catch (Exception e)
                            {
                            }

                            if (tcs != null)
                            {
                                tcs.TrySetResult(new FacebookUser(id, result.Token.TokenString, first_name, last_name, email, url));
                                _onLoginComplete?.Invoke(new FacebookUser(id, result.Token.TokenString, first_name, last_name, email, url), string.Empty);
                            }
                        }
                    });
                }
            });
        }

        public void Logout()
        {
            LoginManager manager = new LoginManager();
            manager.LogOut();
        }
    }
}