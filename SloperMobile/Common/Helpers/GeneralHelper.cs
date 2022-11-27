using System;
using System.Text.RegularExpressions;
using SloperMobile.Common.Constants;
using SloperMobile.DataBase.DataTables;
using SloperMobile.DataBase;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;
using Acr.UserDialogs;
using SloperMobile.Common.Interfaces;
using Xamarin.Forms;
using SloperMobile.Common.Extentions;
using System.Diagnostics;
using SloperMobile.ViewModel.UserViewModels;
using Prism.Navigation;

namespace SloperMobile.Common.Helpers
{
    public static class GeneralHelper
    {
        
        //static GeneralHelper() 
        //{
        //    //it's not safe without await. We need to control updated state
        //    CragModified += (s, e) => UpdateCragsDownloadedStateAsync();
        //}

        public static bool IsEmailValid(string emailaddress)
        {
            try
            {
                Regex regex = new Regex(AppConstant.emailRegex);
                Match match = regex.Match(emailaddress);
                if (match.Success)
                    return true;
                else
                    return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static Exception GetInnerException(Exception exception)
        {
            if (exception.InnerException == null)
                return exception;
            return GetInnerException(exception.InnerException);
        }

        public static async Task<Position> GetMyPositionAsync()
        {
            if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                UserDialogs.Instance.Toast(new ToastConfig("")
                    {
                        Message = $"Location not available",
                        BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
                        MessageTextColor = System.Drawing.Color.White,
                        Duration = TimeSpan.FromSeconds(3)
                    });
                return DefaultPosition;
            }

            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    var lastKnown = await CrossGeolocator.Current?.GetLastKnownLocationAsync();
                    if (lastKnown != null
                        && lastKnown != default(Position)
                        && (lastKnown.Timestamp - DateTimeOffset.Now) < TimeSpan.FromMinutes(5))
                        return lastKnown;
                }

                return await CrossGeolocator.Current?.GetPositionAsync(TimeSpan.FromMilliseconds(300));
            }
            catch (TaskCanceledException)
            {
                try
                {
                    return await CrossGeolocator.Current?.GetPositionAsync(TimeSpan.FromMilliseconds(3000));
                }
                catch { }
            }
            catch { }
            return DefaultPosition;
        }

        public static async Task LogOut()
        {
            Settings.AccessToken = string.Empty;
            Settings.RenewalToken = string.Empty;
            Settings.DisplayName = string.Empty;
            Cache.accessToken = string.Empty;
            if (Settings.FBLogIn == true)
            {
                Xamarin.Forms.DependencyService.Get<IFacebookManager>().Logout();
                Settings.FBLogIn = false;
                Cache.isModel = false;
            }
            if (Settings.GPLogIn == true)
            {
                Xamarin.Forms.DependencyService.Get<IGoogleSignIn>().Logout();
                Settings.GPLogIn = false;
            }
            Cache.twitterSocialData = string.Empty;

            if (TrailCollector.Instance.ActiveRecord != null)
            {
                try
                {
                    await TrailCollector.Instance.CancelCurrentRecordingAsync();
                }
                catch { }
            }

            var dm = App.ContainerProvider.Resolve(typeof(IDownloadManager)) as IDownloadManager;
            if (dm != null && dm.DownloadingQueue.Any())
                dm.StopAll();

            if (!(Application.Current.MainPage is Views.UserPages.UserLoginPage))
                await App.Navigation.ResetNavigation<ViewModel.UserViewModels.UserLoginViewModel>();
        }

        public static Position DefaultPosition { get; } = new Position(51.0747, -115.36); //Canmore

        public static string GetCurrentDate(string format)
        {
            string currentDate = DateTime.Now.Date.ToString(format);
            return currentDate;
        }

        static bool isCragsDownloaded;
        public static bool IsCragsDownloaded => isCragsDownloaded;
        public static async Task UpdateCragsDownloadedStateAsync()
        {
            isCragsDownloaded = (await new Repository<CragExtended>().GetAsync(c => c.is_downloaded && c.is_enabled && c.is_app_store_ready)).Any();
        }

        public async static Task<bool> HandleDownloadedCrag(int cragId, string routeName,  IRepository<CragExtended> cragRepository, INavigationService navigationService)
        {
            var crag = await cragRepository.GetAsync(cragId);
            if (!crag.is_downloaded)
            {
                if (await UserDialogs.Instance.ConfirmAsync($"download {crag.crag_name}?", $"To view {routeName}", "Yes", "No"))
                {

                    var navParams = new Prism.Navigation.NavigationParameters();
                    navParams.Add(NavigationParametersConstants.SelectedCragIdParameter, crag.crag_id);
                    await navigationService.NavigateAsync<ViewModel.CragDetailsViewModel>(navParams);
                }
                return false;
            }
            return true;
        }

        //in test mode. Maybe we'll want to move it to other class
        #region AppState events
        //unfortunately, we cannot use events, coz callers can be located in different pages/vms
        //Never asign to delegate directly! Use add/remove only (+=, -=). Call ?.Invoke() carefully only on pages designed for this
        public static EventHandler<AppProductTable> AppOrGuidebookPurchased { get; set; }
        public static EventHandler<CragExtended> CragModified { get; set; } //on removed, downloaded, purchased
        #endregion
    }
}
