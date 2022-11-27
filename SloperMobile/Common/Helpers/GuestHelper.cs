using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase.DataTables;
using SloperMobile.ViewModel.UserViewModels;
using Rg.Plugins.Popup.Services;
using System.Linq;

namespace SloperMobile
{
    public static class GuestHelper
    {
        static bool isAlertVisible;

        public static bool IsGuest => Settings.AccessToken == "Guest"; //Settings.UserID == -1;

        public static bool CheckGuest()
        {
            if (!IsGuest)
                return false;

            ShowGuestAlertAsync();
            return true;
        }

        static async Task ShowGuestAlertAsync()
        {
            if (isAlertVisible)
                return;

            isAlertVisible = true;
            if (await UserDialogs.Instance.ConfirmAsync("Available only for registered users. Please Login or Register.", okText: "Login/Register", cancelText: "Cancel"))
            {
                if (PopupNavigation.Instance.PopupStack.Any())
                    await PopupNavigation.Instance.PopAllAsync();
                await App.Navigation.ResetNavigation<UserLoginViewModel>();
            }
            isAlertVisible = false;
        }

        public static UserInfoTable GetGuestUser()
        {
            return new UserInfoTable
            {
                UserID = -1,
                DisplayName = "Guest",
                Email = "guest@sloperclimbing.com",
                FirstName = "Guest",
                LastName = "User",
                NumberOfFreeCrags = 0
            };
        }
    }
}
