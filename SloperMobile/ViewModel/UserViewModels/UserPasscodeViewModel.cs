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
using Xamarin.Forms;

namespace SloperMobile.ViewModel.UserViewModels
{
    public class UserPasscodeViewModel : BaseViewModel
    {
        private string userEmail;

        public UserPasscodeViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {
            PasscodeCommand = new Command(OnPasscodeAsync);
        }

        private string _pin1;
        public string Pin1
        {
            get { return _pin1; }
            set
            {
                _pin1 = value; RaisePropertyChanged();
            }
        }

        private string _pin2;
        public string Pin2
        {
            get { return _pin2; }
            set
            {
                _pin2 = value; RaisePropertyChanged();
            }
        }

        private string _pin3;
        public string Pin3
        {
            get { return _pin3; }
            set
            {
                _pin3 = value; RaisePropertyChanged();
            }
        }

        private string _pin4;
        public string Pin4
        {
            get { return _pin4; }
            set
            {
                _pin4 = value; RaisePropertyChanged();
            }
        }

        public ICommand NavigateToLoginCommand
        {
            get
            {
                return new Command(async () => await navigationService.ResetNavigation<UserLoginViewModel>());
            }
        }

        public ICommand PasscodeCommand { get; set; }

        private async void OnPasscodeAsync(object parma)
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    UserDialogs.Instance.ShowLoading("Please Wait...");
                    if (string.IsNullOrWhiteSpace(Pin1) || string.IsNullOrWhiteSpace(Pin2) || string.IsNullOrWhiteSpace(Pin3) || string.IsNullOrWhiteSpace(Pin4))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Security Code can't be empty, please try again.", "OK");
                        UserDialogs.Instance.Loading().Hide();
                        return;
                    }

                    UserDialogs.Instance.ShowLoading("Please Wait...");
                    
                    string token = Pin1 + Pin2 + Pin3 + Pin4;
                   // httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_SloperUser_ResetPasswordVerification, userEmail, token), String.Empty);
                    //var response = await httpClinetHelper.GetResponse<string>();

                    var response = await httpHelper.GetAsync<string>(ApiUrls.Url_SloperUser_ResetPasswordVerification(userEmail, token));

                    if (response.ValidateResponse())
                    {
                        if (response.Result.Contains("success"))
                        {
                            UserDialogs.Instance.Loading().Hide();
                            var navigationParameters = new NavigationParameters();
                            navigationParameters.Add("userEmail", userEmail);
                            navigationParameters.Add("token", token);
                            await navigationService.NavigateAsync<UserChangePasswordViewModel>(navigationParameters);
                        }
                        else
                        {
                            Pin1 = String.Empty;
                            Pin2 = String.Empty;
                            Pin3 = String.Empty;
                            Pin4 = String.Empty;
                            UserDialogs.Instance.Loading().Hide();
                            await Application.Current.MainPage.DisplayAlert("Error", response.Result, "OK");
                            return;
                        }
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
                    Method = nameof(this.OnPasscodeAsync),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(parma)
                });
            }
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (parameters.Count == 0)
            {
                return;
            }
                
            if (userEmail == null)
            {
                userEmail = (string)parameters["userEmail"];
            }
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }
    }
}