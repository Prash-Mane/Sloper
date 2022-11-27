using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.SubscriptionViewModels
{
    public class UnlockViewModel : BaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private int cragId;
        private long guideBookId;
        private CragExtended cragData;
        private GuideBookTable guideBookData;
        private IList<CarouselPagesModel> carouselPages;
        private int selectedItemPosition;
        private ImageSource backGroundImage;
        private AppProductTable cragPrice;
        private AppProductTable guideBookPrice;
        private AppProductTable appPriceMonthly;
        AppProductTable appPriceYearly;
        private string cragPriceValue;
        private string guideBookPriceValue;
        private string appPriceValue;
        bool isLoaded;

		public UnlockViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IUserDialogs userDialogs,
            IRepository<CragExtended> cragRepository,
            IRepository<GuideBookTable> guideBookRepository,
            IHttpHelper httpHelper,
			IConnectivity connectivity) 
            : base(navigationService, exceptionManager, httpHelper)
        {
			this.userDialogs = userDialogs;
            this.cragRepository = cragRepository;
            this.guideBookRepository = guideBookRepository;
			IsBackButtonVisible = true;
            HasFade = true;

            GeneralHelper.AppOrGuidebookPurchased += OnAppOrGuidebookPurchased;
            GeneralHelper.CragModified += OnCragModified;
        }

		public UnlockViewModel(
            INavigationService navigationService) : base(navigationService)
        {
		}

		public ICommand NavigateToUnlockCommand => new Command<int>(NavigateToUnlock);

		public ICommand NavigateToTermsPageCommand => new Command(NavigateToTermsPage);	

		public IList<CarouselPagesModel> CarouselPages
        {
            get => carouselPages;
		    set => SetProperty(ref carouselPages, value);
	    }

        public bool IsCragVisible => cragPrice?.AppProductPrice > 0;
        public bool IsGBVisible => guideBookPrice?.AppProductPrice > 0;
        public bool IsAppVisible => appPriceMonthly?.AppProductPrice > 0 || appPriceYearly?.AppProductPrice > 0;

        public string CragPrice
        {
            get => cragPriceValue;
            set => SetProperty(ref cragPriceValue, value);
        }

        public string GuideBookPrice
		{
			get => guideBookPriceValue;
			set => SetProperty(ref guideBookPriceValue, value);
		}

		public string AppPrice
		{
			get => appPriceValue;
			set => SetProperty(ref appPriceValue, value);
		}
		
		public int SelectedItemPosition
        {
            get => selectedItemPosition;
			set
            {
                BackGroundImage = CarouselPages[value].BackGroundImage;
                SetProperty(ref selectedItemPosition, value);
            }
        }

        public ImageSource BackGroundImage
        {
            get => backGroundImage;
	        set => SetProperty(ref backGroundImage, value);
        }

		private async void NavigateToUnlock(int sloperType)
		{
            if (GuestHelper.CheckGuest())
                return;

            try
			{
				userDialogs.ShowLoading();
                var navigationParameters = new NavigationParameters
                {
                    {NavigationParametersConstants.SloperIdParameter, sloperType == 3 ? cragId : (int)guideBookId},
                    {NavigationParametersConstants.PageHeaderTextParameter, sloperType == 1 ? "Sloper App" : (sloperType == 3 ? PageHeaderText : PageSubHeaderText)},
                    {NavigationParametersConstants.CragDataParameter, cragData },
                    {NavigationParametersConstants.GuideBookDataParameter, guideBookData },
                };

                if (sloperType == (int)ProductTypes.App)
                {
                    navigationParameters.Add(NavigationParametersConstants.SloperMontlyPriceParameter, appPriceMonthly);
                    navigationParameters.Add(NavigationParametersConstants.SloperYearlyPriceParameter, appPriceYearly);
                }
                else if (sloperType == (int)ProductTypes.Guidebook)
                    navigationParameters.Add(NavigationParametersConstants.SloperMontlyPriceParameter, guideBookPrice);
                else if (sloperType == (int)ProductTypes.Crag)
                    navigationParameters.Add(NavigationParametersConstants.SloperMontlyPriceParameter, cragPrice);

                await navigationService.NavigateAsync<SubscriptionViewModel>(navigationParameters);
			}
			catch (System.Exception exception)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.NavigateToUnlock),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
                    Data = $"sloperType = {sloperType}, guideBookId = {guideBookId}"
                });
			}
			finally
			{
				userDialogs.HideLoading();
			}
		}

		private async void NavigateToTermsPage()
		{
			await navigationService.NavigateAsync<TermsViewModel>();
		}

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
        }

		public async override void OnNavigatingTo(NavigationParameters parameters)
		{
            base.OnNavigatingTo(parameters);
            if (isLoaded)
                return;

            isLoaded = true;

			try
			{
				if (parameters.Count == 1 && parameters.ContainsKey(NavigationParametersConstants.IsUnlockedParameter))
				{																						  
					await navigationService.GoBackAsync(parameters, false);
					return;
				}

                parameters.TryGetValue(NavigationParametersConstants.SloperIdParameter, out cragId);
                parameters.TryGetValue(NavigationParametersConstants.GuideBookIdParameter, out guideBookId);

				BackGroundImage = "sub_next_level_background";

				CarouselPages = new List<CarouselPagesModel> {
					new CarouselPagesModel
					{
						ItemImage = "sub_next_level",
						IsRouteData = false,
						IsAnnotationVisible = false,
						BackGroundImage = "sub_next_level_background"
					},
					new CarouselPagesModel
					{
						ItemImage= "sub_unlimited_access",																				
						IsRouteData = false,
						IsAnnotationVisible = true,
						BackGroundImage = "sub_unlimited_access_background"
					},
					new CarouselPagesModel
					{
						ItemImage = "sub_sector_images",																				
						IsRouteData = false,
						IsAnnotationVisible = true,
						BackGroundImage = "sub_sector_images_background"
					},
					new CarouselPagesModel
					{
						ItemImage = "sub_detailed_maps",																				
						IsRouteData = false,
						IsAnnotationVisible = true,
						BackGroundImage = "sub_detailed_maps_background"
					},
					new CarouselPagesModel
					{
					   ItemImage = "sub_route_descriptions",																			
						IsRouteData = true,
						IsAnnotationVisible = true,
						BackGroundImage = "sub_route_descriptions_background"
					},
				};

                if (cragId != 0)
                {
                    cragData = await cragRepository.FindAsync(crag => crag.crag_id == cragId);
                    guideBookData = await guideBookRepository.FindAsync(guidebook => guidebook.GuideBookId == cragData.crag_guide_book);

                    var cragPriceResponse = await httpHelper.PostAsync<AppProductTable>(ApiUrls.Url_AppPurchase_GetCurrentAppProductIds, new
                    {
                        app_product_type_id = 3,
                        sloperId = cragId
                    });


                    if (!cragPriceResponse.ValidateResponse())
                        return;
                    cragPrice = cragPriceResponse.Result;

                    PageHeaderText = cragData.crag_name;
                    PageSubHeaderText = guideBookData.GuideBookName;
                }


                if (guideBookId != 0)
                {
                    if (guideBookData == null)
                        guideBookData = await guideBookRepository.FindAsync(guidebook => guidebook.GuideBookId == guideBookId);
                    var guideBookPriceResponse = await httpHelper.PostAsync<AppProductTable>(ApiUrls.Url_AppPurchase_GetCurrentAppProductIds, new
                    {
                        app_product_type_id = 2,
                        sloperId = guideBookId
                    });

                    if (!guideBookPriceResponse.ValidateResponse())
                        return;
                    guideBookPrice = guideBookPriceResponse.Result;

                    if (string.IsNullOrEmpty(PageHeaderText))
                    {
                        PageHeaderText = guideBookData.GuideBookName;
                        PageSubHeaderText = "GUIDEBOOK";
                    }
                }

                if (string.IsNullOrEmpty(PageHeaderText))
                    PageHeaderText = "SLOPER APP";

                var appPriceMonthlyResponse = await httpHelper.PostAsync<AppProductTable>(ApiUrls.Url_AppPurchase_GetCurrentAppProductIds, new
                {
                    app_product_type_id = 1,
					sloperId = -1,
                    SubscriptionRange = (byte)SubscriptionRange.Monthly
				});

                if (!appPriceMonthlyResponse.ValidateResponse())
                    return;
                appPriceMonthly = appPriceMonthlyResponse.Result;

                var appPriceYearlyResponse = await httpHelper.PostAsync<AppProductTable>(ApiUrls.Url_AppPurchase_GetCurrentAppProductIds, new
                {
                    app_product_type_id = 1,
                    sloperId = -1,
                    SubscriptionRange = (byte)SubscriptionRange.Yearly
                });

                //todo: remove. Test simulation hack
#if DEBUG
                appPriceYearlyResponse = OperationResult<AppProductTable>.CreateSuccessResult(new AppProductTable { SloperId = -1, AppProductTypeId = 1, AppProductPrice = 24.99, SubscriptionRange = SubscriptionRange.Yearly });
#endif            

                if (!appPriceYearlyResponse.ValidateResponse())
                    return;
                appPriceYearly = appPriceYearlyResponse.Result;


                CragPrice = $"${cragPrice?.AppProductPrice ?? 0}/MTH";
                GuideBookPrice = $"${guideBookPrice?.AppProductPrice ?? 0}/MTH";

                //TODO: Or just show minimum price? It doesn't fit
                AppPrice = $"${(appPriceYearly.AppProductPrice / 12).ToString("0.##")} - {appPriceMonthly?.AppProductPrice}/MTH";

                RaisePropertyChanged(nameof(IsCragVisible));
                RaisePropertyChanged(nameof(IsGBVisible));
                RaisePropertyChanged(nameof(IsAppVisible));

                userDialogs.HideLoading();
			}
			catch (System.Exception exception)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatedTo),
					Page = this.GetType().Name,
					StackTrace = exception.StackTrace,
					Exception = exception.Message,
					Data = $"sloperId = {parameters[NavigationParametersConstants.SloperIdParameter]}, guideBookId = {parameters[NavigationParametersConstants.GuideBookIdParameter]}"
				});
			}
        }

        void OnAppOrGuidebookPurchased(object sender, AppProductTable e)
        {
            navigationService.GoBackAsync(animated: false);
        }

        void OnCragModified(object sender, CragExtended e)
        {
            if (sender is SubscriptionViewModel && cragId == e.crag_id)
                navigationService.GoBackAsync(animated: false);
        }


        public override void Destroy()
        {
            base.Destroy();
            GeneralHelper.AppOrGuidebookPurchased -= OnAppOrGuidebookPurchased;
            GeneralHelper.CragModified -= OnCragModified;
        }
    }
}
