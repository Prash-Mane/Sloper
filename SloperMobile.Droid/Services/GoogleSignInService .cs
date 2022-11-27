using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SloperMobile.Droid.Services;
using SloperMobile.Common.Interfaces;
using Android.Gms.Common.Apis;
using Xamarin.Forms;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Auth.Api;
using Android.Support.V4.Content;
using Xamarin.Forms.Platform.Android;
using Android;
using Android.Support.V4.App;
using SloperMobile.Views;
using SloperMobile.Model;
using Android.Gms.Common;
using SloperMobile.Common.Constants;
using Android.Gms.Auth;
using Android.Accounts;
using Acr.UserDialogs;

[assembly: Dependency(typeof(GoogleSignInService))]
namespace SloperMobile.Droid.Services
{
    public class GoogleSignInService : PageRenderer, IGoogleSignIn
    {
        public Action<GooglePlusUser, string> _onGoogleLoginComplete;
        public static GoogleApiClient GoogleApiClient;
        public IUserDialogs _userDialogs;
        public static GoogleSignInService googleService { get; private set; }

        public GoogleSignInService()
        {
            googleService = this;
        }

        public void Login(Action<GooglePlusUser, string> onGoogleLoginComplete, IUserDialogs userDialogs)
        {
            try
            {               
                var activity = this.Context as Activity;
                _userDialogs = userDialogs;
                _userDialogs.ShowLoading("Authenticating...");
                _onGoogleLoginComplete = onGoogleLoginComplete;
                Logout();
                Intent signInIntent = Auth.GoogleSignInApi.GetSignInIntent(GoogleApiClient);
                ((MainActivity)Forms.Context).StartActivityForResult(signInIntent, 1);
                GoogleApiClient.Connect();                   
               
            }
            catch (Exception ex)
            {
            }
        }
        public void OnAuthCompleted(GoogleSignInResult result)
        {
            string gender = string.Empty;
            if (result.IsSuccess)
            {
                GoogleSignInAccount accountt = result.SignInAccount;

                _onGoogleLoginComplete?.Invoke(new GooglePlusUser
                        (accountt.Id, accountt.GivenName, accountt.FamilyName, accountt.Email, (accountt.PhotoUrl != null ? $"{accountt.PhotoUrl}" : $""), gender, accountt.DisplayName), string.Empty); //https://autisticdating.net/imgs/profile-placeholder.jpg
            }
            else
            {
                _onGoogleLoginComplete?.Invoke(null, "An error occured!");
            }
        }
        public void Logout()
        {            
            if (GoogleApiClient != null &&
                GoogleApiClient.IsConnected)
            {
                Auth.GoogleSignInApi.SignOut(GoogleApiClient);
                GoogleApiClient.Disconnect();
            }
        }       
    }
}