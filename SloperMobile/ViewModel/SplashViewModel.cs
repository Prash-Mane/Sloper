using Newtonsoft.Json;
using Plugin.Connectivity;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.CheckForUpdateModels;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.ViewModel.UserViewModels;
using SloperMobile.Views.FlyoutPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Acr.UserDialogs;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.Model;
using System.Diagnostics;
using System.Text;

namespace SloperMobile.ViewModel
{
    //TODO: Refactor
    public class SplashViewModel : BaseViewModel
    {
        private readonly IRepository<TechGradeTable> techGradeRepository;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<RouteTable> routeRepository;
        private readonly IRepository<AppSettingTable> appSettingsRepository;
        private readonly IRepository<SectorTable> sectorRepository;
        private readonly IRepository<AreaTable> areaRepository;
        private readonly IRepository<TopoTable> topoRepository;
        private readonly IRepository<CragImageTable> cragImageRepository;
        private readonly IRepository<CragSectorMapTable> cragSectorMapRepository;
        private readonly IRepository<MapTable> mapRepository;
        private readonly IRepository<GradeTable> gradeRepository;
        private readonly IRepository<BucketTable> bucketRepository;
        private readonly IRepository<ProfileFilterTable> profileFilterRepository;
        private readonly IRepository<ProfileFilterTypeTable> profileFilterTypesRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<IssueCategoryTable> issueCategoryRepository;
        private readonly IRepository<IssueCategoryIssueTypeLinkTable> issueCategoryIssueTypeLinkRepository;
        private readonly IRepository<IssueTypeDetailTable> issuetypedetailRepository;
        private readonly IRepository<IssueTypeTable> issuetypeRepository;
        private readonly IRepository<IssueTypeDetailLinkTable> typetissuetypedetaillinkRepository;
        private readonly IRepository<AppProductTable> appProductRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        readonly IRepository<IssueTable> issueRepository;
        readonly IRepository<TrailTable> trailRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<ReceiptTable> receiptRepository;
        readonly IRepository<TempIssueTable> tempIssueRepository;
        readonly IRepository<TempRouteImageTable> tempRouteImageRepository;
        readonly IRepository<TempAscentTable> tempAscentRepository;
        readonly IRepository<UserTrailRecordsTable> trailRecordRepository;
        readonly IRepository<UserLocationTable> userLocationRepository;
        private readonly IExceptionSynchronizationManager exceptionSynchronizationManager;

        public SplashViewModel(
            INavigationService navigationService,
            IRepository<TechGradeTable> techGradeRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<AppSettingTable> appSettingsRepository,
            IRepository<SectorTable> sectorRepository,
            IRepository<AreaTable> areaRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<CragImageTable> cragImageRepository,
            IRepository<CragSectorMapTable> cragSectorMapRepository,
            IRepository<MapTable> mapRepository,
            IRepository<GradeTable> gradeRepository,
            IRepository<BucketTable> bucketRepository,
            IRepository<ProfileFilterTable> profileFilterRepository,
            IRepository<ProfileFilterTypeTable> profilefiltertypeRepository,
            IRepository<IssueCategoryTable> issuecategoryRepository,
            IRepository<IssueCategoryIssueTypeLinkTable> issuecategorytissuetypelinkRepository,
            IRepository<IssueTypeDetailTable> issuetypedetailRepository,
            IRepository<IssueTypeTable> issuetypeRepository,
            IRepository<IssueTypeDetailLinkTable> typetissuetypedetaillinkRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<AppProductTable> appProductRepository,
            IRepository<GuideBookTable> guideBookRepository,
            IRepository<IssueTable> issueRepository,
            IRepository<TrailTable> trailRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<ReceiptTable> receiptRepository,
            IRepository<TempIssueTable> tempIssueRepository,
            IRepository<TempRouteImageTable> tempRouteImageRepository,
            IRepository<TempAscentTable> tempAscentRepository,
            IRepository<UserTrailRecordsTable> trailRecordRepository,
            IRepository<UserLocationTable> userLocationRepository,
            IHttpHelper httpHelper,
            IExceptionSynchronizationManager exceptionSynchronizationManager)
            : base(navigationService, httpHelper)
        {
            this.techGradeRepository = techGradeRepository;
            this.cragRepository = cragRepository;
            this.routeRepository = routeRepository;
            this.appSettingsRepository = appSettingsRepository;
            this.sectorRepository = sectorRepository;
            this.areaRepository = areaRepository;
            this.topoRepository = topoRepository;
            this.cragImageRepository = cragImageRepository;
            this.cragSectorMapRepository = cragSectorMapRepository;
            this.mapRepository = mapRepository;
            this.gradeRepository = gradeRepository;
            this.bucketRepository = bucketRepository;
            this.profileFilterRepository = profileFilterRepository;
            this.profileFilterTypesRepository = profilefiltertypeRepository;
            this.issueCategoryRepository = issuecategoryRepository;
            this.issueCategoryIssueTypeLinkRepository = issuecategorytissuetypelinkRepository;
            this.issuetypedetailRepository = issuetypedetailRepository;
            this.issuetypeRepository = issuetypeRepository;
            this.typetissuetypedetaillinkRepository = typetissuetypedetaillinkRepository;
            this.userInfoRepository = userInfoRepository;
            this.appProductRepository = appProductRepository;
            this.guideBookRepository = guideBookRepository;
            this.issueRepository = issueRepository;
            this.trailRepository = trailRepository;
            this.parkingRepository = parkingRepository;
            this.receiptRepository = receiptRepository;
            this.tempIssueRepository = tempIssueRepository;
            this.tempRouteImageRepository = tempRouteImageRepository;
            this.tempAscentRepository = tempAscentRepository;
            this.exceptionSynchronizationManager = exceptionSynchronizationManager;
            this.trailRecordRepository = trailRecordRepository;
            this.userLocationRepository = userLocationRepository;

            ContinueCommand = new Command(ExecuteOnProcced);
            TermsCommand = new Command(ExecuteOnTerms);
            CancelCommand = new Command(async _ => await navigationService.GoBackAsync());
        }

        private float progressCounter = 0;
        private int totalPercentage = 100;
        private ProgressPopup progressPopup;
        bool oldDbExists;
        StringBuilder debugInfo;

        public string AppTitle => AppSetting.APP_TITLE.ToUpper();

        public string AppCompany => AppSetting.APP_COMPANY;

        public ICommand ContinueCommand { get; set; }
        public ICommand TermsCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        void ExecuteOnProcced(object obj)
        {
            ProceedAsync();
        }

        async Task ProceedAsync()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                bool success = false;
                try
                {
                    success = await DownloadUpdatesAsync();
                }
                catch
                {
                    success = false;
                }
                if (!success)
                {
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAllAsync(false);
                    progressCounter = 0;
                    return;
                }

                await (Application.Current as App).LoadInitializedPage();

                await Task.Delay(2000);
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAllAsync(false);
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new NetworkErrorPopup());
            }
        }

        private async Task<bool> DownloadUpdatesAsync()
        {
            debugInfo = new StringBuilder($"Old Db Exists: {oldDbExists}"); //temp hack for XAM-1552
            try
            {
                oldDbExists = appSettingsRepository.ChekIfOldDbExists();
                if (oldDbExists) //additional check if old db is initialized
                    oldDbExists = (await appSettingsRepository.GetAsyncFromOldDB()).FirstOrDefault()?.IS_INITIALIZED ?? false;
                if (oldDbExists)
                    totalPercentage = 141; //number is bigger since we'll need to recover data for downloaded crags content

                progressPopup = new ProgressPopup();
                progressPopup.BackgroundImageSource = ImageSource.FromFile("bg_splash.jpg");
                progressPopup.PopOnFinish = false;

                string url = null;

                await PopupNavigation.PushAsync(progressPopup, false);

                #region Areas
                url = ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "area");
                var areasSuccess = await LoadEntity(areaRepository, "Areas", url, 1);
                if (!areasSuccess)
                    return false;
                #endregion

                //CragExtended singleCrag = null;
                #region Crag
                //Debug.WriteLine("Loading Crags");
                //UpdateProgress("Loading\nCrags", 0);
                //debugInfo.AppendLine("Loading Crags");
                //var serverCragIdsResponse = await httpHelper.GetAsync<List<int>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "cragids"));
                //if (!serverCragIdsResponse.ValidateResponse())
                //    return false;
                //var serverCragIds = serverCragIdsResponse.Result;
                //var localCragIds = (await cragRepository.GetAsync()).Select(c => c.crag_id).ToList();
                //var localImageIds = (await cragImageRepository.GetAsync()).Select(i => i.crag_id).ToList();
                ////serverCragIds.Result.RemoveAll(c => localCragIds.Contains(c));
                //var cragsDownloaded = serverCragIds.Count <= localCragIds.Count;
                //var cragImagesDownloaded = serverCragIds.Count <= localImageIds.Count;
                //if (cragsDownloaded && cragImagesDownloaded)
                //{
                //    Debug.WriteLine("Crags and images are already in new db");
                //    UpdateProgress("Loading\nCrags", 82);
                //}
                //else if (oldDbExists) 
                //{
                //    if (!cragsDownloaded)
                //    {
                //        var cragsCopied = await cragRepository.CopyAllOldDataAsync();
                //        if (cragsCopied)
                //        {
                //            localCragIds = (await cragRepository.GetAsync()).Select(c => c.crag_id).ToList();
                //            cragsDownloaded = serverCragIds.Count <= localCragIds.Count;
                //        }
                //    }

                //    if (cragsDownloaded) //if all crags copied from DB -> try to copy images
                //    {
                //        var imgsCopied = await cragImageRepository.CopyAllOldDataAsync();
                //        if (imgsCopied)
                //        {
                //            localImageIds = (await cragImageRepository.GetAsync()).Select(i => i.crag_id).ToList();
                //            cragImagesDownloaded = serverCragIds.Count <= localImageIds.Count;
                //        }
                //    }

                //    if (cragsDownloaded && cragImagesDownloaded)
                //    {
                //        Debug.WriteLine("Crags and images are all copied");
                //        UpdateProgress("Loading\nCrags", 82);
                //    }
                //}

                //if (!cragsDownloaded || !cragImagesDownloaded)
                //{
                //    debugInfo.AppendLine("Downloading Crags");
                //    //await cragSectorMapRepository.DeleteAll();
                //    //await mapRepository.DeleteAll();
                //    //var cragsProgress = new Progress<double>();
                //    //cragsProgress.ProgressChanged += (object sender, double e) =>
                //    //{
                //    //    //82 is coeficient for crags to take 82% of total time
                //    //    var val = (decimal)(e * 82 + progressCounter) / totalPercentage;
                //    //    progressPopup.Report(("Loading\nCrags", val));
                //    //};
                //    //Debug.WriteLine($"Downloading crags from: {ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "crag")}");
                //    //var cragsResult = await httpHelper.GetWithProgressAsync<List<CragExtended>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "crag"), cragsProgress);
                //    //progressCounter += 82;
                //    //cragsProgress = null;
                //    Debug.WriteLine($"Downloading crags from: {ApiUrls.GetCragById(0)}");
                //    var localJoinedIds = localCragIds.Intersect(localImageIds);
                //    serverCragIds = serverCragIds.Except(localJoinedIds).ToList();

                //    for (int i = 0; i < serverCragIds.Count; i++)
                //    {
                //        var cragResult = await httpHelper.GetAsync<CragExtended>(ApiUrls.GetCragById(serverCragIds[i]));
                //        if (!cragResult.ValidateResponse())
                //            return false;

                //        var crag = cragResult.Result;
                //        if (!localCragIds.Any(id => id == crag.crag_id)) //do not insert to hadle crag's downloaded state from old db. Happens if only images were changed
                //            await cragRepository.InsertAsync(crag);


                //        var cragImage = new CragImageTable {
                //            crag_id = crag.crag_id,
                //            crag_image = crag.crag_image,
                //            crag_portrait_url = crag.crag_portrait_url,
                //            crag_landscape_url = crag.crag_landscape_url
                //            //crag_portrait_image = crag.crag_portrait_image,
                //            //crag_landscape_image = crag.crag_landscape_image
                //        };
                //        await cragImageRepository.InsertOrReplaceAsync(cragImage);
                //        var percents = (((decimal)i / serverCragIds.Count) * 82 + progressCounter)/totalPercentage;
                //        progressPopup.Report(("Loading\nCrags", percents));
                //    }
                //    progressCounter += 82;

                //    //if (!cragsResult.ValidateResponse())
                //    //    return false;

                //    //await cragRepository.DeleteAll();
                //    //await cragRepository.InsertAllAsync(cragsResult.Result);

                //    //var cragImageToInsert = cragsResult.Result
                //    //   .Where(crag =>
                //    //   {
                //    //       return !string.IsNullOrEmpty(crag.crag_image);
                //    //   })
                //    //   .Select(crag => new CragImageTable()
                //    //   {
                //    //       crag_id = crag.crag_id,
                //    //       crag_image = crag.crag_image,
                //    //       crag_portrait_image = crag.crag_portrait_image,
                //    //       crag_landscape_image = crag.crag_landscape_image
                //    //   }).ToList();
                //    //try { singleCrag = cragsResult.Result.SingleOrDefault(crag => crag.crag_latitude != 0 && crag.crag_longitude != 0); }
                //    //catch { }
                //    //cragsResult = null;
                //    //GC.Collect();

                //    //await cragImageRepository.DeleteAll();
                //    ////it may thow out of memory on 512MB RAM android devices
                //    ////var insertedCrageImages = await cragImageRepository.InsertAllAsync(cragImageToInsert);
                //    //while (cragImageToInsert.Any())
                //    //{
                //    //    var cragImg = cragImageToInsert.First();
                //    //    await cragImageRepository.InsertAsync(cragImg);
                //    //    cragImageToInsert.Remove(cragImg);
                //    //    cragImg = null;
                //    //    GC.Collect();
                //    //}
                //    //cragImageToInsert = null;
                //}
                #endregion


                Debug.WriteLine("Loading CragIds");
                UpdateProgress("Loading\nCrags", 0);
                debugInfo.AppendLine("Loading CragIds");
                var serverCragIdsResponse = await httpHelper.GetAsync<List<int>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "cragids"));
                if (!serverCragIdsResponse.ValidateResponse())
                    return false;
                var serverCragIds = serverCragIdsResponse.Result;

                #region Crag
                Debug.WriteLine($"Loading Crags");
                UpdateProgress($"Loading\nCrags", 1);
                debugInfo.AppendLine("Downloading Crags");
                var localCragIds = (await cragRepository.GetAsync()).Select(c => c.crag_id).ToList();
                var cragIdsToDownload = serverCragIds.Except(localCragIds);
                IEnumerable<int> cragIdsWithContent = null;
                bool cragsDownloaded = !cragIdsToDownload.Any();

                if (oldDbExists && !cragsDownloaded)
                {
                    cragsDownloaded = await cragRepository.CopyAllOldDataAsync();
                    cragIdsWithContent = (await cragRepository.GetAsyncFromOldDB())
                                                ?.Where(c => c.is_downloaded)
                                                ?.Select(c => c.crag_id);
                }

                if (cragsDownloaded)
                {
                    UpdateProgress($"Loading\nCrags", 11);
                    debugInfo.AppendLine("Crag copied from old DB");
                }
                else
                {
                    foreach (var cragId in cragIdsToDownload)
                    {
                        Debug.WriteLine($"Downloading from endpoint: {ApiUrls.GetCragById(cragId)}");
                        debugInfo.AppendLine($"Downloading from endpoint: {ApiUrls.GetCragById(cragId)}");
                        var cragResponse = await httpHelper.GetAsync<CragExtended>(ApiUrls.GetCragById(cragId));
                        if (!cragResponse.ValidateResponse())
                            return false;

                        cragResponse.Result.is_downloaded = cragIdsWithContent?.Contains(cragId) ?? false;

                        await cragRepository.InsertOrReplaceAsync(cragResponse.Result);
                        UpdateProgress($"Loading\nCrags", 11.0f / (float)cragIdsToDownload.Count());
                    }
                    Debug.WriteLine($"Download successful: {cragIdsToDownload.Count()} items");
                    debugInfo.AppendLine($"Download successful: {cragIdsToDownload.Count()} items");
                }
                #endregion

                #region CragImage
                Debug.WriteLine($"Loading CragImages");
                UpdateProgress($"Loading\nCrag Images", 0);
                debugInfo.AppendLine("Downloading Crag Images");
                var localImageIds = (await cragImageRepository.GetAsync()).Select(i => i.crag_id).ToList();
                var idsToDownload = serverCragIds.Except(localImageIds);
                bool imagesDownloaded = !idsToDownload.Any();

                if (oldDbExists && !imagesDownloaded)
                    imagesDownloaded = await cragImageRepository.CopyAllOldDataAsync();

                if (imagesDownloaded)
                {
                    UpdateProgress($"Loading\nCrag Images", 50);
                    Debug.WriteLine("Crags images are all copied");
                    debugInfo.AppendLine("Crags images are all copied");
                }
                else
                {
                    foreach (var id in idsToDownload)
                    {
                        Debug.WriteLine($"Downloading from endpoint: {ApiUrls.GetCragImages(id)}");
                        debugInfo.AppendLine($"Downloading from endpoint: {ApiUrls.GetCragImages(id)}");
                        var imageResponse = await httpHelper.GetAsync<CragImageTable>(ApiUrls.GetCragImages(id));
                        if (!imageResponse.ValidateResponse())
                            return false;

                        await cragImageRepository.InsertOrReplaceAsync(imageResponse.Result);
                        UpdateProgress($"Loading\nCrag Images", 50 / (float)idsToDownload.Count());
                    }
                    Debug.WriteLine($"Download successful: {idsToDownload.Count()} items");
                    debugInfo.AppendLine($"Download successful: {idsToDownload.Count()} items");
                }
                #endregion

                // ========================== Added by Ravi [02-Mar-2019] =====================
                var timer = Stopwatch.StartNew();
                //those will not work if query string is too long (too many crags on live)
                var cragids = string.Join(",", serverCragIds);
                url = ApiUrls.Url_M_GetUpdatesByCragAndType(cragids, AppConstant.DefaultDate, "sector", true);
                var sectorsSuccess = await LoadEntity(sectorRepository, "Sectors", url, 1);
                if (!sectorsSuccess)
                    return false;

                Debug.WriteLine("Sector download time using Ravi's approach:");
                Debug.WriteLine(timer.ElapsedMilliseconds);

                //those will work on live and won't crash due to too long urls. But are a lot slower
                //timer.Restart();
                //Func<int, string> urlPattern1 = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.SectorRequestType);
                //var sectorsSuccess = await LoadEntityInLoop(sectorRepository, "Sector", urlPattern1, 4, serverCragIds);
                //if (!sectorsSuccess)
                //    return false;
                //Debug.WriteLine("Sectors download time using old approach:");
                //Debug.WriteLine(timer.ElapsedMilliseconds);

                timer.Restart();
                url = ApiUrls.Url_M_GetUpdatesByCragAndType(cragids, AppConstant.DefaultDate, "route", true);
                //var routesSuccess = await LoadEntity(routeRepository, "Routes", url, 7);
                //if (!routesSuccess)
                //return false;

                var resultRoutes = await LoadEntityWithProgress(routeRepository, url, "Routes", 7);
                if (!resultRoutes)
                    return false;
                Debug.WriteLine("Routes download time using Ravi's approach:");
                Debug.WriteLine(timer.ElapsedMilliseconds);
                //timer.Restart();
                //urlPattern1 = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.RouteRequestType);
                //var routesSuccess = await LoadEntityInLoop(routeRepository, "Routes", urlPattern1, 4, serverCragIds);
                //if (!routesSuccess)
                //    return false;
                //Debug.WriteLine("Routes download time using old approach:");
                //Debug.WriteLine(timer.ElapsedMilliseconds);
                //==============================================================================


                //TODO: Remove; testing block
#if DEBUG
                var crags = await cragRepository.GetAsync();
                var sectors = await sectorRepository.GetAsync();
                var routes = await routeRepository.GetAsync();
                var sectorsNoCrag = sectors.Where(s => !crags.Any(c => s.crag_id == c.crag_id)).Select(s => s.sector_id).ToList();
                var cragsNoSector = crags.Where(c => !sectors.Any(s => c.crag_id == s.crag_id)).Select(c => c.crag_id).ToList();
                var sectorNoRoutes = sectors.Where(s => !routes.Any(r => s.sector_id == r.sector_id)).Select(s => s.sector_id).ToList();
                var routesNoSector = routes.Where(r => !sectors.Any(s => s.sector_id == r.sector_id)).Select(r => r.route_id).ToList();

                if (sectorsNoCrag.Any()) {
                    Debug.WriteLine("Sectors without crags:");
                    var sectorsWithoutCrags = string.Join(",", sectorsNoCrag);
                    Debug.WriteLine(sectorsWithoutCrags);
                }
                if (cragsNoSector.Any())
                {
                    Debug.WriteLine("Crags without sectors:");
                    var cragsWithoutSectors = string.Join(",", cragsNoSector);
                    Debug.WriteLine(cragsWithoutSectors);
                }
                if (sectorNoRoutes.Any())
                {
                    Debug.WriteLine("Sectors without routes:");
                    var sectorsWithoutRoutes = string.Join(",", sectorNoRoutes);
                    Debug.WriteLine(sectorsWithoutRoutes);
                }
                if (routesNoSector.Any())
                {
                    Debug.WriteLine("Routes without sectors:");
                    var routesWithoutSectors = string.Join(",", routesNoSector);
                    Debug.WriteLine(routesWithoutSectors);
                }
#endif
                /*

                #region Single Crag   (It has to be removed after successfull implementation of Local store of Sector and Routes)
                // START - CODE BELOW MIGHT BE USED FOR APP_TYPE = "indoor" ====================================================== 
                CragExtended singleCrag = null;
                try
                {
                    singleCrag = (await cragRepository.GetAsync(c => c.crag_latitude != 0 && c.crag_latitude != 0)).SingleOrDefault();
                }
                catch { }

                if (singleCrag != null)
                {
                    UpdateProgress("Loading\nSectors");
                    var sectorsResponse = await httpHelper.GetAsync<List<SectorTable>>(ApiUrls.Url_M_GetUpdatesByType(singleCrag.crag_id, "sector"));
                    if (!sectorsResponse.ValidateResponse())
                        return false;

                    //SectorObj = await GetUpdatesByTypeAndIdAsync<SectorTable>("sector", cragWithCoordinates.FirstOrDefault().crag_id);
                    var allFromDb = await sectorRepository.GetAsync();

                    //Left outer join to select only those, that do not exist in the database
                    var insertIntoDb = (from remote in sectorsResponse.Result
                                        join db in allFromDb on remote.sector_id equals db.sector_id into @group
                                        where !@group.Any()
                                        select remote);
                    await sectorRepository.DeleteAll();
                    var resultInserted = await sectorRepository.InsertAllAsync(insertIntoDb);

                    var topos = new List<TopoTable>();

                    var sectorsCount = sectorsResponse.Result.Count();
                    UpdateProgress("Loading\nTopos");
                    for (var sector = 0; sector < sectorsCount; sector++)
                    {
                        UpdateProgress($"Loading\nTopo {sector + 1} of {sectorsCount}", 0);
                        url = ApiUrls.Url_TopoImageServer_Get(sectorsResponse.Result[sector].sector_id);
                        var topoResult = await httpHelper.GetAsync<string>(url);
                        if (!topoResult.ValidateResponse())
                            return false;

                        TopoTable topo = new TopoTable();
                        topo.sector_id = sectorsResponse.Result[sector].sector_id;
                        topo.topo_json = topoResult.Result;
                        topo.upload_date = GeneralHelper.GetCurrentDate("yyyyMMdd");
                        topos.Add(topo);
                    }

                    await topoRepository.DeleteAll();
                    var resultInsertedTopos = await topoRepository.InsertAllAsync(topos);

                    // INSERT ROUTES
                    UpdateProgress("Loading\nRoutes");
                    var routesResponse = await httpHelper.GetAsync<List<RouteTable>>(ApiUrls.Url_M_GetUpdatesByType(singleCrag.crag_id, "route"));
                    if (!routesResponse.ValidateResponse())
                        return false;

                    var allroutesFromDb = await routeRepository.GetAsync();

                    //Left outer join to select only those, that do not exist in the database
                    var insertRouteDb = (from remote in routesResponse.Result
                                         join db in allroutesFromDb on remote.route_id equals db.route_id into @group
                                         where !@group.Any()
                                         select remote);

                    await routeRepository.DeleteAll();
                    var reslutrootInserted = await routeRepository.InsertAllAsync(insertRouteDb);

                    singleCrag.is_downloaded = true;
                    await cragRepository.UpdateAsync(singleCrag);
                    Settings.ActiveCrag = singleCrag.crag_id;
                }

                #endregion


                */

                // END - CODE BELOW MIGHT BE USED FOR APP_TYPE = "indoor" ======================================================

                // INSERT GRADES
                url = ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "grade");
                var gradesSuccess = await LoadEntity(gradeRepository, "Grades", url, 85 - progressCounter);
                if (!gradesSuccess)
                    return false;

                // INSERT GRADE BUCKETS
                var bucketsSuccess = await LoadEntity(bucketRepository, "Grade Buckets", ApiUrls.Url_M_GetGradesByAppId, 1);
                if (!bucketsSuccess)
                    return false;

                // INSERT TECH GRADES
                var techGradesSuccess = await LoadEntity(techGradeRepository, "Technical Grades", ApiUrls.Url_M_GetTTechGrades, 1);
                if (!techGradesSuccess)
                    return false;


                // INSERT Profile Filter Type
                var profileFilterTypesSuccess = await LoadEntity(profileFilterTypesRepository, "Profile Filter Types", ApiUrls.Url_M_ProfileFilterType, 1);
                if (!profileFilterTypesSuccess)
                    return false;

                //INSERT Profile Filter
                var profileFilterSuccess = await LoadEntity(profileFilterRepository, "Profile Filter", ApiUrls.Url_M_ProfileFilter, 1);
                if (!profileFilterSuccess)
                    return false;

                ////INSERT Profile
                //ProgressText = "Loading Profile";
                //await _userInfoRepository.DropTableAsync();
                //userInfoObj = await HttpGetUserInfo();
                //if (userInfoObj != null)
                //{
                //    Settings.UserIDSettings = userInfoObj.UserID.ToString();
                //    await _userInfoRepository.InsertAsync(userInfoObj);
                //}

                //INSERT ISSUE DATA ============================================
                var issueCategorySuccess = await LoadEntity(issueCategoryRepository, "Route Issues", ApiUrls.Url_M_GetIssueData(0), 1);
                if (!issueCategorySuccess)
                    return false;

                var issuetypeSuccess = await LoadEntity(issuetypeRepository, "Route Issues", ApiUrls.Url_M_GetIssueData(1), 0);
                if (!issueCategorySuccess)
                    return false;

                var issueTypeDetailsSuccess = await LoadEntity(issuetypedetailRepository, "Route Issues", ApiUrls.Url_M_GetIssueData(2), 0);
                if (!issueTypeDetailsSuccess)
                    return false;

                var issueCategoryLinksSuccess = await LoadEntity(issueCategoryIssueTypeLinkRepository, "Route Issues", ApiUrls.Url_M_GetIssueData(3), 0);
                if (!issueCategoryLinksSuccess)
                    return false;

                var issueTypeDetailLinksSuccess = await LoadEntity(typetissuetypedetaillinkRepository, "Route Issues", ApiUrls.Url_M_GetIssueData(4), 0);
                if (!issueTypeDetailLinksSuccess)
                    return false;


                // Insert App Products Ids
                url = ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "appproduct");
                var appProductsSuccess = await LoadEntity(appProductRepository, "App Products", url, 1);
                if (!appProductsSuccess)
                    return false;

                // Insert GuideBook data
                debugInfo.AppendLine("Loading Guidebooks");
                Debug.WriteLine($"Loading Guidebooks");
                UpdateProgress($"Loading\nGuidebooks", 0);
                var entitiesDownloaded = (await guideBookRepository.CountAsync()) > 0;
                if (entitiesDownloaded)
                {
                    Debug.WriteLine($"Guidebook is already in new db");
                    debugInfo.AppendLine($"Guidebook is already in new db");
                }

                if (oldDbExists && !entitiesDownloaded)
                    entitiesDownloaded = await guideBookRepository.CopyAllOldDataAsync();

                if (!entitiesDownloaded)
                {
                    //Debug.WriteLine($"Downloading from endpoint: {url}");
                    //debugInfo.AppendLine($"Downloading from endpoint: {url}");
                    //var guidebooksProgress = new Progress<double>();
                    //guidebooksProgress.ProgressChanged += (object sender, double e) =>
                    //{
                    //    //7 is coeficient for guidebooks to take 7% of total time
                    //    var val = (decimal)(e * 7 + progressCounter) / totalPercentage;
                    //    progressPopup.Report(("Loading\nGuidebooks", val));
                    //};
                    //Debug.WriteLine($"Downloading guidebooks from: {ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "guidebook")}");
                    //debugInfo.AppendLine($"Downloading guidebooks from: {ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "guidebook")}");
                    //var gbsResult = await httpHelper.GetWithProgressAsync<List<GuideBookTable>>(ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "guidebook"), guidebooksProgress);
                    //guidebooksProgress = null;
                    //if (!gbsResult.ValidateResponse())
                    //    return false;

                    //await guideBookRepository.InsertOrReplaceAsync(gbsResult.Result);
                    //Debug.WriteLine($"Download successful: {gbsResult.Result.Count} items");
                    //debugInfo.AppendLine($"Download successful: {gbsResult.Result.Count} items");
                    //gbsResult = null;

                    var resultGuidebook = await LoadEntityWithProgress(guideBookRepository,
                         ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "guidebook"), "Guidebooks", 7);
                    if (!resultGuidebook)
                        return false;
                }
                //url = ApiUrls.Url_M_GetInitialUpdatesByType(AppConstant.DefaultDate, "guidebook");
                //var guideBooksSuccess = await LoadEntity(guideBookRepository, "Guidebooks", url, 7);
                //if (!guideBooksSuccess)
                //return false;

                if (oldDbExists)
                {
                    debugInfo.AppendLine("Loading CragContentData");
                    #region CragContent
                    var downloadedCragIds = (await cragRepository.GetAsync(c => c.is_downloaded)).Select(c => c.crag_id).ToList();
                    if (downloadedCragIds.Any())
                    {
                        var allSectors = await sectorRepository.GetAsync();
                        List<int> sectorIds = allSectors.Where(s => downloadedCragIds.Any(c => s.crag_id == c)).Select(s => s.sector_id).ToList(); //downloaded sectorIds needed for topo downloading url

                        //sectors
                        //debugInfo.AppendLine("Loading Sectors");
                        //UpdateProgress("Loading Sectors", 0);
                        //sectorsSuccess = (await sectorRepository.CountAsync()) > 0; //check if already populated
                        //if (!sectorsSuccess)
                        //    sectorsSuccess = await sectorRepository.CopyAllOldDataAsync(); //try to copy from old DB

                        //if (sectorsSuccess)
                        //{
                        //    sectorIds = (await sectorRepository.GetAsyncFromOldDB()).Select(s => s.sector_id).ToList();
                        //    UpdateProgress("Loading Sectors", 1);
                        //}
                        //else //no entries in new db yet, failed to copy from old db -> grab from endpoint
                        //{
                        //    List<SectorTable> sectorsReceived = new List<SectorTable>();
                        //    foreach (var cragId in downloadedCragIds)
                        //    {
                        //        var sectorsResponse = await httpHelper.GetAsync<List<SectorTable>>(ApiUrls.Url_M_GetUpdatesByType(cragId, AppConstant.SectorRequestType));
                        //        if (!sectorsResponse.ValidateResponse())
                        //            return false;

                        //        sectorsReceived.AddRange(sectorsResponse.Result);
                        //    }
                        //    await sectorRepository.InsertOrReplaceAsync(sectorsReceived, false); //we want combined sectors to be saved all at once to be able to check if copying/downloading is needed
                        //    sectorIds = sectorsReceived.Select(s => s.sector_id).ToList();
                        //    UpdateProgress("Loading Sectors", 1);
                        //}

                        //routes
                        //Func<int, string> urlPattern = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.RouteRequestType);
                        //routesSuccess = await LoadEntityInLoop(routeRepository, "Routes", urlPattern, 4, downloadedCragIds);
                        //if (!routesSuccess)
                        //return false;

                        //route issues
                        Func<int, string> urlPattern = (id) => ApiUrls.Url_M_GetRouteIssueData(id, AppConstant.DefaultDate);
                        var routeIssuesSuccess = await LoadEntityInLoop(issueRepository, "Route Issues", urlPattern, 1, downloadedCragIds);
                        if (!routeIssuesSuccess)
                            return false;

                        //maps
                        urlPattern = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.MapRequestType);
                        Func<MapData, MapTable> mapsConverter = (MapData model) => new MapTable
                        {
                            map_id = model.map_id,
                            crag_id = model.crag_id,
                            map_name = model.map_name,
                            imagedata = model.map_image,
                            width = model.width,
                            height = model.height,
                            sort_order = model.sort_order,
                            is_enabled = model.is_enabled
                        };
                        var mapsSuccess = await LoadEntityInLoopConvertable(mapRepository, "Maps", urlPattern, 1, downloadedCragIds, mapsConverter);
                        if (!mapsSuccess)
                            return false;

                        //regions
                        urlPattern = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.MapRegionRequestType);
                        Func<MapRegion, CragSectorMapTable> regionConverter = (MapRegion model) => new CragSectorMapTable
                        {
                            id = $"{model.crag_id}+{model.sector_id}+{model.map_id}",
                            crag_id = model.crag_id,
                            map_id = model.map_id,
                            sector_id = model.sector_id,
                            x = model.x,
                            y = model.y,
                            height = model.height,
                            width = model.width,
                        };
                        var regionsSuccess = await LoadEntityInLoopConvertable(cragSectorMapRepository, "Map Regions", urlPattern, 1, downloadedCragIds, regionConverter);
                        if (!regionsSuccess)
                            return false;

                        //trails
                        urlPattern = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.TrailRequestType);
                        var trailsSuccess = await LoadEntityInLoop(trailRepository, "Trails", urlPattern, 1, downloadedCragIds);
                        if (!trailsSuccess)
                            return false;

                        //parkings
                        urlPattern = (id) => ApiUrls.Url_M_GetUpdatesByType(id, AppConstant.ParkingRequestType);
                        var parkingsSuccess = await LoadEntityInLoop(parkingRepository, "Parking Lots", urlPattern, 1, downloadedCragIds);
                        if (!parkingsSuccess)
                            return false;

                        //topos
                        debugInfo.AppendLine("Loading Topos");
                        UpdateProgress("Loading Topos", 0);
                        Debug.WriteLine("Loading Topos");
                        var existingTopoIds = (await topoRepository.GetAsync()).Select(t => t.sector_id);
                        var toposSuccess = sectorIds.Count <= existingTopoIds.Count();
                        if (!toposSuccess)
                            toposSuccess = await topoRepository.CopyAllOldDataAsync();

                        if (toposSuccess)
                            UpdateProgress("Loading Topos", 30);
                        else
                        {
                            var idsFiltered = sectorIds.Except(existingTopoIds).ToArray();
                            for (int i = 0; i < idsFiltered.Length; i++)
                            {
                                var sectorId = idsFiltered[i];
                                Debug.WriteLine($"Loading topo from {ApiUrls.Url_TopoImageServer_Get(sectorId)}");
                                debugInfo.AppendLine($"Loading topo from {ApiUrls.Url_TopoImageServer_Get(sectorId)}");
                                var topoResponse = await httpHelper.GetAsync<string>(ApiUrls.Url_TopoImageServer_Get(sectorId));
                                if (!topoResponse.ValidateResponse())
                                    return false;

                                var dbTopo = new TopoTable
                                {
                                    sector_id = sectorId,
                                    topo_json = topoResponse.Result,
                                    upload_date = GeneralHelper.GetCurrentDate(AppConstant.DateParseString)
                                };

                                await topoRepository.InsertOrReplaceAsync(dbTopo);
                                var percents = (i / idsFiltered.Length * 30 + progressCounter) / totalPercentage;
                                progressPopup.Report(("Loading\nTopos", (decimal)percents));
                            }
                            progressCounter += 30;
                            Debug.WriteLine($"Download topos successful: {idsFiltered.Length} items");
                            debugInfo.AppendLine($"Download topos successful: {idsFiltered.Length} items");
                        }
                    }
                    else
                        UpdateProgress("Copying Mobile Data", 40);
                    #endregion


                    #region MobileTables
                    debugInfo.AppendLine("Copying Mobile Data");
                    UpdateProgress("Copying Mobile Data", 1);
                    await receiptRepository.CopyAllOldDataAsync();
                    await tempIssueRepository.CopyAllOldDataAsync();
                    await tempRouteImageRepository.CopyAllOldDataAsync();
                    await tempAscentRepository.CopyAllOldDataAsync();
                    await trailRecordRepository.CopyAllOldDataAsync();
                    await userLocationRepository.CopyAllOldDataAsync();
                }
                #endregion

                debugInfo.AppendLine("Initializing App");
                UpdateProgress(" \nInitializing App", 1);
                await userInfoRepository.CopyAllOldDataAsync();
                #region RestoreSettings
                try
                {
                    var profile = await userInfoRepository.GetAsyncFromOldDB();
                    Settings.UserID = profile?.FirstOrDefault()?.UserID ?? 0;
                    var downloadedCrag = (await cragRepository.GetAsync(c => c.is_enabled
                                                                && c.is_app_store_ready
                                                                && c.is_downloaded
                                                                && c.crag_latitude != null
                                                                && c.crag_longitude != null
                                                                && c.crag_latitude != 0
                                                                && c.crag_longitude != 0)
                                  ).OrderBy(crag => crag.crag_sort_order)
                                   .FirstOrDefault();
                    Settings.MapSelectedCrag = Settings.ActiveCrag = downloadedCrag?.crag_id ?? 0;
                }
                catch { } //it's fine to silently catch if failed to restore. We don't know if all versions of old db user profiles are compatible
                #endregion

                await GeneralHelper.UpdateCragsDownloadedStateAsync();

                // Pass -1 to just return the server DateTime
                var checkForUpdateResponse = await httpHelper.GetAsync<CheckForUpdateModel>(ApiUrls.Url_M_AvailableUpdates("-1"));
                if (!checkForUpdateResponse.ValidateResponse())
                    return false;

                var appSettings = new AppSettingTable
                {
                    UPDATED_DATE = checkForUpdateResponse.Result.updated_date,
                    IS_INITIALIZED = true
                };
                await appSettingsRepository.DeleteAll();
                await appSettingsRepository.InsertAsync(appSettings);
                UpdateProgress(" \nApp Initialized!");

                debugInfo.AppendLine("Deleting OldDB");
                await cragRepository.DeleteOldDb();

                return true;
            }
            catch (Exception exception)
            {
                //added try/catch in case T_EXCEPTION hasn't been created yet
                try
                {
                    await exceptionSynchronizationManager.LogException(new ExceptionTable
                    {
                        Method = nameof(this.DownloadUpdatesAsync),
                        Page = this.GetType().Name,
                        StackTrace = exception.StackTrace,
                        Exception = exception.Message,
                        Data = debugInfo.ToString()
                    });
                }
                catch { }

                throw;
            }
        }

        void ExecuteOnTerms(object obj)
        {
            navigationService.NavigateAsync<TermsViewModel>();
        }

        void UpdateProgress(string message, float increment = 1)
        {
            progressCounter += increment;

            var progress = progressCounter / totalPercentage;

            progressPopup.Report((message, (decimal)progress));
        }

        async Task<bool> LoadEntity<T>(IRepository<T> repository, string name, string url, float progressWeight) where T : class, new()
        {
            Debug.WriteLine($"LoadEntity {name}");
            UpdateProgress($"Loading\n{name}", 0);
            debugInfo.AppendLine($"LoadEntity {name}");
            var entitiesDownloaded = (await repository.CountAsync()) > 0;
            if (entitiesDownloaded)
            {
                Debug.WriteLine($"{name} is already in new db");
                debugInfo.AppendLine($"{name} is already in new db");
            }
            if (oldDbExists && !entitiesDownloaded)
                entitiesDownloaded = await repository.CopyAllOldDataAsync();

            if (!entitiesDownloaded)
            {
                Debug.WriteLine($"Downloading from endpoint: {url}");
                debugInfo.AppendLine($"Downloading from endpoint: {url}");
                //await repository.DeleteAll();
                var downloadResponse = await httpHelper.GetAsync<List<T>>(url);
                if (!downloadResponse.ValidateResponse())
                    return false;

                await repository.InsertOrReplaceAsync(downloadResponse.Result);
                Debug.WriteLine($"Download successful: {downloadResponse.Result.Count} items");
                debugInfo.AppendLine($"Download successful: {downloadResponse.Result.Count} items");
                downloadResponse = null;
            }
            UpdateProgress($"Loading\n{name}", progressWeight);
            return true;
        }

        async Task<bool> LoadEntityInLoop<T>(IRepository<T> repository, string name, Func<int, string> urlPattern, float progressWeight, List<int> ids) where T : class, new()
        {
            Debug.WriteLine($"LoadEntityInLoop {name}");
            debugInfo.AppendLine($"LoadEntityInLoop {name}");
            UpdateProgress($"Loading\n{name}", 0);
            var entitiesDownloaded = (await repository.CountAsync()) > 0;
            if (entitiesDownloaded)
            {
                Debug.WriteLine($"{name} is already in new db");
                debugInfo.AppendLine($"{name} is already in new db");
            }
            if (oldDbExists && !entitiesDownloaded)
                entitiesDownloaded = await repository.CopyAllOldDataAsync();

            if (!entitiesDownloaded) //no entries in new db yet, failed to copy from old db -> grab from endpoint
            {
                List<T> entitiesCombined = new List<T>();
                foreach (var containerId in ids)
                {
                    Debug.WriteLine($"Downloading from endpoint: {urlPattern(containerId)}");
                    debugInfo.AppendLine($"Downloading from endpoint: {urlPattern(containerId)}");
                    var entityResponse = await httpHelper.GetAsync<List<T>>(urlPattern(containerId));
                    if (!entityResponse.ValidateResponse())
                        return false;

                    entitiesCombined.AddRange(entityResponse.Result);
                }
                await repository.InsertOrReplaceAsync(entitiesCombined); //we want combined entities to be saved all at once to be able to check if copying/downloading is needed
                Debug.WriteLine($"Download successful: {entitiesCombined.Count} items");
                debugInfo.AppendLine($"Download successful: {entitiesCombined.Count} items");
            }
            UpdateProgress($"Loading\n{name}", progressWeight);
            return true;
        }

        async Task<bool> LoadEntityInLoopConvertable<TDb, TModel>(IRepository<TDb> repository, string name, Func<int, string> urlPattern, float progressWeight, List<int> ids, Func<TModel, TDb> converter) where TDb : class, new()
        {
            Debug.WriteLine($"LoadEntityInLoopConvertable {name}");
            UpdateProgress($"Loading\n{name}", 0);
            debugInfo.AppendLine($"LoadEntityInLoopConvertable {name}");
            var entitiesDownloaded = (await repository.CountAsync()) > 0; //check if already populated
            if (entitiesDownloaded)
            {
                Debug.WriteLine($"{name} is already in new db");
                //return true;
            }
            if (oldDbExists && !entitiesDownloaded)
                entitiesDownloaded = await repository.CopyAllOldDataAsync();

            if (!entitiesDownloaded) //no entries in new db yet, failed to copy from old db -> grab from endpoint
            {
                List<TModel> modelsCombined = new List<TModel>();
                foreach (var containerId in ids)
                {
                    Debug.WriteLine($"Downloading from endpoint: {urlPattern(containerId)}");
                    debugInfo.AppendLine($"Downloading from endpoint: {urlPattern(containerId)}");
                    var entityResponse = await httpHelper.GetAsync<List<TModel>>(urlPattern(containerId));
                    if (!entityResponse.ValidateResponse())
                        return false;

                    modelsCombined.AddRange(entityResponse.Result);
                }
                var dbEntities = modelsCombined.Select(converter);
                await repository.InsertOrReplaceAsync(dbEntities); //we want combined entities to be saved all at once to be able to check if copying/downloading is needed
                Debug.WriteLine($"Download successful: {modelsCombined.Count} items");
                debugInfo.AppendLine($"Download successful: {modelsCombined.Count} items");
            }
            UpdateProgress($"Loading\n{name}", progressWeight);
            return true;
        }

        async Task<bool> LoadEntityWithProgress<T>(IRepository<T> repository,string requestUri, string name, float progressWeight) where T : class, new()
        {
            UpdateProgress($"Loading\n{name}", 0);
            var progress = new Progress<double>();
            progress.ProgressChanged += (object sender, double e) =>
            {
                //7 is coeficient for guidebooks to take 7% of total time
                var val = (decimal)(e * progressWeight + progressCounter) / totalPercentage;
                progressPopup.Report(($"Loading\n{name}", val));
            };

            Debug.WriteLine($"Downloading {name} from: {requestUri}");
            debugInfo.AppendLine($"Downloading {name} from: {requestUri}");
            var result = await httpHelper.GetWithProgressAsync<List<T>>(requestUri, progress);
            progress = null;

            if (!result.ValidateResponse())
                return false;

            await repository.InsertOrReplaceAsync(result.Result);
            UpdateProgress($"Loading\n{name}", progressWeight);

            return true;
        }
    }
}
