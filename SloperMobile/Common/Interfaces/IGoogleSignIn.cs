using Acr.UserDialogs;
using SloperMobile.Model;
using System;

namespace SloperMobile.Common.Interfaces
{
    public interface IGoogleSignIn
    {
        void Login(Action<GooglePlusUser, string> onLoginComplete, IUserDialogs userDialogs);
        void Logout();
    }    
}
