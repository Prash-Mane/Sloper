using System;
using System.IO;
using System.Windows.Input;
using Acr.UserDialogs;
using FFImageLoading.Forms;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.ReportedIssueViewModels
{
	public class ReportedIssueSummaryViewModel : BaseViewModel
	{
		private readonly IRepository<IssueTable> issueRepository;
		private readonly IRepository<RouteTable> routeRepository;
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IUserDialogs userDialogs;
		private RouteTable routeData;
		private CragExtended currentCrag;

		public ReportedIssueSummaryViewModel(
			INavigationService navigationService,
			IRepository<IssueTable> issueRepository,
			IRepository<RouteTable> routeRepository,
			IRepository<CragExtended> cragRepository,
			IUserDialogs userDialogs
		) : base(navigationService)
		{
			this.issueRepository = issueRepository;
			this.routeRepository = routeRepository;
			this.cragRepository = cragRepository;
			this.userDialogs = userDialogs;
            IssueImagePopupCommand = new Command(ExecuteOnIssueImagePopup);

            IsBackButtonVisible = true;
		}

		public Command IssueImagePopupCommand { get; set; }

		public override void OnNavigatedFrom(NavigationParameters parameters)
		{
            base.OnNavigatedFrom(parameters);
        }

		public override void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
        }

		private string routeName = "";

		public string RouteName
		{
			get { return routeName; }
			set
			{
				routeName = value;
				RaisePropertyChanged();
			}
		}

		private ImageSource icon_selectedcategory;

		public ImageSource Icon_SelectedCategory
		{
			get { return icon_selectedcategory; }
			set
			{
				icon_selectedcategory = value;
				RaisePropertyChanged();
			}
		}

		private ImageSource icon_selectedissue;

		public ImageSource Icon_SelectedIssue
		{
			get { return icon_selectedissue; }
			set
			{
				icon_selectedissue = value;
				RaisePropertyChanged();
			}
		}

		private ImageSource icon_selecteddetail;

		public ImageSource Icon_SelectedDetail
		{
			get { return icon_selecteddetail; }
			set
			{
				icon_selecteddetail = value;
				RaisePropertyChanged();
			}
		}

		private ImageSource _issueimage;

		public ImageSource IssueImage
		{
			get { return _issueimage; }
			set
			{
				_issueimage = value;
				RaisePropertyChanged();
			}
		}

		private string _selectedbultnumber;

		public string SelectedBoltNumber
		{
			get { return _selectedbultnumber; }
			set
			{
				_selectedbultnumber = value;
				RaisePropertyChanged();
			}
		}

		private string commenttext;

		public string CommentText
		{
			get { return commenttext; }
			set
			{
				commenttext = value;
				RaisePropertyChanged();
			}
		}

		private bool is_visibleboltext = false;

		public bool IsVisibleBoltText
		{
			get { return is_visibleboltext; }
			set
			{
				is_visibleboltext = value;
				RaisePropertyChanged();
			}
		}
        private bool isdisplaycomment = false;
        public bool IsDisplayComment
        {
            get { return isdisplaycomment; }
            set { isdisplaycomment = value; RaisePropertyChanged(); }
        }
        private byte[] imageBytes;
		private int height;
		private int width;

		public ICommand SuccessCommand
		{
			get
			{
				return new Command<CachedImageEvents.SuccessEventArgs>(e =>
				{
					height = e.ImageInformation.OriginalHeight;
					width = e.ImageInformation.OriginalWidth;
				});
			}
		}

		public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            var currentRouteId = (int) parameters[NavigationParametersConstants.CurrentRouteIdParameter];
			var currentIssueId = (int) parameters["CurrentIssueID"];
			currentCrag = await cragRepository.FindAsync(c => c.crag_id == Settings.ActiveCrag);
			routeData = await routeRepository.FindAsync(r => r.route_id == currentRouteId && r.is_enabled);
			RouteName = $"{(routeData.route_name).ToUpper()} {routeData.tech_grade}";

			var isssue = await issueRepository.FindAsync(x => x.issue_id == currentIssueId);
			if (isssue != null)
			{
				Icon_SelectedCategory = ImageSource.FromFile("icon_issue_category_" + isssue.issue_category_id);
				Icon_SelectedIssue = ImageSource.FromFile("icon_issue_type_" + isssue.issue_type_id);
				Icon_SelectedDetail = ImageSource.FromFile("icon_issue_type_detail_" + isssue.issue_type_detail_id);
				if (!string.IsNullOrEmpty(isssue.image))
				{
					imageBytes = Convert.FromBase64String(isssue.image);
					IssueImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
				}
				if (string.IsNullOrEmpty(isssue.bolt_numbers))
					IsVisibleBoltText = false;
				else
					IsVisibleBoltText = true;
				SelectedBoltNumber = isssue.bolt_numbers;
				CommentText = isssue.comments;
                if (CommentText != "")
                    IsDisplayComment = true;
                else
                    IsDisplayComment = false;
            }

			PageHeaderText = RouteName;
			PageSubHeaderText = $"{Cache.SelctedCurrentSector.SectorName}, {(currentCrag.crag_name).Trim()}";
		}

		private async void ExecuteOnIssueImagePopup()
		{
			var navigationParameters = new NavigationParameters
			{
				{NavigationParametersConstants.ImageBytesParameter, imageBytes},
                {NavigationParametersConstants.ImageHeightParameter, (float)height},
                {NavigationParametersConstants.ImageWidthParameter, (float)width},
                {"PageSubHeaderText", PageSubHeaderText},
                {"PageHeaderText", PageHeaderText }
			};

			await navigationService.NavigateAsync<ReportedIssueImageViewModel>(navigationParameters);
		}
	}
}
