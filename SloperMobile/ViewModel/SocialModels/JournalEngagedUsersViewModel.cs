using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.NotifyProperties;
using SloperMobile.Model.SocialModels;
using SloperMobile.Common.Extentions;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace SloperMobile.ViewModel.SocialModels
{
    public class JournalEngagedUsersViewModel : BaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        public JournalEngagedUsersViewModel(
            INavigationService navigationService,
            IHttpHelper httpHelper,
            IUserDialogs userDialogs) : base(navigationService, httpHelper)
        {
            this.userDialogs = userDialogs;
            FollowCommand = new Command<JournalUsers>(OnFollowClick);
            ProfileCommand = new Command<JournalUsers>(GoToProfile);
		}
        #region Commands
        public Command FollowCommand { get; set; }
        public Command ProfileCommand { get; set; }

		#endregion

		#region Properties
		private ObservableCollectionFast<JournalUsers> _userlist;
        public ObservableCollectionFast<JournalUsers> UserList
        {
            get => _userlist == null ? new ObservableCollectionFast<JournalUsers>() : _userlist;
            set { SetProperty(ref _userlist, value); RaisePropertyChanged("UserList"); }
        }


        private int _journalID;
        public int JournalID
        {
            get => _journalID;
            set => SetProperty(ref _journalID, value);
        }
        private string _flag;
        public string Flag
        {
            get => _flag;
            set => SetProperty(ref _flag, value);
        }


        private bool _loadusersfromprofilepage = false;
        public bool LoadUsersFromProfilePage
        {
            get => _loadusersfromprofilepage;
            set => SetProperty(ref _loadusersfromprofilepage, value);
        }

        private string _pflag;
        public string ProfileFlag
        {
            get => _pflag;
            set => SetProperty(ref _pflag, value);
        }

        private int _userID;
        public int UserID
        {
            get => _userID;
            set => SetProperty(ref _userID, value);
        }
        #endregion

        public async Task LoadUsers()
        {
            string url;
            if (LoadUsersFromProfilePage)
            {
                if(ProfileFlag=="Follower")
                {
                    url = ApiUrls.Url_M_Social_GetFollowers(UserID, Settings.UserID);
                }
                else
                {
                    url = ApiUrls.Url_M_Social_GetFollowings(UserID, Settings.UserID);
                }
            }
            else
            {
                url = ApiUrls.Url_M_Social_GetJournalEngagement(JournalID, Settings.UserID, Flag);
            }

             var usersResponse = await httpHelper.PostAsync<ObservableCollectionFast<JournalUsers>>(url,"");
            if (usersResponse.ValidateResponse())
            {
                var result = usersResponse.Result;

                var currentUser = result.FirstOrDefault(x => x.UserID == Settings.UserID);

                if (currentUser != null)
                {
                    currentUser.IsFollowingBtnVisible = false;
                }

                UserList = result;
            }

        }

        async void OnFollowClick(JournalUsers journalusr)
        {
            if (!journalusr.IsFollowing)
            {
                FollowDTO followdto = new FollowDTO();
                followdto.FollowId = journalusr.UserID;
                var response = await httpHelper.PostAsync<RequestResult>(ApiUrls.Url_M_Social_Follow, followdto);
                if (response.ValidateResponse() && response.Result.Result == "success")
                    journalusr.IsFollowing = true;
            }
            else
            {
                FollowDTO followdto = new FollowDTO();
                followdto.FollowId = journalusr.UserID;
                var response = await httpHelper.PostAsync<RequestResult>(ApiUrls.Url_M_Social_UnFollow, followdto);
                if (response.ValidateResponse() && response.Result.Result == "success")
                    journalusr.IsFollowing = false;
            }
        }

        public void GoToProfile(JournalUsers journalusr)
        {
            userDialogs.ShowLoading("Loading...");
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(NavigationParametersConstants.MemberProfileId, journalusr.UserID);
            navigationParameters.Add(NavigationParametersConstants.MemberProfileName, journalusr.DisplayName);
            navigationService.NavigateAsync<MemberProfileViewModel>(navigationParameters);
            if (Rg.Plugins.Popup.Services.PopupNavigation.PopupStack.Count > 0)
                Rg.Plugins.Popup.Services.PopupNavigation.PopAllAsync();
        }
		#region Services 
		#endregion
		}
}
