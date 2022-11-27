using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.IssueModels;
using SloperMobile.Model.ResponseModels;
using Xamarin.Forms;

namespace SloperMobile.Common.Services
{
	public class SynchronizationManager : ISynchronizationManager
	{
		private readonly IRepository<TempAscentTable> ascentProcessRepository;
		private readonly IRepository<TempIssueTable> issueLocalRepository;
		private readonly IRepository<TempRouteImageTable> routeImageRepository;
		private readonly IRepository<RouteTable> routeRepository;
        readonly IRepository<ReceiptTable> receiptRepository;
        readonly IExceptionSynchronizationManager exceptionManager;
		private readonly IConnectivity connectivityManager;
        private readonly IHttpHelper httpHelper;
        private bool issueDataSet;
		private bool ascentDataSet;
		private bool imageDataSend;
		private RouteTable routeData;

		public SynchronizationManager(
			IRepository<TempAscentTable> ascentProcessRepository,
			IRepository<TempIssueTable> issueLocalRepository,
			IRepository<TempRouteImageTable> routeImageRepository,
			IRepository<RouteTable> routeRepository,
            IRepository<ReceiptTable> receiptRepository,
            IExceptionSynchronizationManager exceptionManager,
            IConnectivity connectivityManager,
            IHttpHelper httpHelper)
		{
			this.ascentProcessRepository = ascentProcessRepository;
			this.issueLocalRepository = issueLocalRepository;
			this.routeImageRepository = routeImageRepository;
			this.routeRepository = routeRepository;
			this.connectivityManager = connectivityManager;
            this.receiptRepository = receiptRepository;
            this.exceptionManager = exceptionManager;
			this.connectivityManager.ConnectivityChanged += ConnectionChanged;
            this.httpHelper = httpHelper;
            //this.httpClinetHelper.ChangeTokens(ApiUrls.Url_Asecnt_CreateAscent, Settings.AccessToken);
			CheckTablesDataAsync();

            TrailCollector.Instance.UploadAllFinishedToServerAsync();
		}

		private async Task CheckTablesDataAsync()
		{
            var ascentData = await ascentProcessRepository.GetAsync();
            var issueData = await issueLocalRepository.GetAsync();
            var imageData = await routeImageRepository.GetAsync();

            if (ascentData != null && ascentData.Count > 0)
            {
                ResendAscentToApi();
            }

            if (issueData != null && issueData.Count > 0)
            {
                ResendIssueToApi();
            }

            if (imageData != null && imageData.Count > 0)
            {
                ResendImageToApi();
            }

            var pendingReceipts = await receiptRepository.GetAsync(r => r.userId == Settings.UserID);
            foreach (var receipt in pendingReceipts)
                await SendReceiptToApi(receipt);
		}

		public async Task<AscentReponseModel> SendAscentDataAsync(AscentPostModel model)
		{
			if (connectivityManager.IsConnected)
			{
                var response = await SendAscentToApi(model);

                return response.ValidateResponse() ? response.Result : null;
            }
            else
			{
				ascentDataSet = true;
				return await SaveAscentToDb(model);
			}
		}

		public async Task<IssueResponseModel> SendIssueDataAsync(IssueModel model)
		{
			if (connectivityManager.IsConnected)
			{
                var response = await SendIssueToApi(model);

                return response.ValidateResponse() ? response.Result : null;
            }
            else
			{
				issueDataSet = true;
				return await SaveIssueToDb(model);
			}
		}
		
		public async Task<int> SendImageDataAsync(AscentImageModel imageData)
		{
			if (connectivityManager.IsConnected)
			{
                var response = await SendImageToApi(imageData);

                return response.ValidateResponse() ? response.Result : 0;
            }
			else
			{
				imageDataSend = true;
				return await SaveImageToDb(imageData);
			}
		}

        public async Task SaveReceipt(string receipt) 
        {
            var dbEntry = new ReceiptTable { 
                receiptData = receipt,
                userId = Settings.UserID
            };

            //for some reason, sometimes same purchase could be triggered twice. Let's handle it here if possible
            var sameReceipt = await receiptRepository.GetAsync(r => r.receiptData == receipt);
            if (sameReceipt.Count > 0)
                return;

            await receiptRepository.InsertAsync(dbEntry);

            await SendReceiptToApi(dbEntry);
        }

        async Task SendReceiptToApi(ReceiptTable receiptTable) {
            if (connectivityManager.IsConnected)
            {
                //httpClinetHelper.ChangeTokens(ApiUrls.Url_AppPurchase_UpdateUnlockStatus, Settings.AccessToken);
                //var success = await httpClinetHelper.Post<bool>(receiptTable.receiptData);

                var response = await httpHelper.PostAsync<bool>(ApiUrls.Url_AppPurchase_UpdateUnlockStatus, receiptTable.receiptData);

                if (response.ValidateResponse() && response.Result)
                {
                    await receiptRepository.DeleteAsync(receiptTable);
                    return;
                }

                //log exception if false received from endpoint
                var dbException = new ExceptionTable
                {
                    Date = DateTime.Now,
                    Method = nameof(SendReceiptToApi),
                    Page = nameof(SynchronizationManager),
                    UserId = receiptTable.userId,
                    Data = $"False returned when trying to send\n{receiptTable.receiptData} to\n{ApiUrls.Url_AppPurchase_UpdateUnlockStatus}"
                };
                exceptionManager.LogException(dbException);
            }
        }

		private async void ResendAscentToApi()
		{
			var models = await ascentProcessRepository.GetAsync();
            //httpClinetHelper.ChangeTokens(ApiUrls.Url_Asecnt_CreateAscent, Settings.AccessToken);
			foreach (var model in models)
			{
				var toSend = new AscentPostModel
				{
					ascent_date = model.ascent_date,
					ascent_id = model.ascent_id,
					ascent_type_id = model.ascent_type_id,
					climbing_angle = model.climbing_angle,
					climbing_angle_value = model.climbing_angle_value,
					comment = model.comment,
					grade_id = model.grade_id,
					hold_type = model.hold_type,
					hold_type_value = model.hold_type_value,
					ImageData = model.ImageData,
					ImageName = model.ImageName,
					photo = model.photo,
					rating = model.rating,
					route_id = model.route_id,
					route_style = model.route_style,
					route_style_value = model.route_style_value,
					route_type_id = model.route_type_id,
					tech_grade_id = model.tech_grade_id,
					video = model.video
				};

				await SendAscentToApi(toSend);
			}

			ascentDataSet = false;
			await ascentProcessRepository.DeleteAll();
		}

		private async void ResendIssueToApi()
		{
			var models = await issueLocalRepository.GetAsync();
            //httpClinetHelper.ChangeTokens(ApiUrls.Url_M_LogUserIssue, Settings.AccessToken);
			foreach (var model in models)
			{
				var toSend = new IssueModel
				{
					route_id = model.route_id,
					bolt_numbers = model.bolt_numbers,
					comments = model.comments,
					image = model.image,
					issue_category_id = model.issue_category_id,
					issue_type_detail_id = model.issue_type_detail_id,
					issue_type_id = model.issue_type_id
				};

				await SendIssueDataAsync(toSend);
			}

			issueDataSet = false;
			await issueLocalRepository.DeleteAll();
		}

		private async void ResendImageToApi()
		{
			var models = await routeImageRepository.GetAsync();										
			foreach (var model in models)
			{			 
				if (model.RouteId != routeData?.route_id)
				{
					routeData = await routeRepository.FindAsync(route => route.route_id == model.RouteId);
				}

				if(routeData.route_image_id != 0 && model.ImageType == Enumerators.ImageType.RouteImage)
				{
					continue;
				}

				var ascentImageModel = new AscentImageModel
				{
					AscentId = model.AscentId ?? 0,
					ImageBytes = Convert.FromBase64String(model.ImageBase64),
					RouteId = model.RouteId ?? 0,
					FiveStarAscentCheck = model.FiveStarAscentCheck,
					ImageOrientation = model.ImageOrientation,
					ImageType = model.ImageType,
					appId = model.appId
				};

				var imageResponse = await SendImageToApi(ascentImageModel);
                if (!imageResponse.ValidateResponse())
                    return;
                if (routeData != null)//imageId != 0)
				{
                    routeData.route_image_id = imageResponse.Result;
					await routeRepository.UpdateAsync(routeData);
				}
			}

			imageDataSend = false;
			await routeImageRepository.DeleteAll();
		}

		private async Task<AscentReponseModel> SaveAscentToDb(AscentPostModel model)
		{
			var toSave = new TempAscentTable
			{
				ascent_date = model.ascent_date,
				ascent_id = model.ascent_id,
				ascent_type_id = model.ascent_type_id,
				climbing_angle = model.climbing_angle,
				climbing_angle_value = model.climbing_angle_value,
				comment = model.comment,
				grade_id = model.grade_id,
				hold_type = model.hold_type,
				hold_type_value = model.hold_type_value,
				ImageData = model.ImageData,
				ImageName = model.ImageName,
				photo = model.photo,
				rating = model.rating,
				route_id = model.route_id,
				route_style = model.route_style,
				route_style_value = model.route_style_value,
				route_type_id = model.route_type_id,
				tech_grade_id = model.tech_grade_id,
				video = model.video
			};

			await ascentProcessRepository.InsertAsync(toSave);
			return null;
		}

		private async Task<OperationResult<AscentReponseModel>> SendAscentToApi(AscentPostModel model)
		{
            //         httpClinetHelper.ChangeTokens(ApiUrls.Url_Asecnt_CreateAscent, Settings.AccessToken);
            //var ascentJson = JsonConvert.SerializeObject(model);
            //var response = await httpClinetHelper.Post<AscentReponseModel>(ascentJson);

            var response = await httpHelper.PostAsync<AscentReponseModel>(ApiUrls.Url_Asecnt_CreateAscent, model);

			return response;
		}

		private async Task<IssueResponseModel> SaveIssueToDb(IssueModel model)
		{
			var toSave = new TempIssueTable
			{
				route_id = model.route_id,
				bolt_numbers = model.bolt_numbers,
				comments = model.comments,
				image = model.image,
				issue_category_id = model.issue_category_id,
				issue_type_detail_id = model.issue_type_detail_id,
				issue_type_id = model.issue_type_id
			};

			await issueLocalRepository.InsertAsync(toSave);
			return null;
		}

		private async Task<OperationResult<IssueResponseModel>> SendIssueToApi(IssueModel model)
		{
            //         httpClinetHelper.ChangeTokens(ApiUrls.Url_M_LogUserIssue, Settings.AccessToken);
            //var ascentJson = JsonConvert.SerializeObject(model);
            //var response = await httpClinetHelper.Post<IssueResponseModel>(ascentJson);

            var response = await httpHelper.PostAsync<IssueResponseModel>(ApiUrls.Url_M_LogUserIssue,model);

			return response;
		}

		private async Task<int> SaveImageToDb(AscentImageModel imageData)
		{
			var tempData = new TempRouteImageTable
			{
				AscentId = imageData.AscentId,
				ImageBase64 = Convert.ToBase64String(imageData.ImageBytes),
				RouteId = imageData.RouteId,
				FiveStarAscentCheck = imageData.FiveStarAscentCheck,
				ImageOrientation = imageData.ImageOrientation,
				ImageType = imageData.ImageType,
				appId = imageData.appId
			};

			await routeImageRepository.InsertAsync(tempData);
			return 0;
		}

		private async Task<OperationResult<int>> SendImageToApi(AscentImageModel imageData)
		{
            return await httpHelper.PostAsync<int>(ApiUrls.Url_M_SaveImage, imageData);
		}

		private void ConnectionChanged(object sender, ConnectivityChangedEventArgs e)
		{
			if (e.IsConnected)
			{
				if (ascentDataSet)
				{
					ResendAscentToApi();
				}

				if (issueDataSet)
				{
					ResendIssueToApi();
				}

				if (imageDataSend)
				{
					ResendImageToApi();
				}
			}
		}
	}
}
