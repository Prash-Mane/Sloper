using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using Plugin.InAppBilling.Abstractions;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.RequestModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Prism.Services;
using Xamarin.Forms;

namespace SloperMobile.Common.Services
{
	public class InAppPurchaseService : IInAppPurchase
	{
		private readonly IInAppBilling billing;
        private readonly IUserDialogs userDialogs;
        private readonly IConnectivity connectivity;
        private readonly IExceptionSynchronizationManager exceptionSynchronizationManager;
		private readonly IPurchaseValidation purchaseValidation;
        readonly IPurchasedCheckService purchasedCheckService;
        readonly ISynchronizationManager synchronizationManager;
		private int appProductId;
		private int sloperId;
		private string receiptData;

        public static bool IsPurchaseInProgress { get; private set; } //true when native modal pop-ups show (that triggeres app resume)


        public InAppPurchaseService(
			IInAppBilling billing,
            IUserDialogs userDialogs,
            IConnectivity connectivity,
            IExceptionSynchronizationManager exceptionSynchronizationManager,
            IPurchasedCheckService purchasedCheckService,
			IDependencyService dependencyService,
            ISynchronizationManager synchronizationManager)
		{
			this.billing = billing;
            this.userDialogs = userDialogs;
			this.connectivity = connectivity;
			this.exceptionSynchronizationManager = exceptionSynchronizationManager;
            this.purchasedCheckService = purchasedCheckService;
            this.synchronizationManager = synchronizationManager;
			if (Device.RuntimePlatform == Device.iOS)
			{
				this.purchaseValidation = dependencyService.Get<IPurchaseValidation>();
				purchaseValidation.ValidatePurchase += ValidatePurchase;
			}										  
		}

		private void ValidatePurchase(string signedData)
		{
			receiptData = signedData;
		}

		private async Task SaveConnectAsync(Func<Task> toDoFunc, string productId)
		{
            if (!connectivity.IsConnected)
			{
				userDialogs.HideLoading();
				await userDialogs.AlertAsync("No Internet Connection.");
                return;
            }

            try
            {
				var connected = await billing.ConnectAsync();
				if (!connected)
				{
					await exceptionSynchronizationManager.LogException(new ExceptionTable
					{
						Method = nameof(this.SaveConnectAsync),
						Page = this.GetType().Name,
						Line = 33,
						Exception = "Cann't connect to purchase server",
						Data = productId
					});

					return;
				}

				await toDoFunc();
			}
			catch (InAppBillingPurchaseException exception)
			{
				await exceptionSynchronizationManager.LogException(new ExceptionTable
				{
					Method = nameof(this.SaveConnectAsync),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data =
						$"productId = {productId}, exceptionData = {JsonConvert.SerializeObject(exception.Data)}"
				});
            }
			catch (Exception exception)
			{
				await exceptionSynchronizationManager.LogException(new ExceptionTable
				{
					Method = nameof(this.SaveConnectAsync),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data =
						$"productId = {productId}, exceptionData = {JsonConvert.SerializeObject(exception.Data)}"
				});
            }
			finally
			{
				await billing.DisconnectAsync();
			}
		}

        private async Task<bool> ValidatePurchase(InAppBillingPurchase inAppBillingPurchase, int sloperId, string productId, int productTypeId)
        {
            var unlockModel = new UnlockSloperRequestModel();
            if (inAppBillingPurchase == null)
            {
                //await userDialogs.AlertAsync("Is not purchased");
                await exceptionSynchronizationManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.CreatePurchaseAsync),
                    Page = this.GetType().Name,
                    Line = 30,
                    Exception = "Did not purchase",
                    Data = productId
                });
                return false;
            }
            else
            {
                MapInAppPurchaseToUnlockModel(inAppBillingPurchase, sloperId, unlockModel, productTypeId);
                var jsonReceipt = JsonConvert.SerializeObject(unlockModel);
                await synchronizationManager.SaveReceipt(jsonReceipt);
                return true;
            }
        }

        private void MapInAppPurchaseToUnlockModel(InAppBillingPurchase inAppBillingPurchase, int sloperId, UnlockSloperRequestModel unlockModel, int productTypeId)
        {
            unlockModel.SloperId = sloperId;
            unlockModel.PurchaseId = inAppBillingPurchase.Id;
			unlockModel.AppProductId = appProductId;
			unlockModel.AppServiceId = inAppBillingPurchase.ProductId;
            unlockModel.UnlockTypeId = productTypeId;
			unlockModel.DeviceType = Device.RuntimePlatform == Device.Android ? 1 : 2;
            unlockModel.PurchaseToken = inAppBillingPurchase.PurchaseToken;
            unlockModel.TransactionDateUtc = inAppBillingPurchase.TransactionDateUtc;
			unlockModel.AppID = long.Parse(AppSetting.APP_ID);
			unlockModel.PackageName = Settings.AppPackageName;
	        if (Device.RuntimePlatform == Device.iOS)
	        {
		        unlockModel.ReceiptData = receiptData;
	        }
        }

        public async Task<bool> CreatePurchaseAsync(int sloperId, int productTypeId, string productId, int appProductId)
		{
            IsPurchaseInProgress = true;
            bool result = false;
            await SaveConnectAsync(async () =>
			{
				try
				{
					this.sloperId = sloperId;
					this.appProductId = appProductId;
					var purchase = await billing.PurchaseAsync(productId, ItemType.Subscription, AppConstant.Payload, purchaseValidation);
					using (userDialogs.Loading("Completing Purchase..."))
					{
						//possibility that a null came through.
						result = await ValidatePurchase(purchase, sloperId, productId, productTypeId);
                        if (result)
                            await purchasedCheckService.UpdateStateAsync();
                    }
                }
				catch (InAppBillingPurchaseException exception)
                {
					if (exception.PurchaseError != PurchaseError.UserCancelled && exception.PurchaseError != PurchaseError.AlreadyOwned)
					{
						//await userDialogs.AlertAsync($"Is not purchased. {exception.Message}");
						await exceptionSynchronizationManager.LogException(new ExceptionTable
						{
							Method = nameof(this.CreatePurchaseAsync),
							Page = this.GetType().Name,
							StackTrace = exception.StackTrace,
                            Exception = $"{exception.PurchaseError}",
                            Data = $"InAppBillingPurchaseException, sloperId = {sloperId}, productId = {productId}, exceptionMsg = {exception.Message}"
						});
					}
                }
				catch (Exception exception)
                {
                    //await userDialogs.AlertAsync($"Is not purchased. {exception.Message}");
                    await exceptionSynchronizationManager.LogException(new ExceptionTable
                    {
                        Method = nameof(this.CreatePurchaseAsync),
                        Page = this.GetType().Name,
                        StackTrace = exception.StackTrace,
                        Exception = exception.Message,
                        Data = $"{exception.GetType()}, sloperId = {sloperId}, productId = {productId}"
                    });
                }
			}, productId);

            EndPurchaseProgress();

            return result;
		}

        async Task EndPurchaseProgress() //to cancel AppResume call
        {
            await Task.Delay(1000);
            IsPurchaseInProgress = false;
        }

		//public async Task<bool> IsPurchasedAsync(string productId)
		//{
		//	var result = false;
		//	await SaveConnectAsync(async () =>
		//	{
  //              try
		//		{
		//			this.appProductId = appProductId;
		//			var productInfo = await billing.GetPurchasesAsync(ItemType.Subscription);
  //                  result = productInfo != null && productInfo.ToList().Count > 0;
  //              }
  //              catch (InAppBillingPurchaseException exception)
  //              {
  //                  //await userDialogs.AlertAsync($"Is not purchased. {exception.Message}");
  //                  await exceptionSynchronizationManager.LogException(new ExceptionTable
  //                  {
  //                      Method = nameof(this.IsPurchasedAsync),
  //                      Page = this.GetType().Name,
  //                      StackTrace = exception.StackTrace,
  //                      Exception = exception.Message,
  //                      Data = $"productId = {productId}, exceptionData = {JsonConvert.SerializeObject(exception.Data)}"
  //                  });
  //              }
  //              catch (Exception exception)
  //              {
  //                  //await userDialogs.AlertAsync($"Is not purchased. {exception.Message}");
  //                  await exceptionSynchronizationManager.LogException(new ExceptionTable
  //                  {
  //                      Method = nameof(this.IsPurchasedAsync),
  //                      Page = this.GetType().Name,
  //                      StackTrace = exception.StackTrace,
  //                      Exception = exception.Message,
  //                      Data = $"productId = {productId}, exceptionData = {JsonConvert.SerializeObject(exception.Data)}"
  //                  });
  //              }
  //          }, productId);

		//	return result;
		//}

        public async Task<bool> PurchaseAppAsync(string productId, int appProductId)
        {
            IsPurchaseInProgress = true;
            bool result = false;
            await SaveConnectAsync(async () =>
            {
                try
                {
                    this.appProductId = appProductId;
                    var purchase = await billing.PurchaseAsync(productId, ItemType.Subscription, AppConstant.Payload, purchaseValidation);
					using (userDialogs.Loading("Completing Purchase..."))
					{
						//possibility that a null came through.
						result = await ValidatePurchase(purchase, -1, productId, 1);
                        await purchasedCheckService.UpdateStateAsync();
					}
                }
                catch (InAppBillingPurchaseException exception)
                {
					if (exception.PurchaseError != PurchaseError.UserCancelled && exception.PurchaseError != PurchaseError.AlreadyOwned)
					{
						//await userDialogs.AlertAsync($"Is not purchased. {exception.Message}");
						await exceptionSynchronizationManager.LogException(new ExceptionTable
						{
							Method = nameof(this.PurchaseAppAsync),
							Page = this.GetType().Name,
							StackTrace = exception.StackTrace,
							Exception = $"{exception.PurchaseError}",
							Data = $"InAppBillingPurchaseException, productId = {productId}, exceptionMsg = {exception.Message}"
						});
					}
                }
                catch (Exception exception)
                {
                    //await userDialogs.AlertAsync($"Is not purchased. {exception.Message}");
                    await exceptionSynchronizationManager.LogException(new ExceptionTable
                    {
                        Method = nameof(this.PurchaseAppAsync),
                        Page = this.GetType().Name,
                        StackTrace = exception.StackTrace,
                        Exception = exception.Message,
                        Data = $"{exception.GetType()}, productId = {productId}"
                    });
                }
            }, productId);

            EndPurchaseProgress();

            return result;
        }
    }
}
