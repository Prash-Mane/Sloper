using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using SloperMobile.ViewModel.MasterDetailViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;
using SloperMobile.ViewModel.ProfileViewModels;
using Xamarin.Forms.Internals;
using System.Text;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using Acr.UserDialogs;
using SloperMobile.Common.Helpers;

namespace SloperMobile.Common.Extentions
{
	public static class NavigationExtention
	{
		public static void RegisterTypeForViewModelNavigation<TView, TViewModel>(this IContainerRegistry container) where TView : Page where TViewModel : class
		{
			var viewType = typeof(TView);
			ViewModelLocationProvider.Register(viewType.ToString(), typeof(TViewModel));
			container.RegisterForNavigation<TView>(typeof(TViewModel).FullName);
		}

        public static async Task NavigateFromMenuAsync(
			this INavigationService navigationService,
            Type viewModel,
			NavigationParameters parameters = null,
			bool? useModalNavigation = null,
			bool animated = true)
		{
            //Should work without white bg animation on Android
            //sometimes triggers double call for OnNavigatedTo. Not safe to use
            //try //will throw on app init, but that's ok
            //{
            //    var currentUrl = navigationService.GetNavigationUriPath();
            //    var vms = currentUrl.Remove(0, 1).Split('/');
            //    var index = vms.IndexOf(typeof(MasterNavigationViewModel).ToString());
            //    if (index != -1) {
            //        var urlBuilder = new StringBuilder();
            //        var backCount = vms.Length - index - 1;
            //        for (int i = 0; i < backCount; i++)
            //            urlBuilder.Append("../");
            //        urlBuilder.Append(viewModel.ToString());
            //        return navigationService.NavigateAsync(urlBuilder.ToString(), parameters, useModalNavigation, animated);
            //    }
            //}
            //catch { }
            //Debug.WriteLine(currentUrl);
            var url = CreateNavigationUrl(typeof(MainMasterDetailViewModel), typeof(MasterNavigationViewModel), viewModel);

            await navigationService.NavigateAsync(url, parameters, useModalNavigation, animated);
		}

        public static async Task NavigateFromMenuAsync<T>(
            this INavigationService navigationService,
            NavigationParameters parameters = null,
            bool? useModalNavigation = null,
            bool animated = true)
        {
            //var currentUrl = navigationService.GetNavigationUriPath();
            //Debug.WriteLine(currentUrl);
            await navigationService.NavigateFromMenuAsync(typeof(T), parameters, useModalNavigation, animated);
        }

        public static async Task NavigateAsync<TViewModel>(
			this INavigationService navigationService,
			NavigationParameters parameters = null,
			bool? useModalNavigation = null,
			bool animated = true) where TViewModel : BindableBase
		{
            //var currentUrl = navigationService.GetNavigationUriPath();
            //Debug.WriteLine(currentUrl);
			await navigationService.NavigateAsync(typeof(TViewModel).FullName, parameters, useModalNavigation, animated);
		}

		public static async Task ResetNavigation<TMenuViewModel, TNavigationViewModel1, TViewModel>(
			this INavigationService navigationService,
			NavigationParameters parameters = null,
			bool? useModalNavigation = null,
			bool animated = true,
			params string[] deepNavigation) where TViewModel : BindableBase
		{
            await navigationService.NavigateFromMenuAsync(typeof(TViewModel), parameters, useModalNavigation, animated);

			//var resetNavigationString = $"SloperMobile:///{typeof(TMenuViewModel).FullName}/{typeof(TNavigationViewModel1).FullName}/{typeof(TViewModel).FullName}";
			//if (deepNavigation != null && deepNavigation.Length > 0)
			//{
			//	var deepnavigationString = string.Empty;
			//	foreach (var page in deepNavigation)
			//	{
			//		deepnavigationString = string.Concat(deepnavigationString, $"/{page}");
			//	}
			//	resetNavigationString = string.Concat(resetNavigationString, deepnavigationString);
			//}

			//return navigationService.NavigateAsync(
			//	new Uri(
			//		resetNavigationString,
			//		UriKind.Absolute),
			//	parameters,
			//	useModalNavigation,
			//	animated);
		}

		public static async Task ResetNavigation<TViewModel>(
		 this INavigationService navigationService,
		 NavigationParameters parameters = null,
		 bool? useModalNavigation = null,
		 bool animated = true)
		{
			var resetNavigationString = $"SloperMobile:///{typeof(TViewModel).FullName}";
			await navigationService.NavigateAsync(
			 new Uri(
			  resetNavigationString,
			  UriKind.Absolute),
			 parameters,
			 useModalNavigation,
			 animated);
		}

		public static async Task StartFromCurrentPage<TMenuViewModel, TNavigationViewModel, TViewModel>(
				this INavigationService navigationService,
				string navigationPages,
				NavigationParameters parameters = null,
				bool? useModalNavigation = null,
				bool animated = true) where TViewModel : BindableBase
		{
			string navigationString;
			if (string.IsNullOrEmpty(navigationPages))
			{
                navigationString = $"SloperMobile:///{typeof(TMenuViewModel).FullName}/{typeof(TNavigationViewModel).FullName}/{typeof(TViewModel).FullName}";
			}
			else
			{
                navigationString = $"SloperMobile:///{typeof(TMenuViewModel).FullName}/{typeof(TNavigationViewModel).FullName}/{typeof(TViewModel).FullName}/{navigationPages}";
			}

			await navigationService.NavigateAsync(navigationString, parameters, useModalNavigation, animated);
		}

        public static async Task ChangeTopPage(
            this INavigationService navigationService,
            Type viewModel,
            NavigationParameters parameters = null,
            bool? useModalNavigation = null,
            bool animated = true) 
        {
            await navigationService.NavigateAsync($"../{viewModel}", parameters, useModalNavigation, animated);
        }

        public static async Task ChangeTopPage<T>(
            this INavigationService navigationService,
            NavigationParameters parameters = null,
            bool? useModalNavigation = null,
            bool animated = true) where T : BindableBase
        {
            await navigationService.ChangeTopPage(typeof(T), parameters, useModalNavigation, animated);
        }

        public static string CreateNavigationUrl(params Type[] types)
        {
            var url = string.Join("/", types.Select(t => t.ToString()));
            return $"/{url}";
        }

        //public static Task OpenTabbedPage<T>(
        //    this INavigationService navigationService,
        //    NavigationParameters parameters = null,
        //    bool? useModalNavigation = null,
        //    bool animated = true) where T : BindableBase
        //{
        //    if (navigationService.IsTabPageActive())
        //        return navigationService.ChangeTopPage<T>(parameters, useModalNavigation, animated);
        //    return navigationService.NavigateAsync<T>(parameters, useModalNavigation, animated);
        //}

        //static bool IsTabPageActive(this INavigationService navigationService) 
        //{
        //    var tabPages = new[] { 
        //        typeof(ProfileRankingViewModel), 
        //        typeof(ProfilePointsViewModel), 
        //        typeof(ProfileSendsViewModel), 
        //        typeof(ProfileCalendarViewModel), 
        //        typeof(ProfileTickListViewModel) 
        //    };

        //    var currentUrl = navigationService.GetNavigationUriPath();
        //    return tabPages.Any(vm => currentUrl.EndsWith(vm.ToString()));
        //}

        public static string GetPreLastVM(this INavigationService navigationService)
        {
            var currentUrl = navigationService.GetNavigationUriPath();
            var vms = currentUrl.Remove(0, 1).Split('/');
            if (vms.Length < 2)
                return "RootViewModel"; //hack

            var preLastVM = vms[vms.Length - 2];
            if (string.IsNullOrEmpty(preLastVM))
                return null;

            var vmNoNamespace = preLastVM.Split('.').Last();

            return vmNoNamespace.Split('?').First();
        }

        public static string GetLastVM(this INavigationService navigationService)
        {
            var currentUrl = navigationService.GetNavigationUriPath();
            var vms = currentUrl.Remove(0, 1).Split('/');
            if (!vms.Any())
                return null;

            var lastVM = vms.LastOrDefault();
            if (string.IsNullOrEmpty(lastVM))
                return null;

            var vmNoNamespace = lastVM.Split('.').Last();

            return vmNoNamespace.Split('?').First();
        }

        public static async Task GoSubscribe(this INavigationService navigationService, IHttpHelper httpHelper, GuideBookTable gb, IExceptionSynchronizationManager exceptionManager, int cragId = 0)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                var navigationParameters = new NavigationParameters();

                if (gb.enable_purchase_guidebook)
                    navigationParameters.Add(NavigationParametersConstants.GuideBookIdParameter, gb.GuideBookId);
                if (gb.enable_purchase_crag && cragId != 0)
                    navigationParameters.Add(NavigationParametersConstants.SloperIdParameter, cragId);
                if (gb.enable_purchase_guidebook || (gb.enable_purchase_crag && cragId != 0))
                {
                    await navigationService.NavigateAsync<ViewModel.SubscriptionViewModels.UnlockViewModel>(navigationParameters);
                    return;
                }

                if (GuestHelper.CheckGuest())
                    return;

                var appPriceMonthlyResponse = await httpHelper.PostAsync<AppProductTable>(ApiUrls.Url_AppPurchase_GetCurrentAppProductIds, new
                {
                    app_product_type_id = 1,
                    sloperId = -1,
                    SubscriptionRange = (int)SubscriptionRange.Monthly
                });

                if (!appPriceMonthlyResponse.ValidateResponse())
                    return;

                var appPriceYearlyResponse = await httpHelper.PostAsync<AppProductTable>(ApiUrls.Url_AppPurchase_GetCurrentAppProductIds, new
                {
                    app_product_type_id = 1,
                    sloperId = -1,
                    SubscriptionRange = (int)SubscriptionRange.Yearly
                });

                if (!appPriceYearlyResponse.ValidateResponse())
                    return;

                //navigationParameters.Add(NavigationParametersConstants.PurchaseSloperTypeParameter, 1);
                navigationParameters.Add(NavigationParametersConstants.PageHeaderTextParameter, "Sloper App");
                navigationParameters.Add(NavigationParametersConstants.SloperMontlyPriceParameter, appPriceMonthlyResponse.Result);
                navigationParameters.Add(NavigationParametersConstants.SloperYearlyPriceParameter, appPriceMonthlyResponse.Result);

                await navigationService.NavigateAsync<ViewModel.SubscriptionViewModels.SubscriptionViewModel>(navigationParameters);
            }
            catch (Exception ex)
            {
                exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(GoSubscribe),
                    Page = nameof(NavigationExtention),
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                });
            }
            finally {
                UserDialogs.Instance.HideLoading();
            }
        }
	}
}
