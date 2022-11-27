using Acr.UserDialogs;
using Newtonsoft.Json;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.CheckForUpdateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using SloperMobile.ViewModel.MasterDetailViewModels;
using System.Text;

namespace SloperMobile.Common.Services
{
	public class CheckForUpdateService : ICheckForUpdateService
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
        private readonly IRepository<ProfileFilterTable> profilefilterRepository;
        private readonly IRepository<ProfileFilterTypeTable> profilefiltertypeRepository;
        private readonly IRepository<UserInfoTable> userInfoRepository;
        private readonly IRepository<IssueCategoryTable> issueCategoryRepository;
        private readonly IRepository<IssueCategoryIssueTypeLinkTable> issueCategoryIssueTypeLinkRepository;
        private readonly IRepository<IssueTypeDetailTable> issuetypedetailRepository;
        private readonly IRepository<IssueTypeTable> issuetypeRepository;
        private readonly IRepository<IssueTypeDetailLinkTable> typetissuetypedetaillinkRepository;
        private readonly IRepository<IssueTable> issueRepository;
        private readonly IRepository<AppProductTable> appProductRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        readonly IRepository<TrailTable> trailRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        private readonly IExceptionSynchronizationManager exceptionManager;
        private readonly IHttpHelper httpHelper;
		private readonly IPurchasedCheckService purchasedCheckService;

        private CheckForUpdateModel checkForModelObj;  
        private string lastUpdate = String.Empty;
        static bool isUpdating;
        StringBuilder debugInfo;

        public CheckForUpdateService(
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
            IRepository<IssueCategoryTable> issuecategoryRepository,
            IRepository<IssueCategoryIssueTypeLinkTable> issuecategorytissuetypelinkRepository,
            IRepository<IssueTypeDetailTable> issuetypedetailRepository,
            IRepository<IssueTypeTable> issuetypeRepository,
            IRepository<IssueTypeDetailLinkTable> typetissuetypedetaillinkRepository,
            IRepository<IssueTable> tissueRepository,
            IRepository<ProfileFilterTable> profilefilterRepository,
            IRepository<ProfileFilterTypeTable> profilefiltertypeRepository,
            IRepository<UserInfoTable> userInfoRepository,
            IRepository<AppProductTable> appProductRepository,
            IRepository<GuideBookTable> guideBookRepository,
            IRepository<TrailTable> trailRepository,
            IRepository<ParkingTable> parkingRepository,
            IExceptionSynchronizationManager exceptionManager, 
            IHttpHelper httpHelper,
			IPurchasedCheckService purchasedCheckService)
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
            this.profilefilterRepository = profilefilterRepository;
            this.profilefiltertypeRepository = profilefiltertypeRepository;
            this.userInfoRepository = userInfoRepository; 
            this.issueCategoryRepository = issuecategoryRepository;
            this.issueCategoryIssueTypeLinkRepository = issuecategorytissuetypelinkRepository;
            this.issuetypedetailRepository = issuetypedetailRepository;
            this.issuetypeRepository = issuetypeRepository;
            this.typetissuetypedetaillinkRepository = typetissuetypedetaillinkRepository;
            this.issueRepository = tissueRepository;
            this.appProductRepository = appProductRepository;
            this.guideBookRepository = guideBookRepository;
            this.trailRepository = trailRepository;
            this.parkingRepository = parkingRepository;
            this.exceptionManager = exceptionManager;
            this.httpHelper = httpHelper;
			this.purchasedCheckService = purchasedCheckService;
		}

        /// <summary>
        /// Returns app's last updated date.
        /// </summary>
        string AppLastUpdateDate
        {
            get
            {
                if (string.IsNullOrEmpty(lastUpdate))
                {
                    lastUpdate = "20160101000000";
                }
                //if the user has an old version of the app, add HH:mm:ss to the string
                if (lastUpdate.Length == 8)
                {
                    lastUpdate = lastUpdate + "000000";
                }
                return lastUpdate;
            }
        }

        private async Task<bool> MakeUpdate()
        {
            debugInfo = new StringBuilder($"Model: {JsonConvert.SerializeObject(checkForModelObj)};");
            try
            {
                // UPDATE AREAS
                if (checkForModelObj.areas_modified > 0)
                {
                    debugInfo.AppendLine("Update Areas;");
                    var response = await GetUpdatesByTypeAsync<AreaTable>("area");
                    if (!response.ValidateResponse())
                        return false;

                    await areaRepository.InsertOrReplaceAsync(response.Result);

                }

                // UPDATE CRAGS
                if (checkForModelObj.crags_modified > 0)
                {
                    //var allCrags = (await cragRepository.GetAsync()).ToList();
                    //int[] userCrags = allCrags.Select(x => x.crag_id).ToArray();

                    //var resp = await httpHelper.PostAsync<IList<CragExtended>>(ApiUrls.Url_M_CheckForUpdatesCrags(AppSetting.APP_ID), userCrags);

                    debugInfo.AppendLine("Update Crags;");
                    var response = await GetUpdatesByTypeAsync<CragExtended>("crag");
                    if (!response.ValidateResponse())
                        return false;

                    var dbCrags = (await cragRepository.GetAsync()).ToList();
                    foreach (var co in response.Result)
                    {
                        var dbCrag = dbCrags.FirstOrDefault(c => c.crag_id == co.crag_id);
                        co.is_downloaded = dbCrag?.is_downloaded ?? false;
                        co.Unlocked = dbCrag?.Unlocked ?? false;
                    }

                    // ROUTE ISSUES
                    debugInfo.AppendLine("Update Route issues;");
                    //select only those, that do exist in the database and is_downloaed == true
                    var updateCragsForIssueUpdates = response.Result.Where(a => a.is_downloaded);
                    foreach (CragExtended ce in updateCragsForIssueUpdates)
                    {
                        var issueResponse = await GetUpdatesByUrl<IssueTable>(ApiUrls.Url_M_GetRouteIssueData(ce.crag_id, AppConstant.DefaultDate));

                        if (!issueResponse.ValidateResponse())
                            return false;

                        await issueRepository.InsertOrReplaceAsync(issueResponse.Result);
                    }

                    // CRAG IMAGES
                    debugInfo.AppendLine("Update Crags images;");
                    var cragImagesObj = response.Result
                            .Where(c => !string.IsNullOrEmpty(c.crag_image))// && !string.IsNullOrEmpty(c.crag_portrait_image) && !string.IsNullOrEmpty(c.crag_landscape_image))
                            .Select(x => new CragImageTable()
                            {
                                crag_id = x.crag_id,
                                crag_image = x.crag_image,
                            //crag_portrait_image = x.crag_portrait_image,
                            //crag_landscape_image = x.crag_landscape_image
                        });

                    await cragRepository.InsertOrReplaceAsync(response.Result);
                    await cragImageRepository.InsertOrReplaceAsync(cragImagesObj);

                    //Update active crag if disabled
                    debugInfo.AppendLine("Update active crag;");
                    var activeId = Settings.ActiveCrag;
                    var currCrag = await cragRepository.GetAsync(activeId);
                    if (currCrag != null)
                    {
                        if (!currCrag.is_enabled || !currCrag.is_app_store_ready || !currCrag.HasLocation)
                        {
                            currCrag = (await cragRepository.GetAsync(c => c.is_enabled
                                                                        && c.is_app_store_ready
                                                                        && c.is_downloaded
                                                                        && c.crag_latitude != null
                                                                        && c.crag_longitude != null
                                                                        && c.crag_latitude != 0
                                                                        && c.crag_longitude != 0)
                                          ).OrderBy(crag => crag.crag_sort_order)
                                           .FirstOrDefault();
                            Settings.MapSelectedCrag = Settings.ActiveCrag = currCrag?.crag_id ?? 0;
                            MainMasterDetailViewModel.Instance?.FillMenuItems();
                        }
                    }
                }

                if (checkForModelObj.maps_modified > 0)
                {
                    debugInfo.AppendLine("Update Map;");
                    var response = await GetUpdatesByTypeAsync<MapData>("map");

                    if (!response.ValidateResponse())
                        return false;

                    var maps = response.Result.Select((m) => new MapTable
                    {
                        map_id = m.map_id,
                        crag_id = m.crag_id,
                        map_name = m.map_name,
                        imagedata = m.map_image,
                        width = m.width,
                        height = m.height,
                        sort_order = m.sort_order,
                        is_enabled = m.is_enabled
                    });

                    await mapRepository.InsertOrReplaceAsync(maps);
                }

                if (checkForModelObj.map_regions_modified > 0)
                {
                    debugInfo.AppendLine("Update MapRegion;");
                    var response = await GetUpdatesByTypeAsync<MapRegion>("mapregion");

                    if (!response.ValidateResponse())
                        return false;

                    var cragSectorMaps = response.Result.Select((csm) => new CragSectorMapTable
                    {
                        id = $"{csm.crag_id}+{csm.sector_id}+{csm.map_id}",
                        crag_id = csm.crag_id,
                        map_id = csm.map_id,
                        sector_id = csm.sector_id,
                        x = csm.x,
                        y = csm.y,
                        height = csm.height,
                        width = csm.width,
                    });

                    await cragSectorMapRepository.InsertOrReplaceAsync(cragSectorMaps);
                }

                string cragids = "";
                var myCrags = (await cragRepository.GetAsync()).ToList();


                //var dcrags = myCrags.Where(x => x.is_downloaded).Select(x => x.crag_id).ToArray();

                //Need to grab all the crags rather than only downloaded 
                var dcrags = myCrags.Select(x => x.crag_id).ToArray();
                if (dcrags.Any())
                {
                    cragids = string.Join(",", dcrags);

                    // UPDATE SECTORS
                    if (checkForModelObj.sectors_modified > 0)
                    {
                        debugInfo.AppendLine("Update Sectors;");
                        var sectorResponse = await GetUpdatesByCragAndType<SectorTable>(cragids, "sector");
                        //sectorObj = await GetUpdatesByCragAndType<SectorTable>("sector");
                        if (!sectorResponse.ValidateResponse())
                            return false;

                        if (sectorResponse.Result.Count > 0)
                        {
                            await sectorRepository.InsertOrReplaceAsync(sectorResponse.Result);
                            var toposToInsert = new List<TopoTable>();
                            var toposToUpdate = new List<TopoTable>();
                            var allToposFromDb = await topoRepository.GetAsync();
                            var downloadedCragIds = (await cragRepository.GetAsync(c => c.is_downloaded)).Select(c => c.crag_id);

                            foreach (var sector in sectorResponse.Result)
                            {
                                if (!downloadedCragIds.Any(c => c == sector.crag_id))
                                    continue;

                                var existInDb = allToposFromDb.FirstOrDefault(topoFromDb => topoFromDb.sector_id == sector.sector_id);
                                var topoResponse = await httpHelper.GetAsync<string>(ApiUrls.Url_TopoImageServer_Get(sector.sector_id, AppLastUpdateDate));
                                if (!topoResponse.ValidateResponse())
                                    return false;
                                var topo = new TopoTable();
                                topo.sector_id = sector.sector_id;
                                topo.topo_json = topoResponse.Result;
                                topo.upload_date = GeneralHelper.GetCurrentDate("yyyyMMdd");

                                if (existInDb != null)
                                {
                                    var existTopoImages = JsonConvert.DeserializeObject<List<Model.ResponseModels.TopoImageResponseModel>>(existInDb.topo_json);
                                    var topoImages = JsonConvert.DeserializeObject<List<Model.ResponseModels.TopoImageResponseModel>>(topoResponse.Result);

                                    foreach (var topoImg in topoImages)
                                    {
                                        var tp = existTopoImages.FirstOrDefault(x => x.image.name == topoImg.image.name);
                                        if ((tp != null) && (topoImg.image.data == ""))
                                        {
                                            topoImg.image.data = tp.image.data;
                                        }
                                    }
                                    string topoJson = JsonConvert.SerializeObject(topoImages);
                                    topo.topo_json = topoJson;

                                    topo.topo_id = existInDb.topo_id;
                                    toposToUpdate.Add(topo);
                                }
                                else
                                {
                                    toposToInsert.Add(topo);
                                }
                            }

                            var reslutInsertedTopos = await topoRepository.InsertAllAsync(toposToInsert);
                            var resultUpdatedTopos = await topoRepository.UpdateAllAsync(toposToUpdate);
                        }
                    }

                    // UPDATE ROUTES
                    if (checkForModelObj.routes_modified > 0)
                    {
                        debugInfo.AppendLine("Update routes;");
                        //TODO: Review: why are we even sending crag ids here if all should be grabbed? Same on Splash page.
                        var routeResponse = await GetUpdatesByCragAndType<RouteTable>(cragids, "route");
                        if (!routeResponse.ValidateResponse())
                            return false;

                        await routeRepository.InsertOrReplaceAsync(routeResponse.Result);
                    }

                    //TODO: Change cragIds to be only downloaded. Probably if Erwin takes away crag ids for sectors and routes we can have only downloaded cragIds
                    if (checkForModelObj.trails_modified > 0)
                    {
                        debugInfo.AppendLine("Update trails;");
                        var trailResponse = await GetUpdatesByCragAndType<TrailTable>(cragids, "trail");
                        if (!trailResponse.ValidateResponse())
                            return false;

                        await trailRepository.InsertOrReplaceAsync(trailResponse.Result);
                    }

                    if (checkForModelObj.parkings_modified > 0)
                    {
                        debugInfo.AppendLine("Update parking;");
                        var parkingResponse = await GetUpdatesByCragAndType<ParkingTable>(cragids, "parking");
                        if (!parkingResponse.ValidateResponse())
                            return false;

                        await parkingRepository.InsertOrReplaceAsync(parkingResponse.Result);
                    }
                }

                // UPDATE GRADES
                if (checkForModelObj.grades_modified > 0)
                {
                    debugInfo.AppendLine("Update grades;");
                    var gradesResponse = await GetUpdatesByTypeAsync<GradeTable>("grade");
                    if (!gradesResponse.ValidateResponse())
                        return false;

                    await gradeRepository.DeleteAll();
                    var insertedGrades = await gradeRepository.InsertAllAsync(gradesResponse.Result);
                }

                // UPDATE GRADE BUCKETS
                debugInfo.AppendLine("Update grade buckets;");
                var bucketsResponse = await GetUpdatesByUrl<BucketTable>(ApiUrls.Url_M_GetGradesByAppId);
                if (!bucketsResponse.ValidateResponse())
                    return false;

                await bucketRepository.DeleteAll();
                var insertedGradeBuckets = await bucketRepository.InsertAllAsync(bucketsResponse.Result);

                // UPDATE TECH GRADES
                debugInfo.AppendLine("Update tech grades;");
                var techGradesResponse = await GetUpdatesByUrl<TechGradeTable>(ApiUrls.Url_M_GetTTechGrades);
                if (!techGradesResponse.ValidateResponse())
                    return false;

                await techGradeRepository.DeleteAll();
                var insertedTeckGrades = await techGradeRepository.InsertAllAsync(techGradesResponse.Result);

                //INSERT ISSUE DATA ============================================
                debugInfo.AppendLine("Insert issue data;");
                await issueCategoryRepository.DeleteAll();
                var issueCategories = await GetUpdatesByUrl<IssueCategoryTable>(ApiUrls.Url_M_GetIssueData(0));
                if (!issueCategories.ValidateResponse())
                    return false;
                await issueCategoryRepository.InsertAllAsync(issueCategories.Result);

                await issuetypeRepository.DeleteAll();
                var issueTypes = await GetUpdatesByUrl<IssueTypeTable>(ApiUrls.Url_M_GetIssueData(1));
                if (!issueTypes.ValidateResponse())
                    return false;
                await issuetypeRepository.InsertAllAsync(issueTypes.Result);

                await issuetypedetailRepository.DeleteAll();
                var issueTypeDetails = await GetUpdatesByUrl<IssueTypeDetailTable>(ApiUrls.Url_M_GetIssueData(2));
                if (!issueTypeDetails.ValidateResponse())
                    return false;
                await issuetypedetailRepository.InsertAllAsync(issueTypeDetails.Result);

                await issueCategoryIssueTypeLinkRepository.DeleteAll();
                var issueCategoryLinks = await GetUpdatesByUrl<IssueCategoryIssueTypeLinkTable>(ApiUrls.Url_M_GetIssueData(3));
                if (!issueCategoryLinks.ValidateResponse())
                    return false;
                await issueCategoryIssueTypeLinkRepository.InsertAllAsync(issueCategoryLinks.Result);

                await typetissuetypedetaillinkRepository.DeleteAll();
                var issueTypeDetailLinks = await GetUpdatesByUrl<IssueTypeDetailLinkTable>(ApiUrls.Url_M_GetIssueData(4));
                if (!issueTypeDetailLinks.ValidateResponse())
                    return false;
                await typetissuetypedetaillinkRepository.InsertAllAsync(issueTypeDetailLinks.Result);

                // Insert App Products Ids
                debugInfo.AppendLine("Insert App Products Ids;");
                var listOfProductsResponse = await GetUpdatesByTypeAsync<AppProductTable>("appproduct");
                if (!listOfProductsResponse.ValidateResponse())
                    return false;
                var productsFromDb = await appProductRepository.GetAsync();
                if (productsFromDb != null)
                {
                    //Left outer join to select only those, that do not exist in the database
                    var insertIntoDb = (from remote in listOfProductsResponse.Result
                                        join db in productsFromDb on remote.AppProductId equals db.AppProductId into @group
                                        where @group.Count() == 0
                                        select remote);

                    //Left outer join to select only those, that do exist in the database
                    var updateIntoDb = listOfProductsResponse.Result
                        .Join(productsFromDb, firstItem => firstItem.AppProductId, secondItem => secondItem.AppProductId, (firstItem, secondItem) => firstItem);

                    var reslutInserted = await appProductRepository.InsertAllAsync(insertIntoDb);
                    var resultUpdated = await appProductRepository.UpdateAllAsync(updateIntoDb);
                }
                else
                {
                    var resultInserted = await appProductRepository.InsertAllAsync(listOfProductsResponse.Result);
                }


                // Insert GuideBook data
                if (checkForModelObj.guidebooks_modified > 0)
                {
                    debugInfo.AppendLine("Insert guideBook Data;");
                    var listOfGuidebooksResponse = await GetUpdatesByTypeAsync<GuideBookTable>("guidebook");
                    if (!listOfGuidebooksResponse.ValidateResponse())
                        return false;

                    var allFromDb = await guideBookRepository.GetAsync();
                    if (allFromDb != null)
                    {
                        //Left outer join to select only those, that do not exist in the database
                        var insertIntoDb = (from remote in listOfGuidebooksResponse.Result
                                            join db in allFromDb on remote.GuideBookId equals db.GuideBookId into @group
                                            where @group.Count() == 0
                                            select remote);

                        //Left outer join to select only those, that do exist in the database
                        var updateIntoDb = listOfGuidebooksResponse.Result
                            .Join(allFromDb, firstItem => firstItem.GuideBookId, secondItem => secondItem.GuideBookId, (firstItem, secondItem) => firstItem);

                        var reslutInserted = await guideBookRepository.InsertAllAsync(insertIntoDb);
                        var resultUpdated = await guideBookRepository.UpdateAllAsync(updateIntoDb);
                    }
                    else
                    {
                        var resultInserted = await guideBookRepository.InsertAllAsync(listOfGuidebooksResponse.Result);
                    }
                }

                // INSERT Profile Filter Type
                debugInfo.AppendLine("Insert profile filter Type;");
                await profilefiltertypeRepository.DeleteAll();
                var profileFilterTypesResponse = await GetUpdatesByUrl<ProfileFilterTypeTable>(ApiUrls.Url_M_ProfileFilterType);
                if (!profileFilterTypesResponse.ValidateResponse())
                    return false;

                await profilefiltertypeRepository.InsertAllAsync(profileFilterTypesResponse.Result);

                // INSERT Profile Filter
                debugInfo.AppendLine("Insert Profile Filter;");
                await profilefilterRepository.DeleteAll();
                var profileFiltersResponse = await GetUpdatesByUrl<ProfileFilterTable>(ApiUrls.Url_M_ProfileFilter);
                if (!profileFiltersResponse.ValidateResponse())
                    return false;

                await profilefilterRepository.InsertAllAsync(profileFiltersResponse.Result);

                isUpdating = false;
                return true;

            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.MakeUpdate),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = debugInfo.ToString()
                });

                return false;
            }
        }

	    public async Task<string> RunCheckForUpdates(bool isFromCheckForUpdates, string lastUpdate)
	    {
            if (GuestHelper.IsGuest)
                return "Thanks for updating";

		    try
		    {
			    if (string.IsNullOrEmpty(lastUpdate))
			    {
				    this.lastUpdate =
					    await appSettingsRepository.ExecuteScalarAsync<string>("Select [UPDATED_DATE] From [APP_SETTING]");
			    }
			    else
			    {
				    this.lastUpdate = lastUpdate;
			    }

                if (isUpdating)
                {
                    return "Updating in progress";
                }
                isUpdating = true;

                var response = await httpHelper.GetAsync<CheckForUpdateModel>(ApiUrls.Url_M_AvailableUpdates(AppLastUpdateDate));

                if (!response.ValidateResponse())
                {
                    isUpdating = false;
                    return string.Empty;
                }


                checkForModelObj = response.Result;

                var appSetting = await appSettingsRepository.FindAsync(lastdate => lastdate.ID == 1);
                appSetting.UPDATED_DATE = checkForModelObj.updated_date;


                if (checkForModelObj == null
                    || (checkForModelObj.areas_modified
                        + checkForModelObj.crags_modified
                        + checkForModelObj.guidebooks_modified
                        + checkForModelObj.routes_modified
                        + checkForModelObj.sectors_modified
                        + checkForModelObj.maps_modified
                        + checkForModelObj.map_regions_modified
                        + checkForModelObj.grades_modified
                        + checkForModelObj.parkings_modified
                        + checkForModelObj.trails_modified <= 0))
                {
                    await appSettingsRepository.UpdateAsync(appSetting);
                    await purchasedCheckService.UpdateStateAsync();
                    isUpdating = false;
                    return "Your app is up to date.";
                }
               
			    var success = await MakeUpdate();
                if (!success)
                {
                    isUpdating = false;
                    return string.Empty;
                }

                await purchasedCheckService.UpdateStateAsync();

                await appSettingsRepository.UpdateAsync(appSetting);
                isUpdating = false;
                return "Thanks for updating.";
		    }
			catch (Exception exception)
			{
                isUpdating = false;
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = $"{nameof(this.RunCheckForUpdates)}",
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data = $"lastUpdate = {AppLastUpdateDate}, data = {exception.Data}"
				});
			}

            isUpdating = false;
            return null;
	    }

        //================= Added by Sandeep on 23-Jun-2017=============
        async Task<OperationResult<IList<SectorTable>>> HttpGetConsensusSectors()
        {
            //httpClinetHelper.ChangeTokens(ApiUrls.Url_M_GetConsensusSectors, Settings.AccessToken);
            var consensusSectorsobj = new ConsensusModel
            {
                app_id = Convert.ToInt32(Common.Constants.AppSetting.APP_ID),
                app_date_last_updated = AppLastUpdateDate
            };

            //string consensusSectorsjson = JsonConvert.SerializeObject(consensusSectorsobj);
            //var tsector_response = await httpClinetHelper.Post<List<SectorTable>>(consensusSectorsjson);

            var response = await httpHelper.PostAsync<IList<SectorTable>>(ApiUrls.Url_M_GetConsensusSectors, consensusSectorsobj);
            return response;
        }

        //================= Added by Sandeep on 23-Jun-2017=============
        async Task<OperationResult<IList<SectorTable>>> HttpGetConsensusRoutes()
        {
            //httpClinetHelper.ChangeTokens(ApiUrls.Url_M_GetConsensusRoutes, Settings.AccessToken);
            var consensusRoutesobj = new ConsensusModel
            {
                app_id = Convert.ToInt32(Common.Constants.AppSetting.APP_ID),
                app_date_last_updated = AppLastUpdateDate
            };

            //string consensusRoutesjson = JsonConvert.SerializeObject(consensusRoutesobj);
            var response = await httpHelper.PostAsync<IList<SectorTable>>(ApiUrls.Url_M_GetConsensusSectors, consensusRoutesobj);
            return response;
        }

		private async Task<OperationResult<IList<T>>> GetUpdatesByCragAndType<T>(string cragids, string type)
        {
            //httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_M_GetUpdatesByCragAndType, cragids, AppLastUpdateDate, type), Settings.AccessToken);

            var response = await httpHelper.GetAsync<IList<T>>(ApiUrls.Url_M_GetUpdatesByCragAndType(cragids, AppLastUpdateDate, type, false));
            return response;
        }

        private async Task<OperationResult<IList<T>>> GetUpdatesByTypeAsync<T>(string type)
        {
            //httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_M_GetCheckForUpdatesByType, AppSetting.APP_ID, AppLastUpdateDate, type, true), Settings.AccessToken);
            var response = await httpHelper.GetAsync<IList<T>>(ApiUrls.Url_M_GetCheckForUpdatesByType(AppLastUpdateDate, type));
            return response;
        }

        private async Task<OperationResult<IList<T>>> GetUpdatesByUrl<T>(string url)
        {
            //httpClinetHelper.ChangeTokens(string.Format(url, appId, AppLastUpdateDate), Settings.AccessToken);
            var response = await httpHelper.GetAsync<IList<T>>(url);
            return response;
        }
    }
}
