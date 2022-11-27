using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.NotifyProperties;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.SocialModels;
using SloperMobile.UserControls;
using SloperMobile.Views.SocialPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.SocialModels
{
    public class JournalFeedViewModel : BaseViewModel
    {
        private const int ITEMS_PER_PAGE = 10;

        private readonly IUserDialogs userDialogs;
        bool isLoaded;

        public JournalFeedViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionSynchronizationManager,
            IHttpHelper httpHelper,
            IUserDialogs userDialogs) : base(navigationService, exceptionSynchronizationManager, httpHelper)
        {
            this.userDialogs = userDialogs;
            IsMenuVisible = true;
            PageHeaderText = "Activity Feed";
            PageSubHeaderText = "What's New?";

			HasFade = true;
            //Offset = Common.Enumerators.Offsets.Header;
            GradientHeaderHeight = 100;
            HeaderColors = new List<GradientColor> {
                new GradientColor { Color = Color.FromHex("#cc000000"), Position = 0.3f },
                new GradientColor { Color = Color.Transparent, Position = 0.9f }
            };
            IsShowFooter = true;
        }


        private ObservableCollectionFast<JournalFeed> _feedlist;
        public ObservableCollectionFast<JournalFeed> FeedList
        {
            get { return _feedlist ?? (_feedlist = new ObservableCollectionFast<JournalFeed>()); }
            set
            {
                SetProperty(ref _feedlist, value);
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
            }
        }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool ShowEmptyOverlay
        {
            get => (FeedList == null || FeedList.Count == 0) && !IsRunningTasks && isLoaded;
        }

        public Command ProfileCommand
        {
            get => new Command<JournalFeed>(GoToProfile);
        }

        public ICommand LikeUnlikeCommand
        {
            get => new Command<JournalFeed>(OnLike);
        }

        public ICommand CommentCommand
        {
            get
            {
                return new Command<JournalFeed>(GoToComments);
            }
        }

        public ICommand RefreshCommand => new Command(async () =>
                                                        {

                                                            IsRefreshing = true;
                                                            FeedList.Clear();
                                                            await SetFeedListAsync();
                                                            IsRefreshing = false;
                                                        });

        public ICommand LoadMoreFeedCommand =>
                new Command(OnLoadMore, () => !IsRunningTasks);

        public ICommand LikeUserList
        {
            get => new Command<JournalFeed>(LoadLikeUsers);
        }

        public ICommand CommentUserList
        {
            get => new Command<JournalFeed>(LoadCommentUsers);
        }

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;
            try
            {
                await SetFeedListAsync();

                isLoaded = true;
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = JsonConvert.SerializeObject(exception.Data)
                });
            }
            finally
            {
                App.IsNavigating = false;
            }
        }

        void GoToProfile(JournalFeed feedModel)
        {
            UserDialogs.Instance.ShowLoading("Loading...");
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(NavigationParametersConstants.MemberProfileId, feedModel.UserId);
            navigationParameters.Add(NavigationParametersConstants.MemberProfileName, feedModel.JournalAuthor.Name);
            navigationService.NavigateAsync<MemberProfileViewModel>(navigationParameters);
        }

        async void OnLoadMore()
        {
            await SetFeedListAsync();
        }

        async void OnLike(JournalFeed feed)
        {
            var response = await httpHelper.GetAsync<int>(ApiUrls.Url_M_Social_LikeAPost(feed.JournalId));

            if (response.ValidateResponse(false))
            {
                if (response.Result == 0)
                {
                    feed.LikeByCurrentUser = 0;
                    //feed.CurrentUserLikes = false;
                    if (feed.LikeCounts > 0)
                        feed.LikeCounts--;
                }
                else
                {
                    feed.LikeByCurrentUser = 1;
                    //feed.CurrentUserLikes = true;
                    feed.LikeCounts++;
                }
            }
        }

        async void LoadLikeUsers(JournalFeed feed)
        {
            if (feed.LikeCounts > 0)
            {
                var journalEngagedVM = new JournalEngagedUsersViewModel(navigationService, httpHelper, userDialogs);
                journalEngagedVM.JournalID = feed.JournalId;
                journalEngagedVM.Flag = "Like";
                await PopupNavigation.PushAsync(new JournalEngagedUsers(journalEngagedVM), true);
            }
        }
        async void LoadCommentUsers(JournalFeed feed)
        {
            if (feed.CommentCount > 0)
            {
                var journalEngagedVM = new JournalEngagedUsersViewModel(navigationService, httpHelper, userDialogs);
                journalEngagedVM.JournalID = feed.JournalId;
                journalEngagedVM.Flag = "Comment";
                await PopupNavigation.PushAsync(new JournalEngagedUsers(journalEngagedVM), true);
            }
        }
        async void GoToComments(JournalFeed feed)
        {
            var navigationParameter = new NavigationParameters();
            navigationParameter.Add("CurrentFeed", feed);
            navigationParameter.Add("OnAddComment", new Action(() => feed.CommentCount++));
            await this.navigationService.NavigateAsync<JournalCommentViewModel>(navigationParameter);
        }

        private async Task SetFeedListAsync()
        {
            IsRunningTasks = true;

            var response = await httpHelper.PostAsync<ObservableCollectionFast<JournalFeed>>
                                           (ApiUrls.Url_M_Social_GetJournal(FeedList.Count, ITEMS_PER_PAGE), string.Empty);

            if (response.ValidateResponse())
            {
                foreach (var journal in response.Result)
                {
                    //for now all types can be converted to same types, since all fields are id, image
                    if (!string.IsNullOrEmpty(journal.Summary))
                        journal.SummaryParsed = JsonConvert.DeserializeObject<ParsedJournalGeneral>(journal.Summary);
                }

                var result = response.Result;

                if (result?.Count > 0)
                {
                    FeedList.AddRange(result);
                }
            }

            await Task.Delay(500); //hack to disable trigger OnLoadMore while list is being updated
            IsRunningTasks = false;
        }
    }
}
