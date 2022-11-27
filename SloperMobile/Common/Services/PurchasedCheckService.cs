using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.PurchaseModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System.Diagnostics;
using System.Text;

namespace SloperMobile.Common.Services
{
    public class PurchasedCheckService : IPurchasedCheckService
	{
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IRepository<GuideBookTable> guideBookRepository;
        readonly IRepository<UserInfoTable> userRepository;
		private readonly IConnectivity connectivity;
		private readonly IExceptionSynchronizationManager exceptionManager;
        private readonly IHttpHelper httpHelper;  

		public PurchasedCheckService(
            IHttpHelper httpHelper,
			IRepository<GuideBookTable> guideBookRepository,
			IRepository<CragExtended> cragRepository,
            IRepository<UserInfoTable> userRepository,
            IExceptionSynchronizationManager exceptionManager,
			IConnectivity connectivity)
		{
            this.httpHelper = httpHelper;
			this.guideBookRepository = guideBookRepository;
            this.userRepository = userRepository;
			this.cragRepository = cragRepository;
			this.connectivity = connectivity;
            this.exceptionManager = exceptionManager;
		}

        public async Task UpdateStateAsync(IEnumerable<CragExtended> crags = null, IEnumerable<GuideBookTable> guidebooks = null)
        {
            if (!connectivity.IsConnected || GuestHelper.IsGuest)
                return;

            StringBuilder debugInfo = new StringBuilder(); //temp hack for XAM-1548

            try
            {
                //var connected = await CrossInAppBilling.Current.ConnectAsync();
                //var timeNow = DateTime.UtcNow;
                //var purchasedItems = await CrossInAppBilling.Current.GetPurchasesAsync(ItemType.Subscription);
                //purchasedItems = purchasedItems.Where(i => i.TransactionDateUtc.AddMinutes(5)/*.AddMonths(1)*/ > timeNow).ToList();
                //foreach (var item in purchasedItems)
                //    Debug.WriteLine(item.ProductId);
                //await CrossInAppBilling.Current.DisconnectAsync();

                //httpClinetHelper.ChangeTokens(string.Format(ApiUrls.Url_AppPurchase_GetUnlock, AppSetting.APP_ID),
                    //Settings.AccessToken);
                var unlockResponse = await httpHelper.GetAsync<UnlockCragResponse>(ApiUrls.Url_AppPurchase_GetUnlock);

                debugInfo.AppendLine($"unlockModel: {JsonConvert.SerializeObject(unlockResponse)}");

                if(unlockResponse.ValidateResponse())
                {
                    var unlockModel = unlockResponse.Result;

                    Settings.AppPurchased = unlockModel.AppPurchased;
                    Settings.FreeCragIds = unlockModel.FreeDownloadedCrags.ToList();
                    if (crags == null)
                        crags = await cragRepository.GetAsync();

                    debugInfo.AppendLine($"Crags Count: {crags.Count()}");

                    var userId = Settings.UserID;
                    var currentUser = await userRepository.GetAsync(userId);
                    if (currentUser == null)
                        return;

                    debugInfo.AppendLine($"Settings.UserId: {userId}");
                    debugInfo.AppendLine($"currentUser: {JsonConvert.SerializeObject(currentUser)}");

                    var isSuperUser = currentUser.NumberOfFreeCrags == -1;

                    foreach (var crag in crags)
                    {
                        crag.Unlocked = isSuperUser
                            || unlockModel.AppPurchased
                            || (unlockModel?.Crags?.Any(c => c == crag.crag_id) ?? false)
                            || (unlockModel?.Guidebooks?.Any(gid => gid == crag.crag_guide_book) ?? false)
                            || (unlockModel?.FreeDownloadedCrags?.Contains(crag.crag_id) ?? false);
                    }
                    await cragRepository.UpdateAllAsync(crags);
                    if (guidebooks == null)
                        guidebooks = await guideBookRepository.GetAsync();

                    debugInfo.AppendLine($"GBs Count: {guidebooks.Count()}");

                    foreach (var guidebook in guidebooks)
                    {
                        guidebook.Unlocked = isSuperUser
                            || unlockModel.AppPurchased
                            || (unlockModel.Guidebooks?.Any(g => g == guidebook.GuideBookId) ?? false);
                    }
                    await guideBookRepository.UpdateAllAsync(guidebooks);

                    debugInfo.AppendLine("All succeed");
                }
            }
            catch (Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = $"{nameof(this.UpdateStateAsync)}",
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = debugInfo.ToString()
                });
            }
        }
    }
}
