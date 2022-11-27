using Acr.UserDialogs;
using SloperMobile.Model;
using System;

namespace SloperMobile.Common.Interfaces
{
    public interface IFacebookManager
    {
        void Login(Action<FacebookUser, string> onLoginComplete, IUserDialogs userDialogs);
        void Logout();
    }
}
