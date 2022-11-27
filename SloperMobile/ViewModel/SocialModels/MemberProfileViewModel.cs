using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.SocialModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using SloperMobile.Common.Extentions;
using SloperMobile.ViewModel.MyViewModels;
using System.Collections.Generic;
using SloperMobile.Views.SocialPages;
using Rg.Plugins.Popup.Services;

namespace SloperMobile.ViewModel.SocialModels
{
    public class MemberProfileViewModel : BaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<ProfileFilterTable> profileFilterTable;

        MemberProfile profile;
        int userId;
        bool isLoaded;


        public MemberProfileViewModel(
            INavigationService navigationService,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<ProfileFilterTable> profileFilterTable,
            IHttpHelper httpHelper,
            IUserDialogs userDialogs) : base(navigationService, httpHelper)
        {
            this.userInfoRepository = userInfoRepository;
            this.profileFilterTable = profileFilterTable;
            this.userDialogs = userDialogs;
            FollowCommand = new Command(OnFollowClick);
            EditProfileCommand = new Command(GoEditProfile);

            FollowersCommand = new Command(LoadFollowers);
            FollowingsCommand = new Command(LoadFollowings);

            PageHeaderText = "PROFILE";
            HasFade = true;
            IsShowFooter = true;
            Offset = Offsets.Footer;
        }

        #region Commands
        public Command FollowCommand { get; set; }
        public Command<string> ActivityCommand { get => new Command<string>(GoToActivity); }
        public Command EditProfileCommand { get; set; }
        public Command FollowersCommand { get; set; }
        public Command FollowingsCommand { get; set; }
        #endregion

        bool IsFollowing
        {
            get => profile != null && profile.FollowingStatus > 0;
        }

        #region Properties
        private int followercount;
        public int FollowerCount
        {
            get => followercount;
            set => SetProperty(ref followercount, value);
        }
        private int followingcount;
        public int FollowingCount
        {
            get => followingcount;
            set => SetProperty(ref followingcount, value);
        }

        public string ProfileImageUrl
        {
            get => profile == null ? (GuestHelper.IsGuest ? "default_sloper_outdoor_square" : "") : ApiUrls.Url_M_ProfileAvatarBig(profile.MemberId, profile.ProfileProperties.profile_update_date);
        }

        private string ageGroup = "N/A";
        public string AgeGroup
        {
            get => ageGroup;
            set => SetProperty(ref ageGroup, value);
        }

        private string heightGroup = "N/A";
        public string HeightGroup
        {
            get => heightGroup;
            set => SetProperty(ref heightGroup, value);
        }

        private string weightGroup = "N/A";
        public string WeightGroup
        {
            get => weightGroup;
            set => SetProperty(ref weightGroup, value);
        }

        private string yearsClimbingGroup = "N/A";
        public string YearsClimbGroup
        {
            get => yearsClimbingGroup;
            set => SetProperty(ref yearsClimbingGroup, value);
        }

        public string FollowText
        {
            get => IsFollowing ? "✓ Following" : "+ Follow";
        }

        public Color FollowTxtColor
        {
            get => IsFollowing ? Color.White : Color.FromHex("#FF8E2D");
        }

        public Color FollowBGColor
        {
            get => IsFollowing ? Color.FromHex("#FF8E2D") : Color.White;
        }

        public Color FollowersBG
        {
            get => Color.Black.MultiplyAlpha(0.5);
        }

        private string flash = "N/A";
        public string Flash
        {
            get => flash;
            set => SetProperty(ref flash, value);
        }

        private double flashPercent;
        public double FlashPercent
        {
            get => flashPercent;
            set => SetProperty(ref flashPercent, value);
        }

        private string onsight = "N/A";
        public string Onsight
        {
            get { return onsight; }
            set { SetProperty(ref onsight, value); }
        }

        private double onsightPercent;
        public double OnsightPercent
        {
            get => onsightPercent;
            set => SetProperty(ref onsightPercent, value);
        }

        private string redpoint = "N/A";
        public string RedPoint
        {
            get => redpoint;
            set => SetProperty(ref redpoint, value);
        }

        private double redpointPercent;
        public double RedPointPercent
        {
            get => redpointPercent;
            set => SetProperty(ref redpointPercent, value);
        }

        private string ranking;
        public string Ranking
        {
            get => ranking;
            set => SetProperty(ref ranking, value);
        }

        private double rankingPercent;
        public double RankingPercentage
        {
            get => rankingPercent;
            set => SetProperty(ref rankingPercent, value);
        }

        private int daysClimbed;
        public int DaysClimbed
        {
            get => daysClimbed;
            set => SetProperty(ref daysClimbed, value);
        }

        private double climbingPercent;
        public double DaysClimbedPercentage
        {
            get => climbingPercent;
            set => SetProperty(ref climbingPercent, value);
        }

        private int sends;
        public int Sends
        {
            get => sends;
            set => SetProperty(ref sends, value);
        }

        private double sendsPercentage;
        public double SendsPercentage
        {
            get => sendsPercentage;
            set => SetProperty(ref sendsPercentage, value);
        }

        public bool IsFollowBtnVisible
        {
            get => profile != null && profile.MemberId != Settings.UserID;
        }

        public Thickness FollowersMargin //bound property is not able to resize with effect
        {
            get
            {
                var margin = IsFollowBtnVisible ? new Thickness(0, 30, 10, 0) : new Thickness(1, 30, 10, 0);
                //if (Device.RuntimePlatform == Device.iOS)
                //{
                var coef = SizeHelper.GetResizeCoeficientsAsync().Result;
                margin.Left *= coef.x;
                margin.Right *= coef.x;
                margin.Top *= coef.y;
                margin.Bottom *= coef.y;
                //}
                return margin;
            }
        }
        #endregion


        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;

            isLoaded = true;

            if (!parameters.TryGetValue(NavigationParametersConstants.MemberProfileId, out userId))
                return;

            //if (!parameters.ContainsKey("userID"))
            //{
            //    return;
            //}

            //userId = (int)parameters["userID"];
            if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileName, out string usrName))
                PageSubHeaderText = usrName;

            var response = await httpHelper.GetAsync<IEnumerable<MemberProfile>>(ApiUrls.Url_M_Social_GetMember(userId));

            if (response.ValidateResponse() && response.Result.Any())
            {
                profile = response.Result.FirstOrDefault();

                if (string.IsNullOrEmpty(PageSubHeaderText))
                    PageSubHeaderText = profile.DisplayName;

                await GetUserGroupInfo();
                FollowerCount = profile.FollowerCount;
                FollowingCount = profile.FollowingCount;

                Flash = profile.ProfileProperties.flash;
                FlashPercent = profile.ProfileProperties.flashpercentage;
                Onsight = profile.ProfileProperties.onsight;
                OnsightPercent = profile.ProfileProperties.onsightpercentage;
                RedPoint = profile.ProfileProperties.redpoint;
                RedPointPercent = profile.ProfileProperties.redpointpercentage;
                Ranking = profile.ProfileProperties.ranking.ToString() == "0" ? "N/A" : profile.ProfileProperties.sends.ToString() == "0" ? "N/A" : profile.ProfileProperties.ranking.ToString();
                RankingPercentage = profile.ProfileProperties.rankingpercent;
                DaysClimbed = profile.ProfileProperties.daysclimbed;
                DaysClimbedPercentage = profile.ProfileProperties.climbpercent;
                Sends = profile.ProfileProperties.sends;
                SendsPercentage = profile.ProfileProperties.sendspercent;
                RaisePropertyChanged(nameof(IsFollowBtnVisible));
                RaisePropertyChanged(nameof(FollowersMargin));
                RaisePropertyChanged(nameof(ProfileImageUrl));
                UpdateFollowProperties();
            }

            userDialogs.HideLoading();
            App.IsNavigating = false;
        }

        private void UpdateFollowProperties()
        {
            RaisePropertyChanged(nameof(FollowText));
            RaisePropertyChanged(nameof(FollowBGColor));
            RaisePropertyChanged(nameof(FollowTxtColor));
        }

        private async Task GetUserGroupInfo()
        {
            #region Bind Age
            if (profile.ProfileProperties?.DOB.HasValue ?? false)
            {
                int age = DateTime.Now.Year - profile.ProfileProperties.DOB.Value.Year;
                string ageFilter = "((" + age + " >= min) and (" + age + " < max) and type = 'a') ";
                string filterSQL = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + ageFilter;
                ageGroup = await profileFilterTable.ExecuteScalarAsync<string>(filterSQL) ?? string.Empty;
                AgeGroup = string.IsNullOrWhiteSpace(ageGroup) ? "N/A" : $"({ageGroup})";
            }
            #endregion



            #region Bind Year Climb
            var year = DateTime.Now.Year - profile.ProfileProperties.FirstYearClimb;
            string yearClimbFilter = "((" + year + " >= min) and (" + year + " < max) and type = 'y') ";
            string yearClimbFilterSql = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + yearClimbFilter;
            var yearClimbGroup = await profileFilterTable.ExecuteScalarAsync<string>(yearClimbFilterSql);
            YearsClimbGroup = string.IsNullOrWhiteSpace(yearClimbGroup) ? "N/A" : $"({yearClimbGroup})";
            #endregion
            var response = await userInfoRepository.GetAsync(Convert.ToInt32(Settings.UserID));
            #region Bind Height
            if (profile.ProfileProperties?.Height.HasValue ?? false)
            {
                int userHeight = Convert.ToInt32(profile.ProfileProperties.Height.Value);
                if ((response != null && response.height_uom == "in" && profile.ProfileProperties.Height_Uom == "in")
                    || (response.height_uom == "in" && profile.ProfileProperties.Height_Uom == "cm"))
                {
                    userHeight = Convert.ToInt32(userHeight * 0.393);
                }

                string heightFilter = "((" + userHeight + " >= min) and (" + userHeight + " < max) and type = 'h' and Trim(uom) = '" + response.height_uom + "') ";
                string heightFilterSql = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + heightFilter;
                heightGroup = await profileFilterTable.ExecuteScalarAsync<string>(heightFilterSql) ?? string.Empty;
                HeightGroup = string.IsNullOrWhiteSpace(heightGroup) ? "N/A" : "(" + heightGroup + ")";
            }
            #endregion

            #region Bind Weight
            if (profile.ProfileProperties?.Weight.HasValue ?? false)
            {
                int userWeight = Convert.ToInt32(profile.ProfileProperties.Weight.Value);
                if ((response != null && response.weight_uom == "lbs" && profile.ProfileProperties.Weight_Uom == "lbs")
                    || (response.weight_uom == "lbs" && profile.ProfileProperties.Weight_Uom == "kg"))
                {
                    userWeight = Convert.ToInt32(userWeight * 2.20462);
                }


                string weightFilter = "((" + userWeight + " >= min) and (" + userWeight + " < max) and type = 'w' and Trim(uom) = '" + response.weight_uom + "') ";
                string weightFilterSql = "SELECT filter_desc FROM [TUSER_PROFILE_FILTER] Where " + weightFilter;
                weightGroup = await profileFilterTable.ExecuteScalarAsync<string>(weightFilterSql) ?? string.Empty;
                WeightGroup = string.IsNullOrWhiteSpace(weightGroup) ? "N/A" : "(" + weightGroup + ")";
            }
            #endregion
        }


        #region CommandExecution
        //private async void ExecuteOnFriendClick()
        //{
        //    userDialogs.ShowLoading("loading..");
        //    if (FriendText == "ADD FRIEND")
        //    {
        //        FriendDTO frnddto = new FriendDTO();
        //        frnddto.FriendId = UserID;
        //        var res = await HttpAddFriend(JsonConvert.SerializeObject(frnddto));
        //        if (res != null)
        //        {
        //            if (res.Result == "success")
        //            {
        //                FriendText = "UNFRIEND";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        FriendDTO frnddto = new FriendDTO();
        //        frnddto.FriendId = UserID;
        //        var res = await HttpRemoveFriend(JsonConvert.SerializeObject(frnddto));
        //        if (res != null)
        //        {
        //            if (res.Result == "success")
        //            {
        //                FriendText = "ADD FRIEND";
        //            }
        //        }
        //    }
        //    userDialogs.HideLoading();
        //}
        async void OnFollowClick()
        {
            userDialogs.ShowLoading("loading..");

            var followdto = new FollowDTO
            {
                FollowId = userId
            };

            if (!IsFollowing)
            {
                var response = await httpHelper.PostAsync<RequestResult>(ApiUrls.Url_M_Social_Follow, followdto);
                if (response.ValidateResponse() && response.Result.Result == "success")
                {
                    profile.FollowerCount++;
                    FollowerCount++;
                    profile.FollowingStatus = 2;
                    UpdateFollowProperties();
                }
            }
            else
            {
                var response = await httpHelper.PostAsync<RequestResult>(ApiUrls.Url_M_Social_UnFollow, followdto);
                if (response.ValidateResponse() && response.Result.Result == "success")
                {
                    profile.FollowerCount--;
                    FollowerCount--;
                    profile.FollowingStatus = 0;
                    UpdateFollowProperties();
                }
            }
            userDialogs.HideLoading();
        }

        async void GoEditProfile()
        {
            if (IsFollowBtnVisible) //not my profile
                return;
            isLoaded = false;

            await navigationService.NavigateAsync<MyPreferencesViewModel>();
        }

        async void GoToActivity(string viewName)
        {
            try
            {
                if (string.IsNullOrEmpty(viewName))
                    return;

                var viewType = (ProfileViews)Enum.Parse(typeof(ProfileViews), viewName);



                var navParams = new NavigationParameters();
                if (IsFollowBtnVisible)
                {
                    navParams.Add(NavigationParametersConstants.MemberProfileId, userId);
                    navParams.Add(NavigationParametersConstants.MemberProfileName, profile.DisplayName ?? "PROFILE");

                }
                navParams.Add(NavigationParametersConstants.ApplicationActivity, viewType);

                await navigationService.NavigateAsync<ProfileViewModel>(navParams);

            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(GoToActivity),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = JsonConvert.SerializeObject(viewName)
                });
                throw;
            }
        }

        public async void LoadFollowers()
        {
            if (FollowerCount > 0)
            {
                var journalEngagedVM = new JournalEngagedUsersViewModel(navigationService, httpHelper, userDialogs);
                journalEngagedVM.UserID = userId;
                journalEngagedVM.LoadUsersFromProfilePage = true;
                journalEngagedVM.ProfileFlag = "Follower";

                var users_popup = new JournalEngagedUsers(journalEngagedVM);
                users_popup.Disappearing += (sender, e) => { this.OnNavigatedFromPopup(); };
                await PopupNavigation.PushAsync(users_popup, true);
            }
        }

        public async void LoadFollowings()
        {
            if (FollowingCount > 0)
            {
                var journalEngagedVM = new JournalEngagedUsersViewModel(navigationService, httpHelper, userDialogs);
                journalEngagedVM.UserID = userId;
                journalEngagedVM.LoadUsersFromProfilePage = true;
                journalEngagedVM.ProfileFlag = "Following";

                var users_popup = new JournalEngagedUsers(journalEngagedVM);
                users_popup.Disappearing += (sender, e) => { this.OnNavigatedFromPopup(); };
                await PopupNavigation.PushAsync(users_popup, true);
            }
        }

        public async void OnNavigatedFromPopup()
        {
            if (userId == Settings.UserID)
            {
                var response = await httpHelper.GetAsync<IEnumerable<MemberProfile>>(ApiUrls.Url_M_Social_GetMember(userId));
                if (response.ValidateResponse() && response.Result.Any())
                {
                    profile = response.Result.FirstOrDefault();
                    FollowerCount = profile.FollowerCount;
                    FollowingCount = profile.FollowingCount;
                }
            }
        }
        #endregion


        #region Services

        //private async Task<RequestResult> HttpAddFriend(string friendJSON)
        //{
        //    httpClinetHelper.ChangeTokens(ApiUrls.Url_M_Social_AddFriend, Settings.AccessToken);
        //    var response = await httpClinetHelper.Post<RequestResult>(friendJSON);
        //    return response;
        //}
        //private async Task<RequestResult> HttpRemoveFriend(string friendJSON)
        //{
        //    httpClinetHelper.ChangeTokens(ApiUrls.Url_M_Social_RemoveFriend, Settings.AccessToken);
        //    var response = await httpClinetHelper.Post<RequestResult>(friendJSON);
        //    return response;
        //}
        #endregion
    }
}
