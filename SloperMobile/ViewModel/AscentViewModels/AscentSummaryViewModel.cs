using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.BucketsModel;
using SloperMobile.Model.SendsModels;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.ViewModel.SectorViewModels;
using Xamarin.Forms;
using SloperMobile.Common.Enumerators;

namespace SloperMobile.ViewModel.AscentViewModels
{
    public class AscentSummaryViewModel : BaseViewModel
    {
        private readonly IRepository<TechGradeTable> techGradeRepository;
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IUserDialogs userDialogs;
        private readonly ICameraService cameraService;
		private readonly ISynchronizationManager synchronizationManager;

		private List<string> grades;
        private List<string> listimgs;

        private DateTime sendsdate = DateTime.Now.Date;
        private IList<byte[]> ascentSummaryImagesToShareBytes;
        private IList<byte[]> ascentSummaryImagesBytes;
        private string sendsgrade;
        private string sendclimbingstyle;
        private string sendholdtype;
        private string sendroutecharacteristics;
        private string commenttext;
        private int routeid;
        private bool isenable = true;
        private bool isdisplaymsg;
        private string sendstypetext;
        private bool isdisplaysummaryimg = false;
        private bool isdisplaysummaryweb = true;
        private ImageSource summaryimage;
        private RouteTable routeData;
        private string routeName = "";
        private string sendsTypeName = "";
        private int sendRating = 0;
        private string commandText = "";
        private ImageSource topAngle = null;
        private ImageSource topRouteChar = null;
        private ObservableCollection<ImageSource> _AscentSummaryImgs;
        private bool isShare = false;
        private int _position;
        private string filePath;
        private bool isButtonVisible = true;
        private bool isTopoBackGroundImageVisible;
        private ImageSource topoBackGroundImage;
        private AbsoluteLayoutFlags layoutFlags = AbsoluteLayoutFlags.SizeProportional;
        private bool isLogAscentSelected;
        private string _commentText;

        public AscentSummaryViewModel(
            INavigationService navigationService,
            IRepository<TechGradeTable> techGradeRepository,
            IRepository<SectorTable> sectorRepository,
            IUserDialogs userDialogs,
            IExceptionSynchronizationManager exceptionManager,
            IRepository<RouteTable> routeRepository,
            ICameraService cameraService,
            ISynchronizationManager synchronizationManager) : base(navigationService, exceptionManager)
        {
            this.techGradeRepository = techGradeRepository;
            this.sectorRepository = sectorRepository;
            this.userDialogs = userDialogs;
            this.routeRepository = routeRepository;
            this.cameraService = cameraService;
			this.synchronizationManager = synchronizationManager;
            _AscentSummaryImgs = new ObservableCollection<ImageSource>();
            ShareCommand = new Command(ExecuteOnShare);
            ContinueCommand = new Command(ExecuteOnContinue);
            Offset = Common.Enumerators.Offsets.Header;
            IsBackButtonVisible = true;

            ascentSummaryImagesBytes = new List<byte[]>();
        }

        public byte[] AscentImageWithNoStarts { get; set; }

        public bool IsLogAscentSelected
        {
            get { return isLogAscentSelected; }
            set { SetProperty(ref isLogAscentSelected, value); }
        }

        public byte[] CanvasCapture { get; set; }

        public AbsoluteLayoutFlags LayoutFlags
        {
            get { return layoutFlags; }
            set { SetProperty(ref layoutFlags, value); }
        }

        public int Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public bool IsShare
        {
            get { return isShare; }
            set { isShare = value; RaisePropertyChanged(); }
        }

        public bool IsDisplaySummaryImg
        {
            get { return isdisplaysummaryimg; }
            set { isdisplaysummaryimg = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<ImageSource> AscentSummaryImgs
        {
            get { return _AscentSummaryImgs; }
            set { SetProperty(ref _AscentSummaryImgs, value); }
        }

        public string RouteName
        {
            get { return routeName; }
            set { routeName = value; RaisePropertyChanged(); }
        }

        public string SendsTypeName
        {
            get { return sendsTypeName; }
            set { sendsTypeName = value; RaisePropertyChanged(); }
        }

        public int SendRating
        {
            get { return sendRating; }
            set { sendRating = value; RaisePropertyChanged(); }
        }

        public ImageSource TopAngle
        {
            get { return topAngle; }
            set { topAngle = value; RaisePropertyChanged(); }
        }

        public ImageSource TopRouteChar
        {
            get { return topRouteChar; }
            set { topRouteChar = value; RaisePropertyChanged(); }
        }

        public string CommandText
        {
            get { return commandText; }
            set { commandText = value; RaisePropertyChanged(); }
        }

        public List<string> AscentGrades
        {
            get { return grades; }
            set { grades = value; RaisePropertyChanged(); }
        }

        public List<string> ListImages
        {
            get { return listimgs; }
            set { listimgs = value; RaisePropertyChanged(); }
        }

        public DateTime SendsDate
        {
            get { return sendsdate; }
            set { sendsdate = value; RaisePropertyChanged(); }
        }

        public string SendsGrade
        {
            get { return sendsgrade; }
            set { sendsgrade = value; RaisePropertyChanged(); }
        }

        public string SendClimbingStyle
        {
            get { return sendclimbingstyle; }
            set { sendclimbingstyle = value; RaisePropertyChanged(); }
        }

        public string SendHoldType
        {
            get { return sendholdtype; }
            set { sendholdtype = value; RaisePropertyChanged(); }
        }

        public string SendRouteCharacteristics
        {
            get { return sendroutecharacteristics; }
            set { sendroutecharacteristics = value; RaisePropertyChanged(); }
        }


        //not used?
        //public ImageSource SummaryImage
        //{
        //    get { return summaryimage; }
        //    set { summaryimage = value; RaisePropertyChanged(); }
        //}

        public int RouteId
        {
            get { return routeid; }
            set { routeid = value; RaisePropertyChanged(); }
        }

        public string SendsTypeText
        {
            get { return sendstypetext; }
            set { sendstypetext = value; RaisePropertyChanged(); }
        }

        public string CommentText
        {
            get { return _commentText; }
            set { _commentText = value; RaisePropertyChanged(); }
        }

        public bool IsDisplayMessage
        {
            get { return isdisplaymsg; }
            set { isdisplaymsg = value; RaisePropertyChanged(); }
        }

        public IList<byte[]> AscentSummaryImagesBytes
        {
            get
            {
                return ascentSummaryImagesBytes;
            }
            set { SetProperty(ref ascentSummaryImagesBytes, value); }
        }

        public IList<byte[]> AscentSummaryImagesToShareBytes
        {
            get { return ascentSummaryImagesToShareBytes; }
            set { SetProperty(ref ascentSummaryImagesToShareBytes, value); }
        }

        public bool IsButtonVisible
        {
            get
            {
                return isButtonVisible;
            }
            set
            {
                SetProperty(ref isButtonVisible, value);
            }
        }

        public bool IsTopoBackGroundImageVisible
        {
            get
            {
                return isTopoBackGroundImageVisible;
            }
            set
            {
                SetProperty(ref isTopoBackGroundImageVisible, value);
            }
        }

        public ImageSource TopoBackGroundImage
        {
            get
            {
                return topoBackGroundImage;
            }
            set
            {
                SetProperty(ref topoBackGroundImage, value);
            }
        }

        public bool ShareSelected { get; set; }
        public byte[] TakenImageBytes { get; set; }

        private string GetSendTypeWording(string obj)
        {
            string SendsCongratsWording = string.Empty;
            switch (Convert.ToString(obj))
            {
                case "Onsight":
                    SendsCongratsWording = "Boom! Nice ";
                    break;
                case "Flash":
                    SendsCongratsWording = "Cool ";
                    break;
                case "Redpoint":
                    SendsCongratsWording = "Awesome ";
                    break;
                case "Repeat":
                    SendsCongratsWording = "Good ";
                    break;
                case "Project":
                    SendsCongratsWording = "Project burn... ";
                    break;
                case "One hang":
                    SendsCongratsWording = "One-hang! ";
                    break;
            }
            return SendsCongratsWording;
        }

        public ICommand ShareCommand { get; set; }
        public ICommand ContinueCommand { get; }
        public bool WasShared { get; set; }
        private bool isFromAscentProcess;
        public bool IsFromAscentProcess { get { return isFromAscentProcess; } set { SetProperty(ref isFromAscentProcess, value); } }

        public async void ExecuteOnShareAfterInvalidating()
        {
            WasShared = true;
            var permisiionsNotGranted = await cameraService.CheckPermissions();
            if (permisiionsNotGranted)
            {
                userDialogs.HideLoading();
                return;
            }

            IsRunningTasks = false;
            IsDisplayMessage = false;
            filePath = await DependencyService.Get<ISavePicture>().SavePictureToDisk("Snap", AscentSummaryImagesBytes[Position]);
            //share image to social media
            userDialogs.HideLoading();
            var shareOption = DependencyService.Get<IShare>();
            shareOption.ShareImage(CommentText, filePath);
        }

        private async void ExecuteOnContinue()
        {
            if (IsBackButtonVisible)
            {
                BackCommand.Execute(null);
                return;
            }

            if (!routeData.route_image_id.HasValue || routeData.route_image_id == 0)
            {
                SaveImageAsync(ImageType.RouteImage, send.Ascent_Id, routeData.route_id, AscentImageWithNoStarts);
            }

            //save summary image
            SaveImageAsync(ImageType.SummaryImage, send.Ascent_Id, routeData.route_id, AscentSummaryImagesBytes[Position]);

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, currentSector);
            var url = NavigationExtention.CreateNavigationUrl(typeof(MainMasterDetailViewModel), typeof(MasterNavigationViewModel), typeof(CragSectorsViewModel), typeof(SectorRoutesViewModel));
            await navigationService.NavigateAsync(url, navigationParameters, false, true);
            IsBackButtonVisible = true;
        }

        private void ExecuteOnShare()
        {
            userDialogs.ShowLoading("Loading...");
            LayoutFlags = AbsoluteLayoutFlags.HeightProportional;
            ShareSelected = true;
            IsLogAscentSelected = true;
            ExecuteOnShareAfterInvalidating();
        }

        private SendModel send;
        private MapListModel currentSector;
        public string RouteNameWithGradeForPicture;

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            try
            {
                if (parameters.Count == 0)
                {
                    return;
                }

                if (parameters.ContainsKey(NavigationParametersConstants.IsFromAscentProcessNavigatedParameter)
                && (bool)parameters[NavigationParametersConstants.IsFromAscentProcessNavigatedParameter])
                {
                    IsBackButtonVisible = false;
                    IsFromAscentProcess = true;
                }

                var routeId = (int)parameters[NavigationParametersConstants.RouteIdParameter];
                this.send = (SendModel)parameters[NavigationParametersConstants.SendItemParameter];
                this.currentSector = (MapListModel)parameters[NavigationParametersConstants.SelectedSectorObjectParameter];
                var cragName = await sectorRepository.QueryAsync<SendsCragModel>($"SELECT DISTINCT T_ROUTE.route_name, T_SECTOR.sector_name, T_CRAG.crag_name FROM T_SECTOR INNER JOIN T_CRAG ON T_SECTOR.crag_id = T_CRAG.crag_id INNER JOIN T_ROUTE ON T_SECTOR.sector_id = T_ROUTE.sector_id  WHERE T_ROUTE.route_id = {routeId}");
                //SummaryImage = Cache.SelctedCurrentSector?.SectorImage;
                RouteId = routeId;
                if (send.Ascent_Type_Description == "Project Burn")
                {
                    send.Ascent_Type_Description = "Project";
                }
                else
                {
                    SendsTypeName = send.Ascent_Type_Description;
                }

                SendsTypeText = send.Ascent_Type_Description;
                SendClimbingStyle = send.Climbing_Angle;
                SendHoldType = send.Hold_Type;
                SendRouteCharacteristics = send.Route_Style;
                SendRating = send.Rating;
                foreach (var t in cragName)
                {
                    PageSubHeaderText = $"{t.sector_name}, {(t.crag_name).Trim()}";
                }
                if (cragName.Count == 0)
                {
                    PageSubHeaderText = $"{send.sector_name}, {(send.crag_name).Trim()}";
                }
                PageHeaderText = send.route_name;
                RouteNameWithGradeForPicture = $"{(send.route_name).ToUpper()} {send.Tech_Grade_Description}";

                routeData = await routeRepository.FindAsync(route => route.route_id == RouteId && route.is_enabled);

                //load data from ascentDetail
                if (routeData == null)
                {
                    PageHeaderText = $"{(send.route_name).ToUpper()} {send.Tech_Grade_Description}";
                    PageSubHeaderText = $"{send.sector_name.Trim()}, {send.crag_name.Trim()}";
                    CommentText = send.Comment;
                    var _grades =
                        (await techGradeRepository.GetAsync(tg => tg.grade_type_id == send.grade_type_id))
                        .OrderBy(tg => tg.sort_order)
                        .Select(names => names.tech_grade)
                        .ToList();
                    AscentGrades = _grades;
                    if (_grades.Count > 0)
                    {
                        SendsGrade = send.tech_grade; //grades[0];
                    }
                }
                else
                {
                    PageHeaderText = (routeData.route_name).ToUpper();
                    CommentText = send.Comment;
                    if (Cache.SelctedCurrentSector != null)
                    {
                        PageHeaderText = (routeData.route_name).ToUpper() + " " + routeData.tech_grade;
                    }
                    var _grades =
                            (await techGradeRepository.GetAsync(tg => tg.grade_type_id == send.grade_type_id))
                            .OrderBy(tg => tg.sort_order)
                            .Select(names => names.tech_grade)
                            .ToList();
                    AscentGrades = _grades;
                    if (_grades.Count > 0)
                    {
                        SendsGrade = routeData.tech_grade; //grades[0];
                    }
                    RouteNameWithGradeForPicture = $"{(routeData.route_name).ToUpper()} {send.Tech_Grade_Description}";
                }
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigation),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"routeId = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.RouteIdParameter])}, SendModel  = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.SendModelParameter])}"
                });
            }
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            UserDialogs.Instance.HideLoading();
        }

        private async void SaveImageAsync(ImageType imageType, int? ascentId, int? routeId, byte[] imageBytes)
        {					  
            var ascentImgData = new AscentImageModel
            {
                ImageBytes = imageBytes,
                ImageType = imageType
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
