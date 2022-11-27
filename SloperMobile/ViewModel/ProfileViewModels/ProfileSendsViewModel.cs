using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.CheckForUpdateModels;
using SloperMobile.Model.SendsModels;
using SloperMobile.ViewModel.AscentViewModels;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.ProfileViewModels
{
    //TODO: Refactor. Too many unused parts
    public class ProfileSendsViewModel : BaseViewModel
    {
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<GradeTable> gradeRepository;
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IUserDialogs userDialogs;

        int userId;

        public ProfileSendsViewModel(
            INavigationService navigationService,
            IRepository<TopoTable> topoRepository,
            IRepository<SectorTable> sectorRepository,
            IRepository<GradeTable> gradeRepository,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<RouteTable> routeRepository,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IHttpHelper httpHelper) : base(navigationService, exceptionManager, httpHelper)
        {
            this.topoRepository = topoRepository;
            this.sectorRepository = sectorRepository;
            this.gradeRepository = gradeRepository;
            this.appSettingsRepository = appSettingsRepository;
            this.routeRepository = routeRepository;
            this.userDialogs = userDialogs;
            //sectorimageList = new ObservableCollection<MapListModel>();

            GaugeThickness = ((Application.Current.MainPage.Width) / (40));
            CenterGaugeMargin = 0;

            RimThickness = (Device.RuntimePlatform == Device.iOS) ? GaugeThickness * 2 : GaugeThickness;

            //GaugeFontSize = ((Application.Current.MainPage.Width) / (20));
            //GaugeFontSizeSub = GaugeFontSize * .7;

            //PageHeaderText = "PROFILE";
            //Offset = Common.Enumerators.Offsets.Header;
        }

        private int centergaugemargin;
        public int CenterGaugeMargin
        {
            get { return centergaugemargin; }
            set { centergaugemargin = value; RaisePropertyChanged(); }
        }

        private double rimThickness;
        public double RimThickness
        {
            get { return rimThickness; }
            set { rimThickness = value; RaisePropertyChanged(); }
        }

        private double gaugethickness;
        public double GaugeThickness
        {
            get { return gaugethickness; }
            set { gaugethickness = value; RaisePropertyChanged(); }
        }

        //private double gaugefontsize;
        //public double GaugeFontSize
        //{
        //    get { return gaugefontsize; }
        //    set { gaugefontsize = value; RaisePropertyChanged(); }
        //}

        //private double gaugefontsizesub;
        //public double GaugeFontSizeSub
        //{
        //    get { return gaugefontsizesub; }
        //    set { gaugefontsizesub = value; RaisePropertyChanged(); }
        //}

        private ObservableCollection<SendModel> sendsList;
        //private ObservableCollection<TickListModel> ticklistsList;

        public ObservableCollection<SendModel> SendsList
        {
            get { return sendsList; }
            set
            {
                sendsList = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ShowEmptyOverlay));
            }
        }

        //public ObservableCollection<TickListModel> TickListsList
        //{
        //    get { return ticklistsList; }
        //    set { ticklistsList = value; RaisePropertyChanged(); }
        //}

        private int onsight;

        public int Onsight
        {
            get { return onsight; }
            set { onsight = value; RaisePropertyChanged(); }
        }

        private int redpoint;

        public int Redpoint
        {
            get { return redpoint; }
            set { redpoint = value; RaisePropertyChanged(); }
        }

        private int projects;

        public int Projects
        {
            get { return projects; }
            set { projects = value; RaisePropertyChanged(); }
        }

        private int projectsGauge;

        public int ProjectsGauge
        {
            get { return projectsGauge; }
            set { projectsGauge = value; RaisePropertyChanged(); }
        }


        private string tabBackgroundColor;

        public string TabBackgroundColor
        {
            get { return tabBackgroundColor; }
            set { tabBackgroundColor = value; RaisePropertyChanged(); }
        }

        private DateTime date_Created;
        public DateTime Date_Created
        {
            get { return date_Created; }
            set
            {
                date_Created = value;
                DateCreated = value.ToString("MM/dd/yy");
            }
        }
        private string dateCreated;

        public string DateCreated
        {
            get { return dateCreated; }
            set
            {
                dateCreated = value; RaisePropertyChanged();

            }
        }

        private string routeName;
        public string route_name
        {
            get => routeName;
            set
            {
                routeName = value; RaisePropertyChanged();
            }
        }

        public bool ShowEmptyOverlay
        {
            get => (SendsList == null || SendsList.Count == 0) && !IsRunningTasks;
        }


        private string gradename;
        public string grade_name
        {
            get { return gradename; }
            set { gradename = value; RaisePropertyChanged(); }
        }

        public bool IsMe { get => userId == 0 || userId == Settings.UserID; }

        public Command OnListTappedCommand { get => new Command<SendModel>(ShowDetails); }

        public async void OnPagePrepration(int id)
        {
            userDialogs.ShowLoading();
            userId = id;
            if (GuestHelper.IsGuest)
                SendsList = new ObservableCollection<SendModel>();
            else
                await InvokeServiceGetAscentData();
            userDialogs.HideLoading();
        }

        //private void ExecuteOnTabSelection(object obj)
        //{

        //}

        private void SetChartValue()
        {
            Onsight = 0;
            Redpoint = 0;
            Projects = 0;

            int _onsight = 0, _redpoint = 0, _project = 0;
            if (SendsList.Count > 0)
            {
                foreach (var item in SendsList)
                {
                    if (item.Ascent_Type_Id == 1)
                        _onsight++;
                    else if (item.Ascent_Type_Id == 3)
                        _redpoint++;
                    else if ((item.Ascent_Type_Id == 6 || item.Ascent_Type_Id == 5) && !SendsList.Any(s => s.Route_Id == item.Route_Id && s.Ascent_Type_Id == 3))
                        _project++;

                }
                Onsight = (int)Math.Round((double)(100 * _onsight) / SendsList.Count);
                Redpoint = (int)Math.Round((double)(100 * _redpoint) / SendsList.Count);
                Projects = _project;
                ProjectsGauge = (int)Math.Round((double)(100 * _project) / SendsList.Count);
            }
        }

        private async Task InvokeServiceGetAscentData()
        {
            try
            {
                var sendsobj = new SendsModel
                {
                    app_id = Convert.ToInt32(AppSetting.APP_ID),
                    start_date = "20160101",
                    end_date = "20300101"
                };
                if (userId != 0)
                    sendsobj.user_id = userId;
                    
                var response = await httpHelper.PostAsync<IEnumerable<SendModel>>(ApiUrls.Url_M_GetAscents, sendsobj);

                if (response.ValidateResponse())
                {
                    if (response.Result.Any())
                    {
                        SendsList = new ObservableCollection<SendModel>(response.Result);
                        SetChartValue();
                    }
                    else
                    {
                        SendsList = new ObservableCollection<SendModel>();
                        SetChartValue();
                    }
                }

            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.InvokeServiceGetAscentData),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message
                });
            }
        }

        //     private async Task InvokeServiceGetTickListData()
        //     {
        //         try
        //         {
        //             httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_TickList_GetTickList, AppSetting.APP_ID), Settings.AccessToken);
        //             var response = await httpClinetHelper.Get<TickListModel>();
        //             if (response.Count > 0)
        //             {
        //                 TickListsList = new ObservableCollection<TickListModel>(response);
        //                 if (TickListsList.Count > 0)
        //                 {
        //                     foreach (var item in TickListsList)
        //                     {
        //                         route_name = WebUtility.HtmlDecode(item.route_name);
        //                         grade_name = item.grade_name;
        //                         DateCreated = item.Date_Created.ToString("MM/dd/yy");
        //                     }
        //                 }
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //	await exceptionManager.LogException(new ExceptionTable
        //	{
        //		Method = nameof(this.InvokeServiceGetTickListData),
        //		Page = this.GetType().Name,
        //		StackTrace = ex.StackTrace,
        //		Exception = ex.Message,
        //	});
        //}
        //}

        public class AscentVM
        {
            public string climbingDays { get; set; }
        }

        private async void GetAscentDetails(int ascent_Id)
        {
            SendsModel sendsobj = new SendsModel
            {
                Ascent_Id = ascent_Id
            };

            var response = await httpHelper.PostAsync<IEnumerable<SendModel>>(ApiUrls.Url_M_GetAscentDetails, sendsobj);


            if (response.ValidateResponse() && response.Result.Any())
            {
                var item = response.Result.Single();

                var navigationParameters = new NavigationParameters();
                navigationParameters.Add(NavigationParametersConstants.RouteIdParameter, item.Route_Id);
                navigationParameters.Add(NavigationParametersConstants.SendItemParameter, item);
                navigationParameters.Add(NavigationParametersConstants.ExifOrientationParameter, item.Orientation);
                navigationParameters.Add(NavigationParametersConstants.IsFromGalleryParameter, true);
                if (!string.IsNullOrEmpty(item.userSelectedImage))
                {
                    navigationParameters.Add(NavigationParametersConstants.TakenPhotoImageBytesParameter, Convert.FromBase64String(item.userSelectedImage.Split(',')[1]));
                }

                await navigationService.NavigateAsync<AscentSummaryViewModel>(navigationParameters);
            }
        }

        //public void OnNavigatedFrom(NavigationParameters parameters)
        //{
        //}

        //public void OnNavigatedTo(NavigationParameters parameters)
        //{
        //    Device.BeginInvokeOnMainThread(() => UserDialogs.Instance.HideLoading());
        //    App.IsNavigating = false;
        //}

        //     private async void LoadSectorImages()
        //     {
        //         try
        //         {
        //             var cragid = Settings.ActiveCrag;
        //             var item = (await sectorRepository.GetAsync(ts1 => ts1.crag_id == cragid && ts1.is_enabled)).Select(ts => ts.sector_id);
        //             var secimglist = await topoRepository.GetAsync(tp => item.Contains(tp.sector_id));

        //             var sector_images = secimglist;
        //             foreach (var sector in sector_images)
        //             {
        //                 var topoimg = JsonConvert.DeserializeObject<List<TopoImageResponseModel>>(sector.topo_json);

        //                 if ((topoimg?.Count > 0) && !string.IsNullOrEmpty(topoimg[0].image.data))
        //                 {
        //                     string strimg64 = topoimg[0].image.data.Split(',')[1];
        //                     if (!string.IsNullOrEmpty(strimg64))
        //                     {
        //                         MapListModel objSec = new MapListModel();
        //                         objSec.IsCragOrDefaultImageCount = 0;
        //                         objSec.SectorId = sector.sector_id;
        //                         byte[] imageBytes = Convert.FromBase64String(strimg64);
        //                         objSec.SectorImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        //                         SectorTable tsec = await sectorRepository.FindAsync(s => s.is_enabled && s.sector_id == sector.sector_id);
        //                         objSec.SectorName = tsec.sector_name;
        //                         string latlong = "";
        //                         if (!string.IsNullOrEmpty(tsec.latitude) && !string.IsNullOrEmpty(tsec.longitude))
        //                         {
        //                             latlong = tsec.latitude + " / " + tsec.longitude;
        //                         }
        //                         objSec.SectorLatLong = latlong;
        //                         objSec.SectorShortInfo = tsec.sector_info_short;
        //                         if (!string.IsNullOrEmpty(tsec.angles_top_2) && tsec.angles_top_2.Contains(","))
        //                         {
        //                             string[] steeps = tsec.angles_top_2.Split(',');
        //                             objSec.Steepness1 = ImageSource.FromFile(GetSteepnessResourceName(Convert.ToInt32(steeps[0])));
        //                             objSec.Steepness2 = ImageSource.FromFile(GetSteepnessResourceName(Convert.ToInt32(steeps[1])));
        //                         }
        //                         else
        //                         {
        //                             objSec.Steepness1 = ImageSource.FromFile(GetSteepnessResourceName(1));
        //                             objSec.Steepness2 = ImageSource.FromFile(GetSteepnessResourceName(2));
        //                         }

        //                         var grades = await gradeRepository.GetAsync(grade => grade.sector_id == tsec.sector_id);
        //                         var tgrades = grades.OrderBy(x => x.grade_bucket_id);
        //                         if (tgrades != null)
        //                         {
        //                             int loopvar = 1;
        //                             foreach (var tgrd in tgrades)
        //                             {
        //                                 if (loopvar == 1)
        //                                 { objSec.BucketCount1 = tgrd.grade_bucket_id_count; }
        //                                 if (loopvar == 2)
        //                                 { objSec.BucketCount2 = tgrd.grade_bucket_id_count; }
        //                                 if (loopvar == 3)
        //                                 { objSec.BucketCount3 = tgrd.grade_bucket_id_count; }
        //                                 if (loopvar == 4)
        //                                 { objSec.BucketCount4 = tgrd.grade_bucket_id_count; }
        //                                 if (loopvar == 5)
        //                                 { objSec.BucketCount5 = tgrd.grade_bucket_id_count; }
        //                                 loopvar++;
        //                             }
        //                         }
        //                         SectorImageList.Add(objSec);
        //                     }
        //                 }
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //	await exceptionManager.LogException(new ExceptionTable
        //	{
        //		Method = nameof(this.LoadSectorImages),
        //		Page = this.GetType().Name,
        //		StackTrace = ex.StackTrace,
        //		Exception = ex.Message
        //	});
        //}
        //}

        //private string GetSteepnessResourceName(int steep)
        //{
        //    string resource = "";
        //    AppSteepness steepvalue;
        //    if (steep == (int)AppSteepness.Slab)
        //    {
        //        steepvalue = AppSteepness.Slab;
        //    }
        //    else if (steep == (int)AppSteepness.Vertical)
        //    {
        //        steepvalue = AppSteepness.Vertical;
        //    }
        //    else if (steep == (int)AppSteepness.Overhanging)
        //    {
        //        steepvalue = AppSteepness.Overhanging;
        //    }
        //    else
        //    {
        //        steepvalue = AppSteepness.Roof;
        //    }
        //    switch (steepvalue)
        //    {
        //        case AppSteepness.Slab:
        //            resource = "steepSlab.png";
        //            break;
        //        case AppSteepness.Vertical:
        //            resource = "steepVertical.png";
        //            break;
        //        case AppSteepness.Overhanging:
        //            resource = "steepOverhanging.png";
        //            break;
        //        case AppSteepness.Roof:
        //            resource = "steepRoof.png";
        //            break;
        //    }
        //    return resource;
        //}

        //public async void OnNavigatingTo(NavigationParameters parameters)
        //{
        //if (isLoaded)
        //    return;

        //parameters.TryGetValue(NavigationParametersConstants.MemberProfileId, out userId);
        //if (parameters.TryGetValue(NavigationParametersConstants.MemberProfileName, out userName))
        //    PageHeaderText = userName;
        //else
        //    PageHeaderText = Settings.DisplayName;

        ////lastUpdate = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");
        ////if (lastUpdate.Length == 8)
        ////{
        ////    lastUpdate = lastUpdate.Substring(4, 2) + "/" + lastUpdate.Substring(6, 2) + "/" + lastUpdate.Substring(0, 4);
        ////}
        ////else
        ////{
        ////    lastUpdate = lastUpdate.Substring(4, 2) + "/" + lastUpdate.Substring(6, 2) + "/" + lastUpdate.Substring(0, 4) + " " + lastUpdate.Substring(8, 2) + ":" + lastUpdate.Substring(10, 2) + ":" + lastUpdate.Substring(12, 2);
        ////}

        //OnPagePrepration();
        //LoadSectorImages();
        //}

        //why do we need it?
        //private ObservableCollection<MapListModel> sectorimageList;

        //public ObservableCollection<MapListModel> SectorImageList
        //{
        //    get { return sectorimageList ?? (sectorimageList = new ObservableCollection<MapListModel>()); }
        //    set { sectorimageList = value; RaisePropertyChanged(); }
        //}

        //private async Task<List<ClimbingDaysModel>> HttpGetClimbdays()
        //{
        //    httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_M_GetInitialUpdatesByType, Common.Constants.AppSetting.APP_ID, AppLastUpdateDate, "ascent", true), Settings.AccessToken);
        //    var area_response = await httpClinetHelper.Get<ClimbingDaysModel>();
        //    return area_response;
        //}

        //private async void DeleteConsensusData(Int32 routeId)
        //{
        //    httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_M_UpdateConsensusData, routeId), Settings.AccessToken);
        //    var area_response = await httpClinetHelper.Get<string>();
        //}

        //string lastUpdate;
        /// <summary>
        /// Returns app's last updated date.
        /// </summary>
        //public string AppLastUpdateDate
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(lastUpdate))
        //        {
        //            lastUpdate = "20160101000000";
        //        }
        //        //if the user has an old version of the app, add HH:mm:ss to the string
        //        if (lastUpdate.Length == 8)
        //        {
        //            lastUpdate = lastUpdate + "000000";
        //        }
        //        return lastUpdate;
        //    }
        //}

        //no refs?
        //     private async void GridTemplate_Tapped(object sender, EventArgs e)
        //     {
        //         try
        //         {
        //             UserDialogs.Instance.ShowLoading("Loading...");
        //             if (sender is SwipeGestureFrame)
        //             {
        //                 var templateGrid = ((SwipeGestureFrame)sender);
        //                 var dataItem = templateGrid.Parent.Parent.BindingContext as SendModel;
        //                 if (templateGrid.Parent != null && templateGrid.Parent.Parent != null && templateGrid.Parent.Parent.BindingContext != null &&
        //                     templateGrid.Parent.Parent.BindingContext is SendModel)
        //                 {
        //                     foreach (var item in sectorimageList)
        //                     {
        //                         if (item.SectorId == dataItem.sector_id)
        //                         {
        //                             Cache.SelctedCurrentSector = item;
        //                         }
        //                     }
        //                     //if sector data null get all detail using ascent               
        //                     GetAscentDetails(dataItem.Ascent_Id);
        //                 }
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //	await exceptionManager.LogException(new ExceptionTable
        //	{
        //		Method = nameof(this.GridTemplate_Tapped),
        //		Page = this.GetType().Name,
        //		StackTrace = ex.StackTrace,
        //		Exception = ex.Message,
        //		Data = $"sender = {JsonConvert.SerializeObject(sender)}, eventArgs = {JsonConvert.SerializeObject(e)}"
        //	});
        //}
        //}

        public async void ShowDetails(SendModel dataItem)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Loading...");
                //todo: inspect, looks wrong
                //foreach (var item in sectorimageList)
                //{
                //    if (item.SectorId == dataItem.sector_id)
                //    {
                //        Cache.SelctedCurrentSector = item;
                //    }
                //}
                //if sector data null get all detail using ascent               
                GetAscentDetails(Convert.ToInt32(dataItem.Ascent_Id));
            }
            catch (Exception ex)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    //Method = nameof(this.s),
                    Page = this.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = JsonConvert.SerializeObject(dataItem)
                });
            }
        }

        public async void DeleteItem(SendModel deleteAscent)
        {
            var response = await httpHelper.GetAsync<AscentVM>(ApiUrls.Url_Ascent_DeleteAscent(deleteAscent.Ascent_Id));

            if (response != null)
            {
                UserDialogs.Instance.Toast(new ToastConfig("")
                {
                    Message = " Ascent deleted successfully!",
                    BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                    MessageTextColor = System.Drawing.Color.White,
                    Duration = TimeSpan.FromSeconds(3)
                });

                if (!string.IsNullOrEmpty(response.Result.climbingDays))
                {
                    Settings.ClimbingDays = Convert.ToInt32(response.Result.climbingDays);
                }

                SendsList.Remove(deleteAscent);
                SetChartValue();
                ReloadConsensusSectors();
                ReloadConsensusRoutes();
            }
        }


        //protected override void OnNavigation(string param, NavigationParameters parameters = null)
        //{
        //    if (parameters == null)
        //        parameters = new NavigationParameters();

        //    if (userId != 0)
        //    {
        //        parameters.Add(NavigationParametersConstants.MemberProfileId, userId);
        //        parameters.Add(NavigationParametersConstants.MemberProfileName, userName);
        //    }
        //    base.OnNavigation(param, parameters);
        //}

        private async void ReloadConsensusSectors()
        {
            var consensusSectorsobj = new ConsensusModel
            {
                app_id = Convert.ToInt32(Common.Constants.AppSetting.APP_ID),
                app_date_last_updated = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]")
            };

            var sectorResponse = await httpHelper.PostAsync<IEnumerable<SectorTable>>(ApiUrls.Url_M_GetConsensusSectors, consensusSectorsobj);

            if (sectorResponse.ValidateResponse())
            {
                if (sectorResponse.Result.Any())
                {
                    var allFromDb = await sectorRepository.GetAsync();
                    if (allFromDb != null)
                    {
                        //Left outer join to select only those, that do exist in the database
                        var updateIntoDb = sectorResponse.Result
                            .Join(allFromDb, firstItem => firstItem.sector_id, secondItem => secondItem.sector_id, (firstItem, secondItem) =>
                            {
                                secondItem.top2_steepness = firstItem.top2_steepness;
                                return secondItem;
                            });

                        var resultUpdated = await sectorRepository.UpdateAllAsync(updateIntoDb);
                    }
                    else
                    {
                        await sectorRepository.InsertAllAsync(sectorResponse.Result);
                    }
                }
            }
        }

        private async void ReloadConsensusRoutes()
        {
            var consensusRoutesobj = new ConsensusModel
            {
                app_id = Convert.ToInt32(Common.Constants.AppSetting.APP_ID),
                app_date_last_updated = await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]")
            };

            var responseRoutes = await httpHelper.PostAsync<IEnumerable<RouteTable>>(ApiUrls.Url_M_GetConsensusRoutes, consensusRoutesobj);

            if (responseRoutes.ValidateResponse())
            {
                if (responseRoutes.Result.Any())
                {
                    var allRoutesFromDb = await routeRepository.GetAsync();
                    if (allRoutesFromDb != null)
                    {
                        //Left outer join to select only those, that do exist in the database
                        var updateIntoDb = responseRoutes.Result
                            .Join(allRoutesFromDb, firstItem => firstItem.route_id, secondItem => secondItem.route_id, (firstItem, secondItem) =>
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
                        await routeRepository.InsertAllAsync(responseRoutes.Result);
                    }
                }
            }
        }
    }
}
