using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using ExifLib;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.CheckForUpdateModels;
using SloperMobile.Model.SendsModels;
using Xamarin.Forms;
using XLabs.Platform.Device;

namespace SloperMobile.ViewModel.AscentViewModels
{
    public class AscentProcessViewModel : BaseViewModel
    {
        private readonly IRepository<AscentTypeTable> ascentTypeRepository;
        private readonly IRepository<TechGradeTable> techGradeRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<GradeTable> gradeRepository;
        private readonly IUserDialogs userDialogs;
        private readonly ICameraService cameraService;
        private readonly IDevice device;
        private readonly ISynchronizationManager synchronizationManager;

        private IEnumerable<ImageSource> _AscentProcessImgs;
        private ObservableCollection<byte[]> ascentProcessImagesBytes;
        private ObservableCollection<byte[]> ascentProcessImagesToShareBytes;
        private MapListModel currentSector;
        private List<string> grades;
        private List<string> listimgs;
        private string sendstypename;
        private string sendscongratswording;
        private DateTime sendsdate = DateTime.Now.Date;
        private string sendsgrade;
        private int sendrating;
        private string sendclimbingstyle;
        private string sendholdtype;
        private string sendroutecharacteristics;
        private string commenttext;
        private int routeid;
        private bool isEnable = true;
        private bool isdisplaymsg;
        private bool showbackheader = true;
        private bool shownobackheader = false;
        private bool isShare = false;
        private string filePath;

        private bool isdisplaysummaryimg = false;
        private bool isdisplaysummaryweb = true;

        private string commandtext = "Log Ascent";
        private string progressmsg = "Sending, please wait...";

        private string steepness_slab = "";
        private string steepness_vertical = "";
        private string steepness_overhanging = "";
        private string steepness_roof = "";

        private string hold_type_sloper = "";
        private string hold_type_crimps = "";
        private string hold_type_jugs = "";
        private string hold_type_pockets = "";
        private string hold_type_pinches = "";
        private string hold_type_jams = "";

        private string route_style_technical = "";
        private string route_style_sequential = "";
        private string route_style_powerful = "";
        private string route_style_sustained = "";
        private string route_style_one_move = "";
        private string route_style_exposed = "";

        private ImageSource summaryImage;
        private ImageSource ascentRatingImage;

        private ImageSource topangle;
        private ImageSource tophold;
        private ImageSource toproutechar;
        private Stream camera_image;
        private RouteTable routeData;
        private CragTable currentCrag;
        private AscentPostModel ascent;
        private bool isTopoBackGroundImageVisible;
        private ImageSource topoBackGroundImage;
        private int _position;

        public bool ShareSelected { get; set; }
        private byte[] imageBytes;
        public byte[] TakenImageBytes { get; set; }
        private IEnumerable newSource = new[] { 1, 1, 1, 1, 1, 1, 1, };
        private double scrollViewHeight;
        private double scrollViewWidth;
        private double canvasViewHeight;
        private double canvasViewWidth;
        private AbsoluteLayoutFlags layoutFlags = AbsoluteLayoutFlags.SizeProportional;
        private bool isLogAscentSelected;
        private bool isLogAscentShowHide;
        private ExifOrientation orientation;
        private SendModel sendModel;
        private bool isFromGallery;
        private int ascentId;

        public AscentProcessViewModel(
            INavigationService navigationService,
            IRepository<AscentTypeTable> ascentTypeRepository,
            IRepository<TechGradeTable> techGradeRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<SectorTable> sectorRepository,
            IRepository<GradeTable> gradeRepository,
            IRepository<AppSettingTable> appSettingsRepository,
            IUserDialogs userDialogs,
            ICameraService cameraService,
            ISynchronizationManager synchronizationManager,
            IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper)
            : base(navigationService, exceptionManager, httpHelper)
        {
            this.ascentTypeRepository = ascentTypeRepository;
            this.techGradeRepository = techGradeRepository;
            this.cragRepository = cragRepository;
            this.routeRepository = routeRepository;
            this.sectorRepository = sectorRepository;
            this.appSettingsRepository = appSettingsRepository;
            this.gradeRepository = gradeRepository;
            this.userDialogs = userDialogs;
            this.cameraService = cameraService;
            this.synchronizationManager = synchronizationManager;

            //ShowHideBackHeader = true;
            //ShowHideNoBackHeader = false;
            SendTypeCommand = new Command<string>(ExecuteOnSendType);
            SendTypeHoldCommand = new Command<string>(ExecuteOnSendHold);
            SendRouteCharaterCommand = new Command<string>(ExecuteOnRouteCharacteristics);
            SendRouteStyleCommand = new Command<string>(ExecuteOnRouteStyle);
            SendSummaryCommand = new Command(ExecuteOnSummary);
            ShareCommand = new Command(ExecuteOnShare);
            CameraClickCommand = new Command(ExecuteOnCameraClick);
            CommentClickCommand = new Command(ExecuteOnCommentClick);
            ascent = new AscentPostModel();
            device = XLabs.Ioc.Resolver.Resolve<IDevice>();
            _AscentProcessImgs = Enumerable.Empty<ImageSource>();
            ascentProcessImagesBytes = new ObservableCollection<byte[]>();
            IsBackButtonVisible = true;
            Offset = Common.Enumerators.Offsets.Header;
        }

        public new bool IsShowHeader => true;

        public bool IsLogAscentShowHide
        {
            get { return isLogAscentShowHide; }
            set { SetProperty(ref isLogAscentShowHide, value); }
        }

        public bool IsLogAscentSelected
        {
            get { return isLogAscentSelected; }
            set { SetProperty(ref isLogAscentSelected, value); }
        }

        public AbsoluteLayoutFlags LayoutFlags
        {
            get { return layoutFlags; }
            set { SetProperty(ref layoutFlags, value); }
        }

        public IEnumerable NewSOurce
        {
            get
            {
                return newSource;
            }
            set
            {
                SetProperty(ref newSource, value);
            }
        }

        public double ScrollViewHeight
        {
            get
            {
                return scrollViewHeight;
            }
            set { SetProperty(ref scrollViewHeight, value); }
        }

        public double ScrollViewWidth
        {
            get
            {
                return scrollViewWidth;
            }
            set { SetProperty(ref scrollViewWidth, value); }
        }

        public double CanvasViewHeight
        {
            get
            {
                return canvasViewHeight;
            }
            set { SetProperty(ref canvasViewHeight, value); }
        }

        public double CanvasViewWidth
        {
            get
            {
                return canvasViewWidth;
            }
            set { SetProperty(ref canvasViewWidth, value); }
        }

        public IEnumerable<ImageSource> AscentProcessImgs
        {
            get { return _AscentProcessImgs; }
            set { SetProperty(ref _AscentProcessImgs, value); }
        }

        public ObservableCollection<byte[]> AscentProcessImagesBytes
        {
            get { return ascentProcessImagesBytes; }
            set { SetProperty(ref ascentProcessImagesBytes, value); }
        }

        public ObservableCollection<byte[]> AscentProcessImagesToShareBytes
        {
            get { return ascentProcessImagesToShareBytes; }
            set { SetProperty(ref ascentProcessImagesToShareBytes, value); }
        }

        public int Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public bool ShowHideBackHeader
        {
            get { return showbackheader; }
            set { showbackheader = value; RaisePropertyChanged(); }
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

        public string SendsTypeName
        {
            get { return sendstypename; }
            set { sendstypename = value; RaisePropertyChanged(); }
        }

        public string SendsCongratsWording
        {
            get { return sendscongratswording; }
            set { sendscongratswording = value; RaisePropertyChanged(); }
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

        public int SendRating
        {
            get { return sendrating; }
            set { sendrating = value; RaisePropertyChanged(); }
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
        public ImageSource SummaryImage
        {
            get { return summaryImage; }
            set { SetProperty(ref summaryImage, value); }
        }

        public ImageSource AcentRatingImage
        {
            get { return ascentRatingImage; }
            set { SetProperty(ref ascentRatingImage, value); }
        }

        public Stream CameraImage
        {
            get { return camera_image; }
            set { camera_image = value; RaisePropertyChanged(); }
        }

        public string CommandText
        {
            get { return commandtext; }
            set { commandtext = value; RaisePropertyChanged(); }
        }

        public string CommentText
        {
            get { return commenttext; }
            set { commenttext = value; RaisePropertyChanged(); }
        }

        public string ProgressMsg
        {
            get { return progressmsg; }
            set { progressmsg = value; RaisePropertyChanged(); }
        }

        public bool IsButtonEnable
        {
            get { return isEnable; }
            set
            {
                isEnable = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDisplayMessage
        {
            get { return isdisplaymsg; }
            set { isdisplaymsg = value; RaisePropertyChanged(); }
        }

        public bool IsDisplaySummaryWeb
        {
            get { return isdisplaysummaryweb; }
            set { isdisplaysummaryweb = value; RaisePropertyChanged(); }
        }

        public int RouteId
        {
            get { return routeid; }
            set { routeid = value; RaisePropertyChanged(); }
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

        public ImageSource TopAngle
        {
            get
            {
                if (topangle != null)
                {
                    return topangle;
                }
                else
                {
                    return topangle = ImageSource.FromFile(GetAngleResourceName(""));
                }
            }
            set { topangle = value; RaisePropertyChanged(); }
        }

        public ImageSource TopHold
        {
            get
            {
                if (tophold != null)
                {
                    return tophold;
                }
                else
                {
                    return tophold = ImageSource.FromFile(GetHoldResourceName(""));
                }

            }
            set { tophold = value; RaisePropertyChanged(); }
        }

        public ImageSource TopRouteChar
        {
            get
            {
                if (toproutechar != null)
                {
                    return toproutechar;
                }
                else
                {
                    return toproutechar = ImageSource.FromFile(GetRouteResourceName(""));
                }

            }
            set { toproutechar = value; RaisePropertyChanged(); }
        }

        public ICommand SendTypeCommand { get; set; }
        public ICommand SendTypeHoldCommand { get; set; }
        public ICommand SendRouteCharaterCommand { get; set; }
        public ICommand SendRouteStyleCommand { get; set; }
        public ICommand SendSummaryCommand { get; set; }
        public ICommand ShareCommand { get; set; }
        public ICommand CameraClickCommand { get; set; }
        public ICommand CommentClickCommand { get; set; }

        private void ExecuteOnSendType(string obj)
        {
            SendsTypeName = obj;
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
                    SendsCongratsWording = "One hang! ";
                    break;
            }
        }

        private void ExecuteOnRouteCharacteristics(string obj)
        {
            var routecharacteristics = "";
            string routes = "";
            if (obj == "1")
            {
                if (route_style_technical == "")
                {
                    route_style_technical = "1";
                }
                else
                {
                    route_style_technical = "";
                }
            }


            if (obj == "2")
            {
                if (route_style_sequential == "")
                {
                    route_style_sequential = "2";
                }
                else
                {
                    route_style_sequential = "";
                }
            }


            if (obj == "4")
            {
                if (route_style_powerful == "")
                {
                    route_style_powerful = "4";
                }
                else
                {
                    route_style_powerful = "";
                }
            }


            if (obj == "8")
            {
                if (route_style_sustained == "")
                {
                    route_style_sustained = "8";
                }
                else
                {
                    route_style_sustained = "";
                }
            }


            if (obj == "16")
            {
                if (route_style_one_move == "")
                {
                    route_style_one_move = "16";
                }
                else
                {
                    route_style_one_move = "";
                }
            }

            if (obj == "32")
            {
                if (route_style_exposed == "")
                {
                    route_style_exposed = "32";
                    // routes = "all";
                }
                else
                {
                    route_style_exposed = "";

                }
            }

            string[] characteristics = { route_style_technical, route_style_sequential, route_style_powerful, route_style_sustained, route_style_one_move, route_style_exposed };
            foreach (string str in characteristics)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    routecharacteristics += str + ",";
                    routes = str;


                }
            }

            if (!string.IsNullOrEmpty(routecharacteristics))
            {
                SendRouteCharacteristics = routecharacteristics.TrimEnd(',');
            }
            else
            {
                SendRouteCharacteristics = routecharacteristics;
            }

            TopRouteChar = ImageSource.FromFile(GetRouteResourceName(routes));
        }

        private void ExecuteOnSendHold(string obj)
        {
            var holdingstyle = "";
            if (obj == "1")
            {
                if (hold_type_sloper == "")
                {
                    hold_type_sloper = "1";
                }
                else
                {
                    hold_type_sloper = "";
                }
            }


            if (obj == "2")
            {
                if (hold_type_crimps == "")
                {
                    hold_type_crimps = "2";
                }
                else
                {
                    hold_type_crimps = "";
                }
            }


            if (obj == "4")
            {
                if (hold_type_jugs == "")
                {
                    hold_type_jugs = "4";
                }
                else
                {
                    hold_type_jugs = "";
                }
            }


            if (obj == "8")
            {
                if (hold_type_pockets == "")
                {
                    hold_type_pockets = "8";
                }
                else
                {
                    hold_type_pockets = "";
                }
            }

            if (obj == "16")
            {
                if (hold_type_pinches == "")
                {
                    hold_type_pinches = "16";
                }
                else
                {
                    hold_type_pinches = "";
                }
            }

            if (obj == "32")
            {
                if (hold_type_jams == "")
                {
                    hold_type_jams = "32";
                }
                else
                {
                    hold_type_jams = "";
                }
            }

            string tophold = "";
            string[] holdstyles = { hold_type_sloper, hold_type_crimps, hold_type_jugs, hold_type_pockets, hold_type_pinches, hold_type_jams };
            foreach (string str in holdstyles)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    holdingstyle += str + ",";
                    tophold = str;
                }
            }

            if (!string.IsNullOrEmpty(holdingstyle))
            {
                SendHoldType = holdingstyle.TrimEnd(',');
            }
            else
            {
                SendHoldType = holdingstyle;
            }

            TopHold = ImageSource.FromFile(GetHoldResourceName(tophold));
        }

        private void ExecuteOnRouteStyle(string obj)
        {
            var climbingstyles = "";
            if (Convert.ToString(obj) == "1")
            {
                if (steepness_slab == "")
                {
                    steepness_slab = "1";
                }
                else
                {
                    steepness_slab = "";
                }
            }


            if (Convert.ToString(obj) == "2")
            {
                if (steepness_vertical == "")
                {
                    steepness_vertical = "2";
                }
                else
                {
                    steepness_vertical = "";
                }
            }


            if (Convert.ToString(obj) == "4")
            {
                if (steepness_overhanging == "")
                {
                    steepness_overhanging = "4";
                }
                else
                {
                    steepness_overhanging = "";
                }
            }


            if (Convert.ToString(obj) == "8")
            {
                if (steepness_roof == "")
                {
                    steepness_roof = "8";
                }
                else
                {
                    steepness_roof = "";
                }
            }

            string[] climbstyles = { steepness_slab, steepness_vertical, steepness_overhanging, steepness_roof };
            string topangle = "";
            foreach (string str in climbstyles)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    climbingstyles += str + ",";
                    if (str != "")
                    {
                        topangle = str;
                    }
                }
            }

            if (!string.IsNullOrEmpty(climbingstyles))
            {
                SendClimbingStyle = climbingstyles.TrimEnd(',');
            }
            else
            {
                SendClimbingStyle = climbingstyles;
            }

            TopAngle = ImageSource.FromFile(GetAngleResourceName(topangle));

        }

        private async void ExecuteOnCameraClick()
        {
            var action = await userDialogs.ActionSheetAsync("Choose Ascent Image", "Cancel", null, null, "Take photo", "Pick a file");
            if (action == "Take photo")
            {
                var mediaFile = await cameraService.TakePhotoAsync("Ascent_", "AscentProcess");
                if (mediaFile == null)
                {
                    return;
                }

                filePath = mediaFile.Path;
                IsDisplaySummaryWeb = false;
                using (var stream = mediaFile.GetStream())
                {
                    TakenImageBytes = ReadStreamByte(stream);
                    ascent.ImageData = TakenImageBytes != null ? Convert.ToBase64String(TakenImageBytes) : string.Empty;
                    var picInfo = ExifReader.ReadJpeg(new MemoryStream(TakenImageBytes));
                    orientation = Device.RuntimePlatform == Device.Android ? ExifOrientation.Undefined : picInfo.Orientation;
                }

                AcentRatingImage = ImageSource.FromStream(() => new MemoryStream(TakenImageBytes));
            }

            if (action == "Pick a file")
            {
                var mediaFile = await cameraService.PickFileAsync();
                if (mediaFile == null)
                {
                    return;
                }

                filePath = mediaFile.Path;
                IsDisplaySummaryWeb = false;
                using (var stream = mediaFile.GetStream())
                {
                    TakenImageBytes = ReadStreamByte(stream);
                    ascent.ImageData = TakenImageBytes != null ? Convert.ToBase64String(TakenImageBytes) : string.Empty;
                    var picInfo = ExifReader.ReadJpeg(new MemoryStream(TakenImageBytes));
                    orientation = picInfo.Orientation;
                }

                isFromGallery = true;
                AcentRatingImage = ImageSource.FromStream(() => new MemoryStream(TakenImageBytes));
            }
        }

        private async void ExecuteOnCommentClick()
        {
            PromptResult result = await userDialogs.PromptAsync(new PromptConfig
            {
                Title = "Comments",
                InputType = InputType.Name,
                Message = "Give us your thoughts on this climb",
                Text = CommentText,
                MaxLength = 250,
                Placeholder = "type here",
                OkText = "Ok",
                IsCancellable = true,
                CancelText = "Cancel"
            });

            if (result.Ok)
            {
                CommentText = result.Text;
            }
        }

        public async Task LogAscent()
        {
            #region Collecting Ascent Data

            ascent.ascent_date = Convert.ToDateTime(SendsDate.Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureInfo.CurrentCulture));
            ascent.route_id = RouteId;
            ascent.ascent_type_id = SendsTypeName != null
                ? (await ascentTypeRepository.FindAsync(ascentType => ascentType.ascent_type_description == SendsTypeName))
                  ?.ascent_type_id ?? 0
                : default(int);
            ascent.climbing_angle = "";
            if (SendClimbingStyle != null)
            {
                if (SendClimbingStyle.Contains(","))
                {
                    string[] sarr1 = SendClimbingStyle.Split(',');
                    int climbbitmask = 0;
                    foreach (string s in sarr1)
                    {
                        climbbitmask += Convert.ToInt32(s);
                    }
                    ascent.climbing_angle_value = climbbitmask.ToString();
                }
                else
                {
                    ascent.climbing_angle_value = SendClimbingStyle;
                }
            }
            else
            {
                ascent.climbing_angle_value = "0";
            }
            ascent.comment = CommentText;
            ascent.grade_id = routeData.grade_type_id;
            ascent.hold_type = "";

            if (SendHoldType != null)
            {
                if (SendHoldType.Contains(","))
                {
                    string[] sarr2 = SendHoldType.Split(',');
                    int holdbitmask = 0;
                    foreach (string s in sarr2)
                    {
                        holdbitmask += Convert.ToInt32(s);
                    }
                    ascent.hold_type_value = holdbitmask.ToString();
                }
                else
                {
                    ascent.hold_type_value = SendHoldType;
                }
            }
            else
            {
                ascent.hold_type_value = "0";
            }

            //ascent.ImageData = imageBytes != null ? Convert.ToBase64String(imageBytes) : string.Empty;
            ascent.ImageName = "";
            ascent.photo = "";
            ascent.rating = SendRating.ToString();
            ascent.route_style = "";

            if (SendRouteCharacteristics != null)
            {
                if (SendRouteCharacteristics.Contains(","))
                {
                    string[] sarr3 = SendRouteCharacteristics.Split(',');
                    int routebitmask = 0;
                    foreach (string s in sarr3)
                    {
                        routebitmask += Convert.ToInt32(s);
                    }
                    ascent.route_style_value = routebitmask.ToString();
                }
                else
                {
                    ascent.route_style_value = SendRouteCharacteristics;
                }
            }
            else
            {
                ascent.route_style_value = "0";
            }
            ascent.route_type_id = routeData.route_type_id;

            //get grade that they used in grade thoughts
            var grade = await techGradeRepository.FindAsync(
                tgrade => tgrade.grade_type_id == routeData.grade_type_id && tgrade.tech_grade == SendsGrade);
            ascent.tech_grade_id = grade == null ? default(int) : grade.tech_grade_id;

            ascent.video = "";
            ascent.Orientation = orientation;
            #endregion

            var response = await synchronizationManager.SendAscentDataAsync(ascent);
            //if (response != null && response.id != null)
            if (response != null)
            {
                ascentId = Convert.ToInt32(response.id);
                if (!string.IsNullOrEmpty(response.climbingDays))
                {
                    Settings.ClimbingDays = Convert.ToInt32(response.climbingDays);
                }

                // CALL GetConsensusSectors
                //================= Added by Sandeep on 23-Jun-2017=============                                            
                var consensusSectorsObj = await HttpGetConsensusSectors();
                if (!consensusSectorsObj.ValidateResponse()) //consensusSectorsObj?.Count > 0
                    return;

                var allFromDb = await sectorRepository.GetAsync();
                if (allFromDb != null)
                {
                    //Left outer join to select only those, that do exist in the database
                    var updateIntoDb = consensusSectorsObj
                        .Result.Join(allFromDb, firstItem => firstItem.sector_id, secondItem => secondItem.sector_id,
                            (firstItem, secondItem) =>
                            {
                                secondItem.top2_steepness = firstItem.top2_steepness;
                                return secondItem;
                            });

                    var resultUpdated = await sectorRepository.UpdateAllAsync(updateIntoDb);
                }
                else
                {
                    await sectorRepository.InsertAllAsync(consensusSectorsObj.Result);
                }

                //=====================================================================

                // CALL GetConsensusRoutes
                //================= Added by Sandeep on 23-Jun-2017=============                    
                var consensusRoutesObj = await HttpGetConsensusRoutes();
                if (!consensusRoutesObj.ValidateResponse()) //consensusRoutesObj?.Count > 0
                    return;

                var allRoutesFromDb = await routeRepository.GetAsync();
                if (allRoutesFromDb != null)
                {
                    //Left outer join to select only those, that do exist in the database
                    var updateIntoDb = consensusRoutesObj
                        .Result.Join(allRoutesFromDb, firstItem => firstItem.route_id, secondItem => secondItem.route_id,
                            (firstItem, secondItem) =>
                            {
                                secondItem.route_style_top_1 = firstItem.route_style_top_1;
                                secondItem.hold_type_top_1 = firstItem.hold_type_top_1;
                                secondItem.angles_top_1 = firstItem.angles_top_1;
                                secondItem.rating = firstItem.rating;
                                return secondItem;
                            });

                    var resultUpdated = await routeRepository.UpdateAllAsync(updateIntoDb);
                }
                else
                {
                    await routeRepository.InsertAllAsync(consensusRoutesObj.Result);
                }
                //=====================================================================

                if (TakenImageBytes != null)
                {
                    var ascentPlainImage = new AscentImageModel
                    {
                        AscentId = ascentId,
                        ImageBytes = TakenImageBytes,
                        ImageOrientation = orientation,
                        appId = Convert.ToInt32(AppSetting.APP_ID),
                        FiveStarAscentCheck = Convert.ToInt32(AppSetting.FiveStarAscentCheck)
                    };

                    await httpHelper.PostAsync<int>(ApiUrls.Url_M_SaveImage, ascentPlainImage);
                }

                ProgressMsg = "Ascent saved successfully.";
                IsRunningTasks = false;
            }
            else
            {
                userDialogs.HideLoading();
                await userDialogs.AlertAsync("No internet connection. Data will be synchronized when connect to internet.", "Network Error", "Continue");
                userDialogs.ShowLoading("Logging Ascent...");
                ProgressMsg = "Data saved locally!";
                IsRunningTasks = false;
            }
        }

        private async void ExecuteOnSummary()
        {
            if (CommandText == "Log Ascent" && IsButtonEnable)
            {
                userDialogs.ShowLoading("Logging Ascent...");

                IsButtonEnable = false;
                IsLogAscentShowHide = true;
                //ShowHideNoBackHeader = true;
                //ShowHideBackHeader = false;
                IsBackButtonVisible = false;
                IsLogAscentSelected = true;

                await LogAscent();
                ExecuteOnShare();
            }
        }

        private async void ExecuteOnShare()
        {
            try
            {
                LayoutFlags = AbsoluteLayoutFlags.HeightProportional;
                var navigationParameters = new NavigationParameters();
                sendModel = new SendModel();
                sendModel.Ascent_Id = ascentId;
                sendModel.Date_Climbed = Convert.ToDateTime(SendsDate.Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureInfo.CurrentCulture)); ;
                sendModel.Route_Id = RouteId;
                sendModel.Ascent_Type_Id = SendsTypeName != null
                    ? (await ascentTypeRepository.FindAsync(sendModelType => sendModelType.ascent_type_description == SendsTypeName))
                      ?.ascent_type_id ?? 0
                    : default(int);
                sendModel.Ascent_Type_Description = SendsTypeName;
                if (SendClimbingStyle != null)
                {
                    sendModel.Climbing_Angle = SendClimbingStyle;
                }
                else
                {
                    sendModel.Climbing_Angle = "0";
                }
                sendModel.Comment = CommentText;
                sendModel.Grade_Id = routeData.grade_type_id;

                if (SendHoldType != null)
                {
                    sendModel.Hold_Type = SendHoldType;
                }
                else
                {
                    sendModel.Hold_Type = "0";
                }

                sendModel.Rating = SendRating;
                if (SendRouteCharacteristics != null)
                {
                    sendModel.Route_Style = SendRouteCharacteristics;
                }
                else
                {
                    sendModel.Route_Style = "0";
                }

                sendModel.Route_Type_Id = routeData.route_type_id;
                var grade = await techGradeRepository.FindAsync(
                    tgrade => tgrade.grade_type_id == routeData.grade_type_id && tgrade.tech_grade == SendsGrade);
                sendModel.Tech_Grade_Id = grade == null ? default(int) : grade.tech_grade_id;
                sendModel.route_name = routeData.route_name;
                sendModel.Tech_Grade_Description = SendsGrade;
                sendModel.sector_id = currentSector.SectorId;
                navigationParameters.Add(NavigationParametersConstants.RouteIdParameter, RouteId);
                navigationParameters.Add(NavigationParametersConstants.SendItemParameter, sendModel);
                navigationParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, currentSector);
                navigationParameters.Add(NavigationParametersConstants.TakenPhotoImageBytesParameter, TakenImageBytes);
                navigationParameters.Add(NavigationParametersConstants.IsFromAscentProcessNavigatedParameter, true);
                navigationParameters.Add(NavigationParametersConstants.ExifOrientationParameter, orientation);
                navigationParameters.Add(NavigationParametersConstants.IsFromGalleryParameter, isFromGallery);
                await navigationService.NavigateAsync<AscentSummaryViewModel>(navigationParameters);
                TakenImageBytes = null;
                currentSector = null;
                sendModel = null;
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.ExecuteOnShare),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"routeId = {RouteId}, SendModel  = {Newtonsoft.Json.JsonConvert.SerializeObject(sendModel)}, currentSector = {Newtonsoft.Json.JsonConvert.SerializeObject(currentSector)}"
                });
            }
        }

        public async Task<OperationResult<IEnumerable<SectorTable>>> HttpGetConsensusSectors()
        {
            //httpClinetHelper.ChangeTokens(ApiUrls.Url_M_GetConsensusSectors, Settings.AccessToken);
            var consensusSectorsobj = new ConsensusModel
            {
                app_id = Convert.ToInt32(AppSetting.APP_ID),
                app_date_last_updated = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]")
            };
            //string consensusSectorsjson = JsonConvert.SerializeObject(consensusSectorsobj);

            var response = await httpHelper.PostAsync<IEnumerable<SectorTable>>(ApiUrls.Url_M_GetConsensusSectors, consensusSectorsobj);
            return response;
        }

        public async Task<OperationResult<IEnumerable<RouteTable>>> HttpGetConsensusRoutes()
        {
            // httpClinetHelper.ChangeTokens(ApiUrls.Url_M_GetConsensusRoutes, Settings.AccessToken);
            var consensusRoutesobj = new ConsensusModel
            {
                app_id = Convert.ToInt32(AppSetting.APP_ID),
                app_date_last_updated = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]")
            };
            //var troute_response = await httpClinetHelper.Post<List<RouteTable>>(consensusRoutesjson);
            //return troute_response;

            var response = await httpHelper.PostAsync<IEnumerable<RouteTable>>(ApiUrls.Url_M_GetConsensusRoutes, consensusRoutesobj);
            return response;
        }

        public string GetAngleResourceName(string steep)
        {
            string resource = "";
            switch (steep)
            {
                case "1":
                    resource = "icon_steepness_1_slab_border_80x80";
                    break;
                case "2":
                    resource = "icon_steepness_2_vertical_border_80x80";
                    break;
                case "4":
                    resource = "icon_steepness_4_overhanging_border_80x80";
                    break;
                case "8":
                    resource = "icon_steepness_8_roof_border_80x80";
                    break;
            }
            return resource;
        }

        public string GetHoldResourceName(string hold)
        {
            string resource = "";
            switch (hold)
            {
                case "1":
                    resource = "icon_hold_type_1_slopers_text_58x92";
                    break;
                case "2":
                    resource = "icon_hold_type_2_crimps_text_41x68";
                    break;

                case "4":
                    resource = "icon_hold_type_4_jugs_text_58x74";
                    break;
                case "8":
                    resource = "icon_hold_type_8_pockets_text_63x94";
                    break;
                case "16":
                    resource = "icon_hold_type_16_pinches_text_73x83";
                    break;
                case "32":
                    resource = "icon_hold_type_32_jams_text_57x86";
                    break;

            }
            return resource;
        }

        public string GetRouteResourceName(string route)
        {
            string resource = "";
            switch (route)
            {
                case "1":
                    resource = "icon_route_style_1_technical_border_80x80";
                    break;
                case "2":
                    resource = "icon_route_style_2_sequential_border_80x80";
                    break;
                case "4":
                    resource = "icon_route_style_4_powerful_border_80x80";
                    break;
                case "8":
                    resource = "icon_route_style_8_sustained_border_80x80";
                    break;
                case "16":
                    resource = "icon_route_style_16_one_move_border_80x80";
                    break;
                case "32":
                    resource = "icon_route_style_32_exposed_border_80x80";
                    break;
            }
            return resource;
        }

        private static byte[] ReadStreamByte(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            try
            {
                base.OnNavigatedTo(parameters);
                var cragid = Convert.ToInt32(Settings.ActiveCrag);
                currentCrag = await cragRepository.FindAsync(crag => crag.crag_id == cragid);
                SummaryImage = Cache.SelctedCurrentSector?.SectorImage;
                routeData = await routeRepository.FindAsync(route => route.route_id == RouteId && route.is_enabled);
                var grades =
                    (await techGradeRepository.GetAsync(tgrade => tgrade.grade_type_id == routeData.grade_type_id))
                    .OrderBy(tgrade => tgrade.sort_order)
                    .Select(tgradeName => tgradeName.tech_grade)
                    .ToList();
                AscentGrades = grades;
                if (grades.Count > 0)
                {
                    SendsGrade = routeData.tech_grade; //grades[0];
                }

                PageHeaderText = (routeData.route_name).ToUpper() + " " + routeData.tech_grade;
                PageSubHeaderText = Cache.SelctedCurrentSector.SectorName + ", " + (currentCrag.crag_name).Trim();
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatedTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"routeId = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.RouteIdParameter])}, SendModel  = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.SendModelParameter])}"
                });
            }
        }

        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            try
            {
                var currentRouteId = (int)parameters[NavigationParametersConstants.CurrentRouteIdParameter];
                currentSector = (MapListModel)parameters[NavigationParametersConstants.CurrentSectorParameter];
                RouteId = currentRouteId;

                //default send type to Onsight
                SendsTypeName = "Onsight";
                ExecuteOnSendType("Onsight");
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigation),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"routeId = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.CurrentRouteIdParameter])}, currentSector  = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.CurrentSectorParameter])}"
                });
            }
        }
    }
}