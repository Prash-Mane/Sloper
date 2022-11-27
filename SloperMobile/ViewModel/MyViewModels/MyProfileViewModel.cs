using System;
using System.IO;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.ViewModel.UserViewModels;
using Xamarin.Forms;
using SloperMobile.ViewModel.SocialModels;

namespace SloperMobile.ViewModel.MyViewModels
{
	public class MyProfileViewModel : BaseViewModel
    {
        private string _displayname;						  
        private readonly IUserDialogs userDialogs;
	    private ImageSource _userProfileImage;
	    private readonly IRepository<UserInfoTable> userInfoRepository;
		private string displayName;
        private bool isPageLoad = false;

        public MyProfileViewModel(
            INavigationService navigationService,
			IRepository<UserInfoTable> userInfoRepository,
			IUserDialogs userDialogs) : base(navigationService)
        {												   
            this.userDialogs = userDialogs;
            this.userInfoRepository = userInfoRepository;
            OnPagePreparing();
            PageHeaderText = "SETTINGS";
            PageSubHeaderText = "User";
            isPageLoad = true;
            //IsShowFooter = false;
            LogOutCommand = new Command(ExecuteOnLogOut);
            EditProfileCommand = new Command(ExecuteOnEditProfile);
            ChangePasswordCommand = new Command(async () =>
            {
                if (GuestHelper.CheckGuest())
                    return;

                await navigationService.NavigateAsync<MyChangePasswordViewModel>();
            });
            IsShowFooter = true;
            Offset = Common.Enumerators.Offsets.Header;
		}

		public string DisplayName
		{
			get { return _displayname; }
			set { _displayname = value; RaisePropertyChanged(); }
		}

	    public ImageSource UserProfileImage
        {
            get { return _userProfileImage; }
            set { SetProperty(ref _userProfileImage, value); }
        }

        public bool IsButtonVisible => !(Settings.FBLogIn || Settings.GPLogIn);

        public ICommand LogOutCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }
        public ICommand EditProfileCommand { get; set; }
        public ICommand AvatarCommand { get => new Command(OnAvatarTapped); }

        private async void OnPagePreparing()
        {
            PageHeaderText = "SETTINGS";
            PageSubHeaderText = "User";
            //DisplayName = Settings.DisplayNameSettings;
            //LoadProfilePic();
        }
        private async void ExecuteOnLogOut()
        {
            await GeneralHelper.LogOut();
        }

        private async void ExecuteOnEditProfile()
        {
            if (GuestHelper.CheckGuest())
                return;

            isPageLoad = false;
            await navigationService.NavigateAsync<MyPreferencesViewModel>();
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        private async void LoadProfilePic()
        {
//            userDialogs.ShowLoading("Loading...", MaskType.Black);
            var response = await userInfoRepository.GetAsync(Convert.ToInt32(Settings.UserID));
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
            UserDialogs.Instance.HideLoading();
        }

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            //           userDialogs.ShowLoading("Loading...",MaskType.Black);
            //reassign display name after edit profile
            if (!isPageLoad)
            DisplayName = Settings.DisplayName;
            
            //update display name
            var response = await userInfoRepository.GetAsync(Convert.ToInt32(Settings.UserID));
            if (response != null)
            {
                DisplayName = response.DisplayName != Settings.DisplayName ? response.DisplayName : Settings.DisplayName;
            }

            LoadProfilePic();

	        App.IsNavigating = false;
		}

        void OnAvatarTapped()
        {
            if (!IsShowFooter)
                return;

            var parameters = new NavigationParameters();
            parameters.Add(NavigationParametersConstants.MemberProfileId, Settings.UserID);
            parameters.Add(NavigationParametersConstants.MemberProfileName, DisplayName);

            navigationService.NavigateAsync<MemberProfileViewModel>(parameters);
        }
    }
}
