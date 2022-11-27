using System;
using System.Windows.Input;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.UserViewModels
{
    public class UserChangePasswordViewModel : BaseViewModel
    {
        private string userEmail;
        private string token;

        public UserChangePasswordViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {
            ChangePasswordCommand = new Command(OnChangePasswordAsync);
        }

        public ICommand ChangePasswordCommand { get; set; }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; RaisePropertyChanged(); }
        }

        private string _confirm;
        public string Confirm
        {
            get { return _confirm; }
            set { _confirm = value; RaisePropertyChanged(); }
        }

        public ICommand NavigateToLoginCommand
        {
            get
            {
                return new Command(async () => await navigationService.ResetNavigation<UserLoginViewModel>());
            }
        }

        private async void OnChangePasswordAsync(object parma)
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    UserDialogs.Instance.ShowLoading("Please Wait...");
                    if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Confirm))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Password or Confirm Password can't be empty, please try again.", "OK");
                        UserDialogs.Instance.Loading().Hide();
                        return;
                    }

                    if (Password != Confirm)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "New Password and Confirm Password do not match. Please try again.", "OK");
                        UserDialogs.Instance.Loading().Hide();
                        return;
                    }

                    var changePasswordModel = new
                    {
                        email = userEmail,
                        token = token,
                        password = Password,
                        confirm = Confirm
                    };

                    var response = await httpHelper.PostAsync<ChangePasswordResponseModel>(ApiUrls.Url_SloperUser_UpdatePassword, changePasswordModel);

                    if (response.ValidateResponse())
                    {
                        var result = response.Result;

                        if (result.status != null && result.status == "true")
                        {
                            UserDialogs.Instance.Loading().Hide();
                            await Application.Current.MainPage.DisplayAlert(result.message, result.data, "OK");
                            DisposeObject();
                            await navigationService.ResetNavigation<UserLoginViewModel>();
                            UserDialogs.Instance.HideLoading();
                        }
                        else if (result.status != null && result.status == "false")
                        {
                            UserDialogs.Instance.HideLoading();
                            await Application.Current.MainPage.DisplayAlert("Error", result.data, "OK");
                        }
                        else
                        {
                            UserDialogs.Instance.HideLoading();
                            await Application.Current.MainPage.DisplayAlert("Error", AppConstant.CHANGEPASSWORD_FAILURE, "OK");
                        }
                    }
                    else
                    {
                        UserDialogs.Instance.HideLoading();
                        await Application.Current.MainPage.DisplayAlert("Change Password", AppConstant.CHANGEPASSWORD_FAILURE, "OK");
                        return;
                    }
                    UserDialogs.Instance.Loading().Hide();
                }
                else
                {
                    UserDialogs.Instance.Loading().Hide();
                    await Application.Current.MainPage.DisplayAlert("Connection Error", "Internet Connection Lost!!", "OK");
                }
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnChangePasswordAsync),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(parma)
                });
            }
        }

        private void DisposeObject()
        {
            Password = null;
            Confirm = null;
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (parameters.Count == 0)
            {
                return;
            }

            if (userEmail == null && token == null)
            {
                userEmail = (string)parameters["userEmail"];
                token = (string)parameters["token"];
            }
        }
    }
}