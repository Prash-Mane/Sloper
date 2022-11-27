using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using Xamarin.Forms;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using System;
using System.IO;
using SloperMobile.ViewModel.MyViewModels;
using SloperMobile.ViewModel.UserViewModels;

namespace SloperMobile.ViewModel
{
    //not used?
    public class SettingViewModel : BaseViewModel
    {
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IUserDialogs userDialogs;
        private readonly IRepository<UserInfoTable> _userInfoRepository;
        private string displayName;

        public SettingViewModel(
            INavigationService navigationService,
            IRepository<CragExtended> cragRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IUserDialogs userDialogs) : base(navigationService)
        {
            this.userDialogs = userDialogs;
            this.cragRepository = cragRepository;
            _userInfoRepository = userInfoRepository;
            PageHeaderText = "SETTINGS";
            PageSubHeaderText = "User";
            DisplayName = Settings.DisplayName;
            LogOutCommand = new Command(ExecuteOnLogOut);
            EditProfileCommand = new Command(ExecuteOnEditProfile);
            ChangePasswordCommand = new Command(async () =>
            {
                await navigationService.NavigateAsync<MyChangePasswordViewModel>();
            });
        }

        public ICommand LogOutCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
        public string DisplayName
        {
            get { return displayName; }
            set { SetProperty(ref displayName, value); }
        }
        private ImageSource _userProfileImage;
        public ImageSource UserProfileImage
        {
            get { return _userProfileImage; }
            set { SetProperty(ref _userProfileImage, value); }
        }
        private async void ExecuteOnLogOut()
        {
            Settings.AccessToken = string.Empty;
            Settings.RenewalToken = string.Empty;
            Settings.DisplayName = string.Empty;
            Cache.accessToken = string.Empty;
            if (Settings.FBLogIn == true)
            {
                FacebookLogout();
                Cache.isModel = false;
            }
            if (Settings.GPLogIn == true)
            {
                Xamarin.Forms.DependencyService.Get<IGoogleSignIn>().Logout();
                Settings.GPLogIn = false;
            }
            Cache.twitterSocialData = string.Empty;
            await navigationService.ResetNavigation<UserLoginViewModel>();
        }

        private void FacebookLogout()
        {
            DependencyService.Get<IFacebookManager>().Logout();
            Settings.FBLogIn = false;
        }
        private async void ExecuteOnEditProfile()
        {
            await navigationService.NavigateAsync<MyProfileViewModel>();
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        private async void LoadProfilePic()
        {
            userDialogs.ShowLoading("Loading...", MaskType.Black);
            var response = await _userInfoRepository.GetAsync(Settings.UserID);

            if (response != null)
            {
                if (!string.IsNullOrWhiteSpace(response.ProfilePicture))
                {
                    byte[] imageBytes = null;
                    try
                    {
                        imageBytes = Convert.FromBase64String(response.ProfilePicture);
                    }
                    catch (Exception)
                    {
                        imageBytes = Convert.FromBase64String(response.ProfilePicture?.Split(',')[1]);
                    }
                    UserProfileImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                else
                    UserProfileImage = "icon_profile_large";
            }
            else
                UserProfileImage = "icon_profile_large";

            userDialogs.Loading().Hide();
        }
        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            userDialogs.HideLoading();
            LoadProfilePic();
            //IsShowFooter = GeneralHelper.IsCragsDownloaded;
        }
    }
}
