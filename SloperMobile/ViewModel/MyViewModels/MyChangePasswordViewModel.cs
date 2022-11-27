using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using SloperMobile.Views.FlyoutPages;
using Xamarin.Forms;

namespace SloperMobile.ViewModel
{
	public class MyChangePasswordViewModel : BaseViewModel
    {
        public MyChangePasswordViewModel(INavigationService navigationService,
                                    IExceptionSynchronizationManager exceptionManager,
                                         IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
		{
            ChangePasswordCommand = new Command(ExecuteOnChangePassword);
            ChangePasswordReq = new ChangePasswordModel();
            Offset = Common.Enumerators.Offsets.Header;
            IsBackButtonVisible = true;
            PageHeaderText = "Settings";
            PageSubHeaderText = "Change Password";
		}
		
        private ChangePasswordModel changePasswordReq;
        public ChangePasswordModel ChangePasswordReq
        {
            get { return changePasswordReq; }
            set { SetProperty(ref changePasswordReq, value); }
        }
        public ICommand ChangePasswordCommand { get; set; }

		private async void ExecuteOnChangePassword(object obj)
		{
			if (CrossConnectivity.Current.IsConnected)
			{
				var isValidate = await IsRegistrationValidation();
				if (isValidate)
				{
					UserDialogs.Instance.ShowLoading("Please Wait...");
					//httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_ChangePassword, Settings.AccessToken);
					//var changepasswordjson = JsonConvert.SerializeObject(ChangePasswordReq);
                    var response = await httpHelper.PostAsync<ChangePasswordResponseModel>(ApiUrls.Url_SloperUser_ChangePassword, ChangePasswordReq);

                    if (response.ValidateResponse())
					{
                        var result = response.Result;

						if (result.status != null && result.status == "true")
						{
							UserDialogs.Instance.Loading().Hide();
							await Application.Current.MainPage.DisplayAlert(result.message, result.data, "OK");
							DisposeObject();
							await PageNavigation(Common.Enumerators.ApplicationActivity.MyProfilePage);
							UserDialogs.Instance.HideLoading();
						}
						else if (result.status != null && result.status == "false")
						{
							UserDialogs.Instance.HideLoading();
							await Application.Current.MainPage.DisplayAlert("Error", result.data, "OK"); ;
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
				}
			}
			else
			{
				//await navigationService.NavigateAsync<NetworkErrorViewModel>();
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.ExecuteOnChangePassword),
					Page = this.GetType().Name,
					StackTrace = JsonConvert.SerializeObject(MasterNavigationPage.Instance?.Navigation.NavigationStack),
					Exception = "Network error page"
				});
			}
		}

        private async Task<bool> IsRegistrationValidation()
        {
            if (string.IsNullOrWhiteSpace(ChangePasswordReq.currentpassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Current Password required, try again.", "OK");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(ChangePasswordReq.newpassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "New Password required, try again.", "OK");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(ChangePasswordReq.confirmpassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Confirm Password required, try again.", "OK");
                return false;
            }
            else if (!ChangePasswordReq.newpassword.Equals(ChangePasswordReq.confirmpassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "The new passwords do not match, please try again.", "OK");
                return false;
            }
            return true;
        }

        private void DisposeObject()
        {
            ChangePasswordReq = new ChangePasswordModel();
        }
    }
}
