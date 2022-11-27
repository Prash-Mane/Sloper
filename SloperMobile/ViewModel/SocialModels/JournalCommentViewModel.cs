using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants; 
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.NotifyProperties;
using System.Linq;
using SloperMobile.Model.SocialModels;
using System;
using SloperMobile.Common.Extentions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.SocialModels
{
	public class JournalCommentViewModel : BaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        bool isLoaded;

        public event Action OnPostAdded;

        public JournalCommentViewModel(
            IHttpHelper httpHelper,
            INavigationService navigationService=null,
            IUserDialogs userDialogs=null) : base(navigationService, httpHelper)
        {
            this.userDialogs = userDialogs;

            IsBackButtonVisible = true;
            PageHeaderText = "Comments";
            Offset = Common.Enumerators.Offsets.Header;
            PostCommand = new Command(ExecuteOnPostComment);
        }

        private ObservableCollectionFast<Comments> _commentlist;
        public ObservableCollectionFast<Comments> CommentList
        {
            get { return _commentlist ?? (_commentlist = new ObservableCollectionFast<Comments>()); }
            set { 
                _commentlist = value; 
                RaisePropertyChanged("CommentList"); 
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
            }
        }
        private JournalFeed _currentfeed;
        public JournalFeed CurrentFeed
        {
            get { return _currentfeed; }
            set { _currentfeed = value; RaisePropertyChanged("CurrentFeed"); }
        }
        private string _commenttext;
        public string CommentText
        {
            get { return _commenttext; }
            set {
                SetProperty(ref _commenttext, value);
                RaisePropertyChanged(nameof(ShowPost));
            }
        }

        public bool ShowPost { get => !string.IsNullOrEmpty(CommentText); }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                RaisePropertyChanged("IsRefreshing");
            }
        }

        public bool ShowEmptyOverlay
        {
            get => (CommentList == null || CommentList.Count == 0) && !IsRunningTasks && isLoaded;
        }

        public Command PostCommand { get; set; }

		public Command ProfileCommand
		{
			get => new Command<Comments>(GoToProfile);
		}

		public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;
            
            CurrentFeed = (JournalFeed)parameters["CurrentFeed"];
            if (parameters.TryGetValue<Action>("OnAddComment", out var addAction))
                OnPostAdded += addAction;
            CommentList = await HttpGetJournalComments(0, 10);
            isLoaded = true;
        }

        public ICommand LoadmoreCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;
                    var coments = await HttpGetJournalComments(CommentList.Count, 10);
                    if (coments.Count > 0)
                        CommentList.AddRangeToTop(coments.Reverse());
                    IsRefreshing = false;
                });
            }
        }

        private async void ExecuteOnPostComment()
        {
            if (string.IsNullOrEmpty(CommentText))
                return;

           var response = await HttpPostJournalComments();

            if(response.ValidateResponse(true))
            {
                var result = response.Result.OrderBy(x=> x.DateCreated);

                CommentList = new ObservableCollectionFast<Comments>(result);

                CommentText = string.Empty;
                OnPostAdded?.Invoke();
            }
        }
        #region Services
        private async Task<ObservableCollectionFast<Comments>> HttpGetJournalComments(int skip, int take)
        {
            IsRunningTasks = true;
            //httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_M_Social_CommentList, CurrentFeed.JournalId,skip,take), Settings.AccessToken);
            //var response = await httpClinetHelper.Post<List<Comments>>(string.Empty);

            var response = await httpHelper.PostAsync<IEnumerable<Comments>>(
                ApiUrls.Url_M_Social_CommentList(CurrentFeed.JournalId, skip, take), string.Empty);


            IsRunningTasks = false;

            if (response.ValidateResponse())
            {
                var result = response.Result.OrderBy(x => x.DateCreated);

                return new ObservableCollectionFast<Comments>(result);
            }
            else
                return new ObservableCollectionFast<Comments>();

        }

        private async Task<OperationResult<ObservableCollectionFast<Comments>>> HttpPostJournalComments()
        {
            var cmnt = new CommentDTO
            {
                Comment = CommentText,
                JournalId = CurrentFeed.JournalId,
                Mentions = string.Empty
            };

            var response = await httpHelper.PostAsync<ObservableCollectionFast<Comments>>(ApiUrls.Url_M_Social_PostComment,cmnt);
            return response;
        }

		void GoToProfile(Comments commentModel)
		{
			UserDialogs.Instance.ShowLoading("Loading...");
			var navigationParameters = new NavigationParameters();
			navigationParameters.Add(NavigationParametersConstants.MemberProfileId, commentModel.UserId);
			navigationParameters.Add(NavigationParametersConstants.MemberProfileName, commentModel.DisplayName);
			navigationService.NavigateAsync<MemberProfileViewModel>(navigationParameters);
		}


		#endregion
	}
}
