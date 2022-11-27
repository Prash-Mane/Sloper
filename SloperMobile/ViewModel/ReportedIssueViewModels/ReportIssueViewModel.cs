using Acr.UserDialogs;
using FFImageLoading.Forms;
using Plugin.Media.Abstractions;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.IssueModels;
using SloperMobile.UserControls.ReportIssue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.ReportedIssueViewModels
{
	public class ReportIssueViewModel : BaseViewModel
    {
        private RouteTable routeData;
        private CragExtended currentCrag;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<IssueCategoryTable> issuecatRepository;
        private readonly IRepository<IssueTypeTable> issuetypeRepository;
        private readonly IRepository<IssueTypeDetailTable> issuetypedetailRepository;
        private readonly IRepository<IssueCategoryIssueTypeLinkTable> catissuetypelinkRepository;
        private readonly IRepository<IssueTypeDetailLinkTable> issuetypedetailtypelinkRepository;
        private readonly IUserDialogs userDialogs;
        private readonly ICheckForUpdateService checkForUpdateService;
        private readonly ICameraService cameraService;
        private readonly ISynchronizationManager synchronizationManager;

        private int routeBolts;
        private string progressmsg = "Sending, please wait...";
        private string commandtext = "SUBMIT";
        private MapListModel currentSector;
        private bool is_visiblecomment_next = false;
        private byte[] imageBytes;
        private string filePath;
        private int height;
        private int width;
        private int currentRouteId;
		private string singleRouteImageByte;

		public ReportIssueViewModel(
            IRepository<RouteTable> routeRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<IssueCategoryTable> issuecatRepository,
            IRepository<IssueTypeTable> issuetypeRepository,
            IRepository<IssueTypeDetailTable> issuetypedetailRepository,
            IRepository<IssueCategoryIssueTypeLinkTable> catissuetypelinkRepository,
            IRepository<IssueTypeDetailLinkTable> issuetypedetailtypelinkRepository,
            ICheckForUpdateService checkForUpdateService,
            INavigationService navigationService,
            IUserDialogs userDialogs,
            ICameraService cameraService,
            ISynchronizationManager synchronizationManager)
            : base(navigationService)
        {
            this.routeRepository = routeRepository;
            this.cragRepository = cragRepository;
            this.userDialogs = userDialogs;
            this.issuecatRepository = issuecatRepository;
            this.issuetypeRepository = issuetypeRepository;
            this.issuetypedetailRepository = issuetypedetailRepository;
            this.catissuetypelinkRepository = catissuetypelinkRepository;
            this.issuetypedetailtypelinkRepository = issuetypedetailtypelinkRepository;
            this.checkForUpdateService = checkForUpdateService;
            this.cameraService = cameraService;
            this.synchronizationManager = synchronizationManager;
            Offset = Common.Enumerators.Offsets.Header;
            IssueCategoryCommand = new Command<int>(ExecuteOnIssueCategory);
            IssueTypeCommand = new Command<int>(ExecuteOnIssueTypeCommand);
            IssueDetailTypeCommand = new Command<int>(ExecuteOnIssueDetailCommand);
            CameraClickCommand = new Command(ExecuteOnCameraClick);
            SelectBoltCommand = new Command(ExecuteOnSelectBoltCommand);
            OkBoltCommand = new Command(ExecuteOnSelectOkBolt);

            SnapNopeCommand = new Command(ExecuteOnSnapNopeCommand);
            SnapTotallyCommand = new Command(ExecuteOnSnapTotallyCommand);
            CommentClickCommand = new Command(ExecuteOnCommentClick);
            CommentNextCommand = new Command(ExecuteOnCommentNextClick);
            SubmitIssueCommand = new Command(ExecuteOnSubmitIssueClick);
            IssueImagePopupCommand = new Command(ExecuteOnIssueImagePopup);

            
            IsBackButtonVisible = true;
        }

        #region Commands

        public Command<int> IssueCategoryCommand { get; set; }
        public Command<int> IssueTypeCommand { get; set; }
        public Command<int> IssueDetailTypeCommand { get; set; }
        public Command CameraClickCommand { get; set; }
        public Command SelectBoltCommand { get; set; }
        public Command OkBoltCommand { get; set; }
        public Command SnapNopeCommand { get; set; }
        public Command SnapTotallyCommand { get; set; }
        public Command CommentClickCommand { get; set; }
        public Command CommentNextCommand { get; set; }
        public Command SubmitIssueCommand { get; set; }
        public Command IssueImagePopupCommand { get; set; }

        #endregion

        #region Properties

        //==============================================================
        public new bool IsShowHeader => true;

        private string _snapcommandtext = "SKIP";
        public string SnapCommandText
        {
            get { return _snapcommandtext; }
            set { _snapcommandtext = value; RaisePropertyChanged(); }
        }
        private int _selected_issue_category_id;
        public int SelectedIssueCategoryID
        {
            get { return _selected_issue_category_id; }
            set { _selected_issue_category_id = value; RaisePropertyChanged(); }
        }

        private string _selectedissuecategoryname;
        public string SelectedIssueCategoryName
        {
            get { return _selectedissuecategoryname; }
            set { _selectedissuecategoryname = value; RaisePropertyChanged(); }
        }

        private int _selected_issue_type_id;
        public int SelectedIssueTypeID
        {
            get { return _selected_issue_type_id; }
            set { _selected_issue_type_id = value; RaisePropertyChanged(); }
        }

        private string _selectedissuetypename;
        public string SelectedIssueTypeName
        {
            get { return _selectedissuetypename; }
            set { _selectedissuetypename = value; RaisePropertyChanged(); }
        }

        private int _issuetypecmdparam_1;
        public int IssueTypeCmdParam_1
        {
            get { return _issuetypecmdparam_1; }
            set { _issuetypecmdparam_1 = value; RaisePropertyChanged(); }
        }
        private int _issuetypecmdparam_2;
        public int IssueTypeCmdParam_2
        {
            get { return _issuetypecmdparam_2; }
            set { _issuetypecmdparam_2 = value; RaisePropertyChanged(); }
        }
        private int _issuetypecmdparam_3;
        public int IssueTypeCmdParam_3
        {
            get { return _issuetypecmdparam_3; }
            set { _issuetypecmdparam_3 = value; RaisePropertyChanged(); }
        }
        private int _issuetypecmdparam_4;
        public int IssueTypeCmdParam_4
        {
            get { return _issuetypecmdparam_4; }
            set { _issuetypecmdparam_4 = value; RaisePropertyChanged(); }
        }


        private int _issuedetailcmdparam_1;
        public int IssueDetailCmdParam_1
        {
            get { return _issuedetailcmdparam_1; }
            set { _issuedetailcmdparam_1 = value; RaisePropertyChanged(); }
        }
        private int _issuedetailcmdparam_2;
        public int IssueDetailCmdParam_2
        {
            get { return _issuedetailcmdparam_2; }
            set { _issuedetailcmdparam_2 = value; RaisePropertyChanged(); }
        }
        private int _issuedetailcmdparam_3;
        public int IssueDetailCmdParam_3
        {
            get { return _issuedetailcmdparam_3; }
            set { _issuedetailcmdparam_3 = value; RaisePropertyChanged(); }
        }
        private int _issuedetailcmdparam_4;
        public int IssueDetailCmdParam_4
        {
            get { return _issuedetailcmdparam_4; }
            set { _issuedetailcmdparam_4 = value; RaisePropertyChanged(); }
        }


        private int _selected_issue_detail_id;
        public int SelectedIssueDetailID
        {
            get { return _selected_issue_detail_id; }
            set { _selected_issue_detail_id = value; RaisePropertyChanged(); }
        }

        private string _selectedissuedetailname;
        public string SelectedIssueDetailName
        {
            get { return _selectedissuedetailname; }
            set { _selectedissuedetailname = value; RaisePropertyChanged(); }
        }

        private bool isdisplaymsg;
        public bool IsDisplayMessage
        {
            get { return isdisplaymsg; }
            set { isdisplaymsg = value; RaisePropertyChanged(); }
        }

        private bool isenablesubmit = true;
        public bool IsEnableSubmitButton
        {
            get { return isenablesubmit; }
            set { isenablesubmit = value; RaisePropertyChanged(); }
        }

        public string CommandText
        {
            get { return commandtext; }
            set { commandtext = value; RaisePropertyChanged(); }
        }
        //==================================================================


        private ImageSource _typeicon_1;
        public ImageSource TypeIcon_1
        {
            get { return _typeicon_1; }
            set { _typeicon_1 = value; RaisePropertyChanged(); }
        }

        private ImageSource _typeicon_2;
        public ImageSource TypeIcon_2
        {
            get { return _typeicon_2; }
            set { _typeicon_2 = value; RaisePropertyChanged(); }
        }
        private ImageSource _typeicon_3;
        public ImageSource TypeIcon_3
        {
            get { return _typeicon_3; }
            set { _typeicon_3 = value; RaisePropertyChanged(); }
        }
        private ImageSource _typeicon_4;
        public ImageSource TypeIcon_4
        {
            get { return _typeicon_4; }
            set { _typeicon_4 = value; RaisePropertyChanged(); }
        }

        //================================================================

        private ImageSource _typedetailicon_1;
        public ImageSource TypeDetailIcon_1
        {
            get { return _typedetailicon_1; }
            set { _typedetailicon_1 = value; RaisePropertyChanged(); }
        }

        private ImageSource _typedetailicon_2;
        public ImageSource TypeDetailIcon_2
        {
            get { return _typedetailicon_2; }
            set { _typedetailicon_2 = value; RaisePropertyChanged(); }
        }
        private ImageSource _typedetailicon_3;
        public ImageSource TypeDetailIcon_3
        {
            get { return _typedetailicon_3; }
            set { _typedetailicon_3 = value; RaisePropertyChanged(); }
        }
        private ImageSource _typedetailicon_4;
        public ImageSource TypeDetailIcon_4
        {
            get { return _typedetailicon_4; }
            set { _typedetailicon_4 = value; RaisePropertyChanged(); }
        }

        private ImageSource issueimage;
        public ImageSource IssueImage
        {
            get { return issueimage; }
            set { issueimage = value; RaisePropertyChanged(); }
        }

        private ImageSource icon_selectedcategory;
        public ImageSource Icon_SelectedCategory
        {
            get { return icon_selectedcategory; }
            set { icon_selectedcategory = value; RaisePropertyChanged(); }
        }

        private ImageSource icon_selectedissue;
        public ImageSource Icon_SelectedIssue
        {
            get { return icon_selectedissue; }
            set { icon_selectedissue = value; RaisePropertyChanged(); }
        }

        private ImageSource icon_selecteddetail;
        public ImageSource Icon_SelectedDetail
        {
            get { return icon_selecteddetail; }
            set { icon_selecteddetail = value; RaisePropertyChanged(); }
        }

        //============================================================================
        private ObservableCollection<Bolts> _issuebolts;
        public ObservableCollection<Bolts> IssueBolts
        {
            get { return _issuebolts; }
            set { _issuebolts = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<Bolts> _selectedbolts;
        public ObservableCollection<Bolts> SelectedBolts
        {
            get { return _selectedbolts; }
            set { _selectedbolts = value; RaisePropertyChanged(); }
        }
        private string commenttext;
        public string CommentText
        {
            get { return commenttext; }
            set { commenttext = value; RaisePropertyChanged(); }
        }

        private string _selectedbultnumber;
        public string SelectedBoltNumber
        {
            get { return _selectedbultnumber; }
            set { _selectedbultnumber = value; RaisePropertyChanged(); }
        }


        public bool IsVisibleCommentNextButton
        {
            get { return is_visiblecomment_next; }
            set { is_visiblecomment_next = value; RaisePropertyChanged(); }
        }
        public string ProgressMsg
        {
            get { return progressmsg; }
            set { progressmsg = value; RaisePropertyChanged(); }
        }
        private bool is_visibleboltext = false;
        public bool IsVisibleBoltText
        {
            get { return is_visibleboltext; }
            set { is_visibleboltext = value; RaisePropertyChanged(); }
        }
        private bool isdisplaycomment = false;
        public bool IsDisplayComment
        {
            get { return isdisplaycomment; }
            set { isdisplaycomment = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Command Execution
        private async void ExecuteOnIssueCategory(int cmdparam)
        {
            SelectedIssueCategoryID = cmdparam;

            SelectedIssueCategoryName = (await issuecatRepository.FindAsync(y => y.issue_category_id == SelectedIssueCategoryID))?.issue_category_name;
            var isutype = await issuecatRepository.QueryAsync<IssueTypeModel>($"SELECT TISSUE_TYPE.issue_type_id,issue_type_name,TISSUE_CATEGORY_TISSUE_TYPE_LINK.sort_order FROM TISSUE_CATEGORY_TISSUE_TYPE_LINK JOIN TISSUE_TYPE ON TISSUE_CATEGORY_TISSUE_TYPE_LINK.issue_type_id = TISSUE_TYPE.issue_type_id WHERE issue_category_id = {cmdparam}  order by TISSUE_CATEGORY_TISSUE_TYPE_LINK.sort_order asc");
            if (isutype.Count == 4)
            {
                if (SelectedIssueCategoryName == "Single Bolt(s)")
                {
                    IsVisibleBoltText = true;
                }
                else
                {
                    IsVisibleBoltText = false;
                }

                TypeIcon_1 = ImageSource.FromFile("icon_issue_type_" + isutype[0].issue_type_id);
                TypeIcon_2 = ImageSource.FromFile("icon_issue_type_" + isutype[1].issue_type_id);
                TypeIcon_3 = ImageSource.FromFile("icon_issue_type_" + isutype[2].issue_type_id);
                TypeIcon_4 = ImageSource.FromFile("icon_issue_type_" + isutype[3].issue_type_id);

                IssueTypeCmdParam_1 = isutype[0].issue_type_id;
                IssueTypeCmdParam_2 = isutype[1].issue_type_id;
                IssueTypeCmdParam_3 = isutype[2].issue_type_id;
                IssueTypeCmdParam_4 = isutype[3].issue_type_id;
                OnConditionAddView?.Invoke("issuetype");
            }
        }

        private async void ExecuteOnIssueTypeCommand(int cmdparam)
        {
            SelectedIssueTypeID = cmdparam;
            SelectedIssueTypeName = (await issuetypeRepository.FindAsync(x => x.issue_type_id == SelectedIssueTypeID)).issue_type_name;
            var isutype = await issuetypeRepository.QueryAsync<IssueTypeDetailModel>($@"SELECT TYPE_TISSUE_TYPE_DETAIL_LINK.issue_type_detail_id,TISSUE_TYPE_DETAIL.issue_type_detail_name ,TYPE_TISSUE_TYPE_DETAIL_LINK.sort_order
                                                    FROM TYPE_TISSUE_TYPE_DETAIL_LINK 
                                                    JOIN TISSUE_TYPE_DETAIL ON TYPE_TISSUE_TYPE_DETAIL_LINK.issue_type_detail_id = TISSUE_TYPE_DETAIL.issue_type_detail_id
                                                    WHERE TYPE_TISSUE_TYPE_DETAIL_LINK.issue_type_id ={cmdparam}
                                                    order by TYPE_TISSUE_TYPE_DETAIL_LINK.sort_order asc");


            if (isutype.Count == 4)
            {
                TypeDetailIcon_1 = ImageSource.FromFile("icon_issue_type_detail_" + isutype[0].issue_type_detail_id);
                TypeDetailIcon_2 = ImageSource.FromFile("icon_issue_type_detail_" + isutype[1].issue_type_detail_id);
                TypeDetailIcon_3 = ImageSource.FromFile("icon_issue_type_detail_" + isutype[2].issue_type_detail_id);
                TypeDetailIcon_4 = ImageSource.FromFile("icon_issue_type_detail_" + isutype[3].issue_type_detail_id);
                IssueDetailCmdParam_1 = isutype[0].issue_type_detail_id;
                IssueDetailCmdParam_2 = isutype[1].issue_type_detail_id;
                IssueDetailCmdParam_3 = isutype[2].issue_type_detail_id;
                IssueDetailCmdParam_4 = isutype[3].issue_type_detail_id;
                OnConditionAddView?.Invoke("issuedetail");
            }
            else if (isutype.Count == 0 && SelectedIssueCategoryName == "Single Bolt(s)")
            {
                SetupBolts();
                OnConditionAddView?.Invoke("bolts");
            }
            else if (isutype.Count == 0)
            {
                OnConditionAddView?.Invoke("snap");
            }
        }

        private void SetupBolts()
        {
            var bolts = new ObservableCollection<Bolts>();

            for (int i = 1; i <= (routeBolts == 0 ? 20 : routeBolts); i++)
            {
                Bolts objbolt = new Bolts();
                objbolt.Bolt = i;
                bolts.Add(objbolt);
            }

            IssueBolts = bolts;
        }

        private async void ExecuteOnIssueDetailCommand(int cmdparam)
        {
            SelectedIssueDetailID = cmdparam;
            var issueType = await issuetypedetailRepository.FindAsync(x => x.issue_type_detail_id == SelectedIssueDetailID);
            SelectedIssueDetailName = issueType.issue_type_detail_name;
            if (SelectedIssueCategoryName == "Single Bolt(s)"
                && (SelectedIssueTypeName == "Loose"
                || SelectedIssueTypeName == "Hardware Missing"
                || SelectedIssueTypeName == "Aged"))
            {
                SetupBolts();
                OnConditionAddView?.Invoke("bolts");
            }
            else
            {
                OnConditionAddView?.Invoke("snap");
            }

        }

        private async void ExecuteOnCameraClick()
        {
            var action = await Application.Current.MainPage.DisplayActionSheet("Choose Route Issue Photo", "Cancel", null, "Select Photo", "Take Photo");
            if (action == "Take Photo")
            {
                var mediaFile = await cameraService.TakePhotoAsync("Ascent_", "ReportIssue");
                if (mediaFile != null)
                {
                    SetupImageBytes(mediaFile);
                    SnapCommandText = "CONTINUE";
                }
            }
            else if (action == "Select Photo")
            {
                var mediaFile = await cameraService.PickFileAsync();
                if (mediaFile != null)
                {
                    SetupImageBytes(mediaFile);
                    SnapCommandText = "CONTINUE";
                }
            }
        }

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

		public ObservableCollection<ImageSource> AscentSummaryImgs { get; internal set; }
		public List<byte[]> AscentSummaryImagesBytes { get; internal set; }
		public bool ShareSelected { get; internal set; }
		public byte[] TakenImageBytes { get; internal set; }
		public object RouteNameWithGradeForPicture { get; internal set; }
		public byte[] AscentImageWithNoStarts { get; internal set; }
		public string SendsTypeText { get; internal set; }
		public int SendRating { get; internal set; }
		public string SendClimbingStyle { get; internal set; }
		public string SendHoldType { get; internal set; }
		public string SendRouteCharacteristics { get; internal set; }
		public List<byte[]> AscentSummaryImagesToShareBytes { get; internal set; }

		private async void SetupImageBytes(MediaFile mediaFile)
        {
            using (var stream = mediaFile.GetStreamWithImageRotatedForExternalStorage())
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    imageBytes = ms.ToArray();
                    var imageResizer = DependencyService.Get<IImageResizer>();
                    imageBytes = imageResizer.ResizeImage(imageBytes, 1024, 1024);
                }
            }

            IssueImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }

        private async void ExecuteOnSelectBoltCommand()
        {												   
            await PopupNavigation.PushAsync(new IssueBoltsPopupPage(this), true);
        }

        private async void ExecuteOnIssueImagePopup()
        {
            var navigationParameters = new NavigationParameters
            {
                {NavigationParametersConstants.ImageBytesParameter, imageBytes},
                {NavigationParametersConstants.ImageHeightParameter, (float)height},
                {NavigationParametersConstants.ImageWidthParameter, (float)width},
                {"PageSubHeaderText", SelectedIssueTypeName},
                {"PageHeaderText", PageHeaderText }
            };

            await navigationService.NavigateAsync<ReportedIssueImageViewModel>(navigationParameters);
        }

        private void ExecuteOnSelectOkBolt()
        {
            OnConditionAddView?.Invoke("snap");
        }

        private void ExecuteOnSnapNopeCommand()
        {											   
            Icon_SelectedCategory = ImageSource.FromFile("icon_issue_category_" + SelectedIssueCategoryID);
            Icon_SelectedIssue = ImageSource.FromFile("icon_issue_type_" + SelectedIssueTypeID);
            Icon_SelectedDetail = ImageSource.FromFile("icon_issue_type_detail_" + SelectedIssueDetailID);
            OnConditionAddView?.Invoke("summary");
        }

        private void ExecuteOnSnapTotallyCommand()
        {
            OnConditionAddView?.Invoke("comments");
        }

        private async void ExecuteOnCommentClick()
        {
            var result = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                Title = "Comments",
                InputType = InputType.Name,
                Message = "Give us your comments on this route",
                Text = CommentText,
                MaxLength = 250,
                Placeholder = "type here",
                OkText = "Ok",
                IsCancellable = true,
                CancelText = "Cancel"
            });

            if (!result.Ok)
            {
                return;
            }

            SnapCommandText = "CONTINUE";
            CommentText = result.Text;
            if (CommentText != "")
                IsDisplayComment = true;
            else
                IsDisplayComment = false;

            IsVisibleCommentNextButton = true;
        }

        private void ExecuteOnCommentNextClick()
        {
            OnConditionAddView?.Invoke("summary");
        }

        private async void ExecuteOnSubmitIssueClick()
        {
            if (CommandText == "SUBMIT" && IsEnableSubmitButton)
            {
                userDialogs.ShowLoading("Reporting Issue");
                IsRunningTasks = true;
                IsDisplayMessage = true;
                IsEnableSubmitButton = false;
				if (!routeData.route_image_id.HasValue || routeData.route_image_id == 0)
				{
					await SaveRouteImageAsync(ImageType.SummaryImage, null, routeData.route_id, AscentImageWithNoStarts);
				}

				IssueModel dtoObj = new IssueModel();
                dtoObj.route_id = Convert.ToInt32(routeData.route_id);
                dtoObj.issue_category_id = SelectedIssueCategoryID;
                dtoObj.issue_type_detail_id = SelectedIssueDetailID;
                dtoObj.issue_type_id = SelectedIssueTypeID;
                dtoObj.comments = !string.IsNullOrEmpty(CommentText) ? CommentText : "";
                dtoObj.bolt_numbers = !string.IsNullOrEmpty(SelectedBoltNumber) ? SelectedBoltNumber : "";
				dtoObj.image = singleRouteImageByte != null ? singleRouteImageByte : string.Empty;
				var res = await synchronizationManager.SendIssueDataAsync(dtoObj);
                if (res?.status == "true")
                {
                    ProgressMsg = "Issue Reported successfully.";
                    IsRunningTasks = false;
                    await checkForUpdateService.RunCheckForUpdates();
                }
                else
                {
					userDialogs.HideLoading();
                    await userDialogs.AlertAsync("No internet connection. Data will be synchronized when connect to internet.", "Network Error", "Continue");
                    ProgressMsg = "Data saved locally!";
                    IsRunningTasks = false;
                }
																													  
                CommandText = "CONTINUE";
                IsEnableSubmitButton = true;
                userDialogs.HideLoading();
                return;
            }

            IsRunningTasks = false;
            IsDisplayMessage = false;
            var navigationParameters = new NavigationParameters();
            navigationParameters.Add("selectedSectorObject", currentSector);
			await navigationService.GoBackAsync();
        }

        #endregion

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }

		public async override void OnNavigatingTo(NavigationParameters parameters)
		{
            base.OnNavigatingTo(parameters);
            try
			{	 
				if (parameters.Count == 0)
				{
					return;
				}

				singleRouteImageByte = (string)parameters[NavigationParametersConstants.SingleRouteImageByteParameter];
				currentRouteId = (int)parameters[NavigationParametersConstants.CurrentRouteIdParameter];
				currentSector = (MapListModel)parameters[NavigationParametersConstants.CurrentSectorParameter];
				currentCrag = await cragRepository.FindAsync(c => c.crag_id == Settings.ActiveCrag);
				routeData = await routeRepository.FindAsync(r => r.route_id == currentRouteId && r.is_enabled);
				routeBolts = routeData.number_of_bolts ?? 0;
				PageSubHeaderText = $"{currentSector?.SectorName}, {(currentCrag?.crag_name).Trim()}";
				PageHeaderText = $"{(routeData.route_name).ToUpper()} {routeData.tech_grade}";
			}
			catch (Exception exception)
			{

			}
		}

		private async Task SaveRouteImageAsync(ImageType imageType, int? ascentId, int? routeId, byte[] imageBytes)
		{				  
			var ascentImgData = new AscentImageModel
			{
				ImageBytes = imageBytes,
				ImageType = ImageType.RouteImage
			};

			if (ascentId != null)
			{
				ascentImgData.AscentId = ascentId.Value;
			}

			if (routeId != null)
			{
				ascentImgData.RouteId = routeId.Value;
			}
			ascentImgData.appId = Convert.ToInt32(AppSetting.APP_ID);
			ascentImgData.FiveStarAscentCheck = Convert.ToInt32(AppSetting.FiveStarAscentCheck);

			var imageId = await synchronizationManager.SendImageDataAsync(ascentImgData);
			if (routeId != null)
			{
			    routeData.route_image_id = imageId;
				routeRepository.UpdateAsync(routeData);
			}
		}
	}
}
