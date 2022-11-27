using Acr.UserDialogs;
using Plugin.Connectivity.Abstractions;
using Prism.Navigation;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.Common.Services;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using SloperMobile.UserControls.PopupControls;
using SloperMobile.ViewModel.MasterDetailViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.SubscriptionViewModels
{
    public class SubscriptionViewModel : BaseViewModel
	{
        private readonly InAppPurchaseService inAppPurchaseService;
		private readonly IDownloadCragService downloadCragService;
		private readonly IUserDialogs userDialogs;
		private readonly IRepository<RouteTable> routeRepository;
		private readonly IRepository<CragExtended> cragRepository;
		private readonly IRepository<CragImageTable> cragImageRepository;
		private readonly IRepository<SectorTable> sectorRepository;
		private readonly IRepository<GuideBookTable> guideBookRepository;
		private readonly IRepository<AppProductTable> appProductRepository;
		private readonly IConnectivity connectivity;

		//private int sloperId;
		private AppProductTable productMonthly;
        AppProductTable productYearly;
		private SubscriptionModel subscriptionModel;
		private ImageSource backgroundImage;
        private CragExtended cragData;
        bool isLoaded;

        private GuideBookTable guideBookData;

		public SubscriptionViewModel(
			INavigationService navigationService,
			IExceptionSynchronizationManager exceptionManager,
			IUserDialogs userDialogs,
			IRepository<RouteTable> routeRepository,
			IRepository<CragExtended> cragRepository,
			IRepository<SectorTable> sectorRepository,
			IRepository<GuideBookTable> guideBookRepository,
			IRepository<AppProductTable> appProductRepository,
			IRepository<CragImageTable> cragImageRepository,
            IHttpHelper httpHelper,
			InAppPurchaseService inAppPurchaseService,
			IDownloadCragService downloadCragService,
			IConnectivity connectivity)
			: base(navigationService, exceptionManager, httpHelper)
		{
			this.userDialogs = userDialogs;
			this.routeRepository = routeRepository;
			this.cragRepository = cragRepository;
			this.sectorRepository = sectorRepository;
			this.guideBookRepository = guideBookRepository;
			this.appProductRepository = appProductRepository;
			this.cragImageRepository = cragImageRepository;
			this.inAppPurchaseService = inAppPurchaseService;
			this.downloadCragService = downloadCragService;
			this.connectivity = connectivity;
			IsBackButtonVisible = true;
            HasFade = true;
			subscriptionModel = new SubscriptionModel();
		}

		public SubscriptionViewModel(
			INavigationService navigationService)
			: base(navigationService)
		{
		}
       
		public ICommand PurchaseItemCommand => new Command(PurchaseItem);

		public ICommand NavigateToTermsPageCommand => new Command(NavigateToTermsPage);

		public ICommand ShowDescriptionPopupCommand => new Command(ShowDescriptionPopup);

		public SubscriptionModel SubscriptionModel
		{
			get => subscriptionModel;
			set => SetProperty(ref subscriptionModel, value);
		}

		public ImageSource BackgroundImage
		{
			get => backgroundImage;
			set => SetProperty(ref backgroundImage, value);
		}

        ProductTypes CurrentProductType => (ProductTypes)(productMonthly?.AppProductTypeId ?? productYearly?.AppProductTypeId);

        int SloperId => (int)(productMonthly?.SloperId ?? productYearly?.SloperId);

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public async override void OnNavigatedTo(NavigationParameters parameters)
		{
            base.OnNavigatedTo(parameters);
            if (isLoaded)
                return;

            isLoaded = true;
			try
			{
				if (!connectivity.IsConnected)
				{
					userDialogs.HideLoading();
					await userDialogs.AlertAsync("This page requires internet connection!");
					await navigationService.GoBackAsync();
					return;
				}

                //parameters.TryGetValue(NavigationParametersConstants.Purchasesloper.AppProductTypeIdParameter, out sloper.AppProductTypeId);
                //parameters.TryGetValue(NavigationParametersConstants.SloperIdParameter, out sloperId); //todo: not needed?
                parameters.TryGetValue(NavigationParametersConstants.PageHeaderTextParameter, out string title);
                parameters.TryGetValue(NavigationParametersConstants.SloperMontlyPriceParameter, out productMonthly);
                parameters.TryGetValue(NavigationParametersConstants.SloperYearlyPriceParameter, out productYearly);

                PageHeaderText = title;
				PageSubHeaderText = "Subscription Details";

                if (CurrentProductType == ProductTypes.Crag)
                {
                    cragData = (CragExtended)parameters[NavigationParametersConstants.CragDataParameter];
                    guideBookData = await guideBookRepository.GetAsync((int)cragData.crag_guide_book);
                    var image = await cragImageRepository.GetAsync(SloperId);
                    var bytesString = string.IsNullOrEmpty(image?.crag_image) ? string.Empty : image?.crag_image?.Split(',')[1];
                    if (!string.IsNullOrEmpty(bytesString))
                    {
                        var imageBytes = Convert.FromBase64String(bytesString);
                        BackgroundImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                    else
                    {
                        BackgroundImage = AppSetting.APP_TYPE == AppConstant.IndoorAppType
                                            ? ImageSource.FromFile(AppConstant.DefaultSloperIndoorPortraiteImageString)
                                            : ImageSource.FromFile(AppConstant.DefaultSloperOutdoorPortraitImageString);
                    }
                }
				else if (CurrentProductType == ProductTypes.Guidebook)
				{
					guideBookData = (GuideBookTable)parameters[NavigationParametersConstants.GuideBookDataParameter];
                    if (!string.IsNullOrEmpty(guideBookData.GuidebookPortraitImage))
                    {
                        BackgroundImage = guideBookData.GuidebookPortraitImage.GetImageSource();
                    }
                    else
                    {
                        BackgroundImage = AppSetting.APP_TYPE == AppConstant.IndoorAppType
                                               ? ImageSource.FromFile(AppConstant.DefaultSloperIndoorPortraiteImageString)
                                               : ImageSource.FromFile(AppConstant.DefaultSloperOutdoorPortraitImageString);
                    }
				}
				else
				{
					BackgroundImage = AppSetting.APP_TYPE == AppConstant.IndoorAppType
											? ImageSource.FromFile(AppConstant.DefaultSloperIndoorPortraiteImageString)
											: ImageSource.FromFile(AppConstant.DefaultSloperOutdoorPortraitImageString);
				}

				var guidebooksCount = (await guideBookRepository.GetAsync(g => g.is_app_store_ready)).Count;
                int cragsCount = 0;

               if (CurrentProductType == ProductTypes.App)
					cragsCount = (await cragRepository.GetAsync(crag => crag.is_enabled &&
													 crag.is_app_store_ready && crag.crag_longitude != null && crag.crag_latitude != null && crag.crag_latitude != 0 && crag.crag_longitude != 0)).Count;
			   else if (CurrentProductType == ProductTypes.Guidebook)
                    cragsCount = (await cragRepository.GetAsync(crag => crag.crag_guide_book == SloperId
                                                     && crag.is_enabled 
                                                     && crag.is_app_store_ready && crag.crag_longitude != null && crag.crag_latitude != null && crag.crag_latitude != 0 && crag.crag_longitude != 0)).Count;
                                                     
                var response = await httpHelper.GetAsync<SectorRouteCount>(ApiUrls.Url_AppPurchase_GetSectorRouteCount((int)CurrentProductType, SloperId));		

                if(response.ValidateResponse())
                {
                    var sectorsCount = response.Result.SectorCount;
                    var routesCount = response.Result.RouteCount;

                    SubscriptionModel = new SubscriptionModel
                    {
                        AreCragsVisible = CurrentProductType != ProductTypes.Crag,
                        AreGuidebooksVisible = CurrentProductType == ProductTypes.App,
                        SubscriptionItemName = CurrentProductType == ProductTypes.App ? "ENTIRE SLOPER APP" : (CurrentProductType == ProductTypes.Guidebook ? $"{guideBookData?.GuideBookName?.ToUpper()} GUIDE" : $"{cragData?.crag_name?.ToUpper()} GUIDE"),
                        SubscriptionItemPrice = $"${productMonthly?.AppProductPrice}/MTH",//todo: handle
                        SubscriptionItemType = CurrentProductType == ProductTypes.App ? string.Empty : (CurrentProductType == ProductTypes.Guidebook ? $"{guideBookData?.GuideBookName?.ToUpper()}" : $"{cragData?.crag_name?.ToUpper()}"),
                        SubscriptionItemTypeDescription = CurrentProductType == ProductTypes.App ? "ENTIRE GUIDEBOOK" : (CurrentProductType == ProductTypes.Guidebook ? "GUIDEBOOK" : "CRAG GUIDE"),
                        SubscriptionItemTypeAuthors = WebUtility.HtmlDecode(guideBookData?.Author ?? string.Empty)?.ToUpper(),
                        GuideBooksCount = guidebooksCount,
                        CragsCount = cragsCount,
                        SectorsCount = sectorsCount,
                        RoutesCount = routesCount,
                        Description = guideBookData?.Description
                    };
                }			
			}
			catch (Exception exception)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatedTo),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data =
						$"sloperId = {parameters[NavigationParametersConstants.SloperIdParameter]}"
				});
			}
			finally
			{
				userDialogs.HideLoading();
			}
		}

        private void PurchaseItem()
        {
            switch (CurrentProductType)
            {
                //TODO: Handle periods
                case ProductTypes.App:
                    UnlockTheApp();
                    break;
                case ProductTypes.Guidebook:
                    UnlockTheGuideBook();
                    break;
                case ProductTypes.Crag:
                    UnlockTheCrag();
                    break;
            }
        }

        private async void UnlockTheApp()
        {
            var result = await inAppPurchaseService.PurchaseAppAsync(productMonthly.ServiceProductId, productMonthly.AppProductId);
            if (!result)
            {
                return;
            }

            try
            {
                Settings.AppPurchased = result;
                GeneralHelper.AppOrGuidebookPurchased?.Invoke(this, productMonthly);
                await navigationService.GoBackAsync();
            }
            catch (System.Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.UnlockTheApp),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"sloperId = {SloperId}, productId = {productMonthly.ServiceProductId}, productTypeId = {(int)ProductTypes.App}"
                });
            }
        }

        private async void UnlockTheGuideBook()
        {
            var result = await inAppPurchaseService.CreatePurchaseAsync(SloperId, (int)ProductTypes.Guidebook, productMonthly.ServiceProductId, productMonthly.AppProductId);
            if (!result)
            {
                return;
            }

            try
            {
                GeneralHelper.AppOrGuidebookPurchased?.Invoke(this, productMonthly);
                var navigationParameters = new NavigationParameters();
                navigationParameters.Add(NavigationParametersConstants.IsUnlockedParameter, result);
                await navigationService.GoBackAsync(navigationParameters);
            }
            catch (System.Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.UnlockTheGuideBook),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"sloperId = {SloperId}, productId = {productMonthly.ServiceProductId}, productTypeId = {(int)ProductTypes.Guidebook}"
                });
            }
        }

        private async void UnlockTheCrag()
        {
            var result = await inAppPurchaseService.CreatePurchaseAsync(SloperId, (int)ProductTypes.Crag, productMonthly.ServiceProductId, productMonthly.AppProductId);
            if (!result)
            {
                return;
            }

            try
            {
                cragData.Unlocked = result;
                await cragRepository.UpdateAsync(cragData);
                GeneralHelper.CragModified?.Invoke(this, cragData);
                var navigationParameters = new NavigationParameters();
                navigationParameters.Add(NavigationParametersConstants.IsUnlockedParameter, result);
                await navigationService.GoBackAsync(navigationParameters);
            }
            catch (System.Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.UnlockTheCrag),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"sloperId = {SloperId}, productId = {productMonthly.ServiceProductId}, productTypeId = {(int)ProductTypes.Crag}"
                });
            }
            finally
            {
                IsRunningTasks = false;
            }
        }

        private async void NavigateToTermsPage()
        {
            await navigationService.NavigateAsync<TermsViewModel>();
        }

        private async void ShowDescriptionPopup()
        {
            if (!string.IsNullOrWhiteSpace(SubscriptionModel.Description))
                await PopupNavigation.PushAsync(new SubscriptionPopup(SubscriptionModel.Description), true);
        }
	}
}