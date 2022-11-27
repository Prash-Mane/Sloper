using Newtonsoft.Json;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.ViewModel.MasterDetailViewModels;
using SloperMobile.Views.FlyoutPages;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;
using System.Threading;
using Acr.UserDialogs;

namespace SloperMobile.Common.Services
{
    public class DownloadCragService : IDownloadCragService
	{
		private readonly IRepository<TechGradeTable> techGradeRepository;
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IRepository<RouteTable> routeRepository;		   
		private readonly IRepository<SectorTable> sectorRepository;	 
		private readonly IRepository<TopoTable> topoRepository;			 		 
		private readonly IRepository<GradeTable> gradeRepository;
		private readonly IRepository<BucketTable> bucketRepository;
		private readonly IRepository<IssueTable> issueRepository;
		private readonly IRepository<MapTable> mapRepository;
        private readonly IRepository<CragSectorMapTable> cragSectorMapRepository;
        readonly IRepository<ParkingTable> parkingRepository;
        readonly IRepository<TrailTable> trailRepository;
        private readonly IExceptionSynchronizationManager exceptionManager;
        readonly IHttpHelper httpHelper;
        private int selectedCragId;
        CragExtended crag;
        CancellationToken cancelToken;
        int totalRequests = 15; //number of tasks we're checking in progress +1 for the saving
        decimal currentCounter;

        public DownloadCragService(
            IRepository<TechGradeTable> techGradeRepository,
            IRepository<CragExtended> cragRepository,
            IRepository<RouteTable> routeRepository,
            IRepository<SectorTable> sectorRepository,
            IRepository<TopoTable> topoRepository,
            IRepository<GradeTable> gradeRepository,
            IRepository<BucketTable> bucketRepository,
            IRepository<IssueTable> issueRepository,
            IRepository<MapTable> mapRepository,
            IRepository<CragSectorMapTable> cragSectorMapRepository,
            IRepository<ParkingTable> parkingRepository,
            IRepository<TrailTable> trailRepository,
            IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper)
        {
            this.techGradeRepository = techGradeRepository;
            this.cragRepository = cragRepository;
            this.routeRepository = routeRepository;
            this.sectorRepository = sectorRepository;
            this.topoRepository = topoRepository;
            this.gradeRepository = gradeRepository;
            this.bucketRepository = bucketRepository;
            this.issueRepository = issueRepository;
            this.mapRepository = mapRepository;
            this.cragSectorMapRepository = cragSectorMapRepository;
            this.parkingRepository = parkingRepository;
            this.trailRepository = trailRepository;
            this.exceptionManager = exceptionManager;
            this.httpHelper = httpHelper;
        }


        public event EventHandler<(string message, decimal percents)> ProgressChanged;

		public async Task<bool> DownloadAsync(int selectedCragId, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
                cancelToken = cancellationToken;
                currentCounter = 0;

                crag = await cragRepository.FindAsync(c => c.crag_id == selectedCragId);
                CalculateTotal();

                var sectors = await sectorRepository.GetAsync(s => s.crag_id == selectedCragId);
				this.selectedCragId = selectedCragId;

				//todo: remove routes and sectors download logic
				/*
                UpdateProgress("Loading\nSectors", 0);
				this.selectedCragId = selectedCragId;
                var sectors = await GetDataAsync<SectorTable>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.SectorRequestType));
                if (!sectors.ValidateResponse())
                    return false;
                
                UpdateProgress("Loading\nRoutes");
                var routes = await GetDataAsync<RouteTable>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.RouteRequestType));
                if (!routes.ValidateResponse())
                    return false;
                */
				UpdateProgress("Loading\nRoute Issues");
                var issues = await GetUpdatesByUrl<IssueTable>(ApiUrls.Url_M_GetRouteIssueData(selectedCragId, AppConstant.DefaultDate));
                if (!issues.ValidateResponse())
                    return false;

                UpdateProgress("Loading\nMaps");
                var mapData = await GetDataAsync<MapData>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.MapRequestType));
                if (!mapData.ValidateResponse())
                    return false;

                UpdateProgress("Loading\nMap Regions");
                var regions = await GetDataAsync<MapRegion>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.MapRegionRequestType));
                if (!regions.ValidateResponse())
                    return false;

                UpdateProgress("Loading\nTrails");
                var trails = await GetDataAsync<TrailTable>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.TrailRequestType));
                if (!trails.ValidateResponse())
                    return false;

                UpdateProgress("Loading\nParking Lots");
                var parkings = await GetDataAsync<ParkingTable>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.ParkingRequestType));
                if (!parkings.ValidateResponse())
                    return false;

                UpdateProgress("Loading\nTopos");
                var topos = await GetToposForSectors(sectors);
                if (!topos.ValidateResponse())
                    return false;

                UpdateProgress("Syncing\nGrades");
                var grades = await GetGradesAsync();
                if (!grades.ValidateResponse())
                    return false;

                UpdateProgress("Syncing\nGrade Buckets");
                var buckets = await GetBucketsAsync();
                if (!buckets.ValidateResponse())
                    return false;

                UpdateProgress("Syncing\nTech Grades");
                var techGrades = await GetTechGradesAsync();
                if (!techGrades.ValidateResponse())
                    return false;

                if (crag != null && crag.crag_role_id != null)
				{
                    UpdateProgress("Syncing\nNews");
                    //ignore result?
                    await HttpAssignCragRole(crag.crag_role_id);
				}

                // Add user crag history in TUSER_CRAG table on server.
                UpdateProgress("Syncing\nDownload Status");
                //ignore result?
                await HttpAddUserCragHistory();

                //save after all http requests are finished
                //UpdateProgress("Downloading Crag Guide");
                UpdateProgress("Syncing\nData");

                //await SaveSectorsAsync(sectors.Result);
                //await SaveRoutesAsync(routes.Result);

                await SaveRouteIssues(issues.Result);
                await SaveGradesAsync(grades.Result);
                await SaveBucketsAsync(buckets.Result);
                await SaveTechGradesAsync(techGrades.Result);
                await SaveMapsAsync(mapData.Result);
                await SaveRegionsAsync(regions.Result);
                await SaveTopos(topos.Result);

                await trailRepository.RemoveAsync((t) => t.crag_id == selectedCragId); //just to be safe if something was missed and not deleted
                await trailRepository.InsertOrReplaceAsync(trails.Result);

                await parkingRepository.RemoveAsync((p) => p.crag_id == selectedCragId);
                await parkingRepository.InsertOrReplaceAsync(parkings.Result);

                if (crag != null)
                {
                    //flag crag as downloaded
                    crag.is_downloaded = true;
                    await cragRepository.UpdateAsync(crag);
                    if (Settings.ActiveCrag == 0)
                    {
                        Settings.ActiveCrag = crag.crag_id;
                        //UpdateMenuList();
                    }
                }

                UpdateProgress("Crag Download\nFinished!");

                GeneralHelper.CragModified?.Invoke(this, crag);
                await GeneralHelper.UpdateCragsDownloadedStateAsync();

				return true;
			}
			catch (Exception exception)
			{
                if (exception is TaskCanceledException || exception is OperationCanceledException)
                    return false;

				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.DownloadAsync),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data = $"selectedCragId = {selectedCragId}, data = {JsonConvert.SerializeObject(exception.Data)}"
				});

				return false;
			}
		}

		//public async void UpdateMenuList()
		//{
			//try
			//{
                //var FlyoutPage = Application.Current.MainPage?.Navigation.ModalStack?.OfType<MainFlyoutPage>().FirstOrDefault();
                //if (FlyoutPage == null)
                    //FlyoutPage = Application.Current.MainPage?.Navigation.NavigationStack?.OfType<MainFlyoutPage>().FirstOrDefault();

				//if (FlyoutPage == null)
				//{
				//	return;
				//}

				//var masterPage = FlyoutPage.Master;
				//var bindingContext = masterPage.BindingContext as MainMasterDetailViewModel;
				//MainMasterDetailViewModel.Instance?.FillMenuItems();
		//	}
		//	catch (Exception exception)
		//	{
		//		await exceptionManager.LogException(new ExceptionTable
		//		{
		//			Method = nameof(this.UpdateMenuList),
		//			Page = this.GetType().Name,
		//			StackTrace = exception.StackTrace,
		//			Exception = exception.Message,
		//		});
		//	}
		//}

		private async Task SaveRoutesAsync(IList<RouteTable> routes)
		{
			var allFromDb = await routeRepository.GetAsync();
			if (allFromDb != null)
			{
				//Left outer join to select only those, that do not exist in the database
				var insertIntoDb = (from remote in routes
									join db in allFromDb on remote.route_id equals db.route_id into @group
									where @group.Count() == 0
									select remote);

				//Left outer join to select only those, that do exist in the database
				var updateIntoDb = routes
					.Join(allFromDb, firstItem => firstItem.route_id, secondItem => secondItem.route_id, (firstItem, secondItem) =>
					{
						firstItem.route_id = secondItem.route_id;
						return firstItem;
					});

				try
				{
					var reslutInserted = await routeRepository.InsertAllAsync(insertIntoDb);
					var resultUpdated = await routeRepository.UpdateAllAsync(updateIntoDb);
				}
				catch (SQLiteException exception)
				{
					await exceptionManager.LogException(new ExceptionTable
					{
                        Method = nameof(this.SaveRoutesAsync),
						Page = this.GetType().Name,
						StackTrace = exception.StackTrace,
						Exception = exception.Message,
					});
				}
			}
			else
			{
				var resultInserted = await routeRepository.InsertAllAsync(routes);
			}
		}

        private async Task SaveSectorsAsync(IList<SectorTable> sectors)
		{
			var allFromDb = await sectorRepository.GetAsync();
			if (allFromDb != null)
			{
				//Left outer join to select only those, that do not exist in the database
				var insertIntoDb = (from remote in sectors
									join db in allFromDb on remote.sector_id equals db.sector_id into @group
									where @group.Count() == 0
									select remote);

				//Left outer join to select only those, that do exist in the database
				var updateIntoDb = sectors
					.Join(allFromDb, firstItem => firstItem.sector_id, secondItem => secondItem.sector_id, (firstItem, secondItem) =>
					{
						firstItem.sector_id = secondItem.sector_id;
						return firstItem;
					});

				var reslutInserted = await sectorRepository.InsertAllAsync(insertIntoDb);
				var reslutUpdated = await sectorRepository.UpdateAllAsync(updateIntoDb);

			//	var toposToInsert = new List<TopoTable>();
			//	var toposToUpdate = new List<TopoTable>();
			//	var allToposFromDb = await topoRepository.GetAsync();
			//	foreach (var sector in sectors)
			//	{
			//		var topodict = new Dictionary<string, int>
			//		{
			//			{ "sectorID", sector.sector_id }
			//		};

			//		var existInDb = allToposFromDb.FirstOrDefault(topoFromDb => topoFromDb.sector_id == sector.sector_id);
			//		httpClinetHelper.ChangeTokens(ApiUrls.Url_TopoImageServer_Get, Settings.AccessToken);
			//		var topo_response = await httpClinetHelper.GetJsonString<string>(topodict);
			//		var topo = new TopoTable
			//		{
			//			sector_id = sector.sector_id,
			//			topo_json = topo_response,
			//			upload_date = Helper.GetCurrentDate(AppConstant.DateParseString)
			//		};

			//		if (existInDb != null)
			//		{
			//			topo.topo_id = existInDb.topo_id;
			//			toposToUpdate.Add(topo);
			//		}
			//		else
			//		{
			//			toposToInsert.Add(topo);
			//		}
			//	}

			//	var reslutInsertedTopos = await topoRepository.InsertAllAsync(toposToInsert);
			//	var resultUpdatedTopos = await topoRepository.UpdateAllAsync(toposToUpdate);
			//}
			//else
			//{
				//var reslutInserted = await sectorRepository.InsertAllAsync(sectors);
				//var toposToInsert = new List<TopoTable>();
				//foreach (var sector in sectors)
				//{
				//	var topodict = new Dictionary<string, int>
				//	{
				//		{ "sectorID", sector.sector_id }
				//	};

				//	httpClinetHelper.ChangeTokens(ApiUrls.Url_TopoImageServer_Get, Settings.AccessToken);
				//	var topo_response = await httpClinetHelper.GetJsonString<string>(topodict);
				//	var topo = new TopoTable
				//	{
				//		sector_id = sector.sector_id,
				//		topo_json = topo_response,
				//		upload_date = Helper.GetCurrentDate(AppConstant.DateParseString)
				//	};

				//	toposToInsert.Add(topo);
				//}

				//var reslutInsertedTopos = await topoRepository.InsertAllAsync(toposToInsert);
			}
		}

        async Task<OperationResult<List<TopoTable>>> GetToposForSectors(IList<SectorTable> sectors)
        {
            var topos = new List<TopoTable>();

            var count = sectors.Count;

            var topoStep = Decimal.Divide(4, count);

            if (topoStep * sectors.Count < 4)
                topoStep *= (decimal)1.01; //to avoid losing fraction after divide


            for (int i = 0; i < count; i++)
            {
                var sector = sectors[i];
                UpdateProgress($"Loading\nTopo {i+1} of {sectors.Count}", topoStep);

                var topoResponse = await httpHelper.GetAsync<string>(ApiUrls.Url_TopoImageServer_Get(sector.sector_id), cancellationToken: cancelToken);

                if (!topoResponse.ValidateResponse(false))
                    return OperationResult<List<TopoTable>>.CreateFailure(topoResponse.ErrorMessage);

                if (string.IsNullOrEmpty(topoResponse.Result))
                    continue;

                if (topoResponse.Result.Contains("\"ExceptionMessage\""))//hack. Should never be returned from server-side
                {
                    var errorText = "Getting topo unknown exception";
                    UserDialogs.Instance.Alert(errorText);
                    await exceptionManager.LogException(new ExceptionTable
                    {
                        Method = nameof(GetToposForSectors),
                        Page = this.GetType().Name,
                        StackTrace = topoResponse.Result,
                        Exception = "Error topo response from endpoint"
                    });
                    return OperationResult<List<TopoTable>>.CreateFailure("Error response from endpoint");
                }

                var topo = new TopoTable
                {
                    sector_id = sector.sector_id,
                    topo_json = topoResponse.Result,
                    upload_date = GeneralHelper.GetCurrentDate(AppConstant.DateParseString)
                };
                topos.Add(topo);
            }
            return OperationResult<List<TopoTable>>.CreateSuccessResult(topos);
        }

        private async Task SaveRouteIssues(IList<IssueTable> issues)
		{
			if (issues != null)
			{
				await issueRepository.InsertOrReplaceAsync(issues);
			}
		}

        async Task SaveTopos(List<TopoTable> topos) 
        {
            await topoRepository.InsertOrReplaceAsync(topos);
        }

        public async Task<bool> GetAndSaveGradesAsync()
        {
            var grades = await GetGradesAsync();
            if (!grades.ValidateResponse())
                return false;
         
            await SaveGradesAsync(grades.Result);

            return true;
        }

        public async Task<bool> GetAndSaveBucketsAsync()
        {
            var buckets = await GetBucketsAsync();
            if (!buckets.ValidateResponse())
                return false;
            
            await SaveBucketsAsync(buckets.Result);


            return true;
        }

        public async Task<bool> GetAndSaveTechGradesAsync()
        {
            var techGrades = await GetTechGradesAsync();
            if (!techGrades.ValidateResponse())
                return false;

            await SaveTechGradesAsync(techGrades.Result);

            return true;
        }

        async Task<OperationResult<IList<GradeTable>>> GetGradesAsync() 
        { 
            return await GetDataAsync<GradeTable>(ApiUrls.Url_M_GetUpdatesByType(selectedCragId, AppConstant.GradeRequestType));
        }

        async Task SaveGradesAsync(IList<GradeTable> grades)
        { 
            await gradeRepository.DeleteAll();
            await gradeRepository.InsertAllAsync(grades);  
        }

        async Task<OperationResult<IList<BucketTable>>> GetBucketsAsync() 
        { 
            return await GetDataAsync<BucketTable>(ApiUrls.Url_M_GetGradesByAppId);
        }

        async Task SaveBucketsAsync(IList<BucketTable> gradeBuckets) 
        { 
            await bucketRepository.DeleteAll();
            await bucketRepository.InsertAllAsync(gradeBuckets);
        }

        async Task<OperationResult<IList<TechGradeTable>>> GetTechGradesAsync()
        {
            return await GetDataAsync<TechGradeTable>(ApiUrls.Url_M_GetTTechGrades);
        }

        async Task SaveTechGradesAsync(IList<TechGradeTable> techGrades)
        {
            await techGradeRepository.DeleteAll();
            await techGradeRepository.InsertAllAsync(techGrades);
        }

        private async Task SaveMapsAsync(IList<MapData> mapData)
		{
			
				var maps = mapData
					.Select((m) => new MapTable
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
				//await mapRepository.InsertAllAsync(maps); //is faster, but some data is not cleared yet
				await mapRepository.InsertOrReplaceAsync(maps);
		}

        private async Task SaveRegionsAsync(IList<MapRegion> regions)
		{
			var mapRegions = regions
				.Select((csm) => new CragSectorMapTable
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
			//await cragSectorMapRepository.InsertAllAsync(mapRegions);
			await cragSectorMapRepository.InsertOrReplaceAsync(mapRegions);
		}

        private async Task<OperationResult<IList<T>>> GetDataAsync<T>(string url) =>
            await httpHelper.GetAsync<IList<T>>(url, cancellationToken: cancelToken);

        private async Task<OperationResult<IList<T>>> GetUpdatesByUrl<T>(string url)
		{
			//httpClinetHelper.ChangeTokens(string.Format(url, appId, AppConstant.DefaultDate), Settings.AccessToken);
            var response = await httpHelper.GetAsync<IList<T>>(url);
			return response;
		}

		private async Task<OperationResult<CragHistoryModel>> HttpAddUserCragHistory()
		{
            var response = await httpHelper.GetAsync<CragHistoryModel>(ApiUrls.Url_M_AddUserCragHistory(selectedCragId), cancellationToken: cancelToken);
			return response;
		}

		private async Task<OperationResult<bool>> HttpAssignCragRole(int? crag_role_id)
		{
            var response = await httpHelper.PostAsync<bool>(ApiUrls.Url_SloperUser_AssignRole(crag_role_id), string.Empty, cancellationToken: cancelToken);

			return response;
		}

        void UpdateProgress(string message, decimal increment = 1) 
        {
            cancelToken.ThrowIfCancellationRequested();
            currentCounter += increment;
            var progress = Decimal.Divide(currentCounter, totalRequests);
            ProgressChanged?.Invoke(crag, (message, progress));
        }

        void CalculateTotal() 
        {
            if (crag != null && crag.crag_role_id != null)
            {
                totalRequests = 16;
            }
        }
	}
}
