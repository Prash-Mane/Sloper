using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prism.Navigation;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.GuideBookModels;
using SloperMobile.ViewModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using SloperMobile.Common.Constants;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Services;
using SloperMobile.Common.Helpers;
using Acr.UserDialogs;
using SloperMobile.Common.Interfaces;

namespace SloperMobile
{
    public class PremiumSubscriptionViewModel : BaseViewModel
    {
        private readonly IRepository<GuideBookTable> guideBookRepository;
        private readonly IUserDialogs userDialogs;
        private readonly InAppPurchaseService inAppPurchaseService;

        private AppProductTable appPriceMonthly;
        private AppProductTable appPriceYearly;
        private List<ImageSource> _guideBooksImage;
        private double _monthlyPrice;
        private double _yearlyPrice;
        private double _monthlyPriceForYearly;

        public PremiumSubscriptionViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IRepository<GuideBookTable> guideBookRepository,
            IHttpHelper httpHelper,
            IUserDialogs userDialogs,
            InAppPurchaseService inAppPurchaseService)
            : base(navigationService, exceptionManager, httpHelper)
        {
            this.guideBookRepository = guideBookRepository;
            this.inAppPurchaseService = inAppPurchaseService;
            this.userDialogs = userDialogs;

            IsBackButtonVisible = true;
        }

        public ICommand NavigateToTermsPageCommand => new Command(NavigateToTermsPage);

        public Command<SubscriptionRange> PurchaseCommand => new Command<SubscriptionRange>(Purchase);

        public List<ImageSource> GuideBooksImage
        {
            get { return _guideBooksImage; }
            set => SetProperty(ref _guideBooksImage, value);
        }

        public double MonthlyPrice
        {
            get => _monthlyPrice;
            set => SetProperty(ref _monthlyPrice, value);
        }

        public double YearlyPrice
        {
            get => _yearlyPrice; 
            set
            {
                SetProperty(ref _yearlyPrice, value);
                MonthlyPriceForYearly = value / 12;
            }
        }

        public double MonthlyPriceForYearly
        {
            get => _monthlyPriceForYearly;
            set => SetProperty(ref _monthlyPriceForYearly, value );

        }

        int SloperId => (int)(appPriceMonthly?.SloperId ?? appPriceYearly?.SloperId);

        private async Task LoadGbImages()
        {
            var guideBooks = await guideBookRepository.GetAsync();
            var images = guideBooks.Select(g => g.GuidebookPortraitCoverImage.GetImageSource());
            GuideBooksImage = images.ToList();
        }

        private async Task LoadAppPrices() {
            OperationResult<IEnumerable<AppProductTable>> listProductsResponse = null;
            try
            {
                listProductsResponse = await httpHelper.PostAsync<IEnumerable<AppProductTable>>(ApiUrls.Url_AppPurchase_GetCurrentAppProductsId, new
                {
                    app_product_type_id = 1,
                    sloperId = -1
                });
            }catch(Exception e) {
                Debug.WriteLine(e);
            }
            if (listProductsResponse != null && listProductsResponse.ValidateResponse()) {
                appPriceMonthly = listProductsResponse.Result.FirstOrDefault(p => p.SubscriptionRange == SubscriptionRange.Monthly);
                appPriceYearly = listProductsResponse.Result.FirstOrDefault(p => p.SubscriptionRange == SubscriptionRange.Yearly);
                YearlyPrice = appPriceYearly?.AppProductPrice??0;
                MonthlyPrice = appPriceMonthly?.AppProductPrice??0;
            }
        }

        private async void NavigateToTermsPage()
        {
            await navigationService.NavigateAsync<TermsViewModel>();
        }

        private async void Purchase(SubscriptionRange range)
        {
            if (GuestHelper.CheckGuest())
                return;

            AppProductTable appProduct = null;
            switch (range)
            {
                case SubscriptionRange.Monthly:
                    appProduct = appPriceMonthly;
                    break;
                case SubscriptionRange.Yearly:
                    appProduct = appPriceYearly;
                    break;
            }

            var result = await inAppPurchaseService.PurchaseAppAsync(appProduct.ServiceProductId, appProduct.AppProductId);
            if (!result)
            {
                return;
            }

            try
            {
                GeneralHelper.AppOrGuidebookPurchased?.Invoke(this, appProduct);
                var navigationParameters = new NavigationParameters();
                navigationParameters.Add(NavigationParametersConstants.IsUnlockedParameter, result);
                await navigationService.GoBackAsync(navigationParameters);
            }
            catch (System.Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.Purchase),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"sloperId = {SloperId}, productId = {appProduct.ServiceProductId}, productTypeId = {(int)ProductTypes.App}"
                });
            }
        }


        public async override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);
            userDialogs.ShowLoading();
            try
            {
                await LoadGbImages();
                await LoadAppPrices();
            }
            catch (System.Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"GuideBookImages = {GuideBooksImage}, appPriceMonthly = {appPriceMonthly}, appPriceYearly = {appPriceMonthly}"
                });
            }
            finally
            {
                userDialogs.HideLoading();
            }
        }

    }
}
