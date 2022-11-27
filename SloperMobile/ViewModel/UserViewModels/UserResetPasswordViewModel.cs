using System;
using System.Windows.Input;
using Acr.UserDialogs;
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
	public class UserResetPasswordViewModel : BaseViewModel 
    {
        public UserResetPasswordViewModel(
			INavigationService navigationService,
			IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
		{
            ResetPasswordCommand = new Command(OnResetPasswordAsync);
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { _email = value;RaisePropertyChanged(); }
        }

		public ICommand ResetPasswordCommand { get; set; }

		public ICommand NavigateToLoginCommand
		{
			get
			{
				return new Command(async () => await navigationService.NavigateAsync<UserLoginViewModel>());
			}
		}

        private async void OnResetPasswordAsync(object parma)
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    UserDialogs.Instance.ShowLoading("Please Wait...");
                    if (string.IsNullOrWhiteSpace(Email))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Email can't be empty, please try again.", "OK");
                        UserDialogs.Instance.Loading().Hide();
                        return;
                    }

                    var response = await    httpHelper.GetAsync<ResetPasswordModel>(ApiUrls.Url_SloperUser_ResetPassword(Email));
                    if (response.ValidateResponse())
                    {
                        var result = response.Result;

                        if(result.message.Equals("Message Sent"))
                        {
                            UserDialogs.Instance.Loading().Hide();
                            await Application.Current.MainPage.DisplayAlert(result.message, result.data, "OK");

                            var navigationParameters = new NavigationParameters();
                            navigationParameters.Add("userEmail", Email);
                            await navigationService.NavigateAsync<UserPasscodeViewModel>(navigationParameters);
						}
                        else
                        {
                            Email = String.Empty;
                            UserDialogs.Instance.Loading().Hide();
                            await Application.Current.MainPage.DisplayAlert(result.message, result.data, "OK");
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
					Method = nameof(this.OnResetPasswordAsync),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(parma)
				});
			}
        }
    }
}
