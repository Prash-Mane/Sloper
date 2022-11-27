using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using SloperMobile.Common.Constants;
using Plugin.Connectivity;
using SloperMobile.Views.FlyoutPages;
using SloperMobile.Views;
using Xamarin.Forms;
using System.Linq;
using SloperMobile.ViewModel;
using SloperMobile.Common.Extentions;
using System.Text;
using SloperMobile.Model.ResponseModels;
using SloperMobile.Common.Interfaces;
using SloperMobile.Views.UserPages;
using SloperMobile.ViewModel.UserViewModels;
using Prism.Navigation;
using SloperMobile.DataBase.DataTables;
using System.Net;
using Rg.Plugins.Popup.Services;
using SloperMobile.UserControls.PopupControls;
using System.Threading;
using System.IO;
using Polly;

namespace SloperMobile.Common.Helpers
{
    public class HttpHelper : IHttpHelper
    {
        const string mediaType = "application/json";

        static bool errorTaskInProgress;
        static string defaultBaseUrl => $"{AppSetting.Base_Url}SloperPlatform/API/{AppSetting.API_VERSION}/";
        static Task<bool> extendTask;
        static HttpClient client;


        static HttpHelper() 
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(2);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }


        public async Task<OperationResult<T>> PostAsync<T>(string requestUri, object model, string baseUrl = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                NavigateToErrorPage();
                return OperationResult<T>.CreateFailure(AppConstant.NETWORK_FAILURE);
            }

            if (baseUrl == null)
                baseUrl = defaultBaseUrl;
                
            var jsonModel = model is string ? (string)model : JsonConvert.SerializeObject(model);
            var fullUrl = $"{baseUrl}{requestUri}";

            if (!string.IsNullOrEmpty(Settings.AccessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);

            HttpContent jsonContent = new StringContent(jsonModel, Encoding.UTF8, mediaType);
            HttpResponseMessage httpResponse = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = httpResponse = await Policy
                    .Handle<HttpRequestException>()
                    .Or<WebException>()
                    .OrInner<HttpRequestException>()
                    .OrInner<WebException>()
                    .WaitAndRetryAsync(5, (i) => TimeSpan.FromSeconds(i * 3))
                    .ExecuteAsync(async () => await client.PostAsync(fullUrl, jsonContent, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && requestUri != ApiUrls.Url_JwtAuth_extendtoken && requestUri != ApiUrls.Url_JwtAuth_login)
                {
                    if (GuestHelper.CheckGuest())
                        return OperationResult<T>.CreateFailure("Unauthorized");

                    Debug.WriteLine($"Start extend token Post: {requestUri}");
                    bool success = false;
                    if (extendTask == null)
                        extendTask = ExtendTokenAsync();
                    if (!extendTask.IsCompleted)
                        success = await extendTask;
                    else
                        success = extendTask.Result;
                    if (success)
                        return await PostAsync<T>(requestUri, model, baseUrl, cancellationToken);
                }

                return await ParseResponseAsync<T>(httpResponse, "POST", fullUrl, jsonModel);
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<T>(ex, "GET", fullUrl);
            }
            finally {
                httpResponse?.Dispose();
            }
        }

        public async Task<OperationResult<T>> GetAsync<T>(string requestUri, string baseUrl = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                NavigateToErrorPage();
                return OperationResult<T>.CreateFailure(AppConstant.NETWORK_FAILURE);
            }

            if (baseUrl == null)
                baseUrl = defaultBaseUrl;

            var fullUrl = $"{baseUrl}{requestUri}";

            if (!string.IsNullOrEmpty(Settings.AccessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);

            HttpResponseMessage httpResponse = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = httpResponse = await Policy
                    .Handle<HttpRequestException>()
                    .Or<WebException>()
                    .OrInner<HttpRequestException>()
                    .OrInner<WebException>()
                    .WaitAndRetryAsync(5, (i) => TimeSpan.FromSeconds(i*3))
                    .ExecuteAsync(async () => await client.GetAsync(fullUrl, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (GuestHelper.CheckGuest())
                        return OperationResult<T>.CreateFailure("Unauthorized");

                    Debug.WriteLine($"Start extend token Get: {requestUri}");
                    bool success = false;
                    if (extendTask == null)
                        extendTask = ExtendTokenAsync();
                    if (!extendTask.IsCompleted)
                        success = await extendTask;
                    else
                        success = extendTask.Result;
                    if (success)
                        return await GetAsync<T>(requestUri, baseUrl, cancellationToken);
                }
                return await ParseResponseAsync<T>(httpResponse, "GET", fullUrl);
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<T>(ex, "GET", fullUrl);
            }
            finally { 
                httpResponse?.Dispose(); 
            }
        }

        public async Task<OperationResult<T>> GetWithProgressAsync<T>(string requestUri, IProgress<double> progress, string baseUrl = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                NavigateToErrorPage();
                return OperationResult<T>.CreateFailure(AppConstant.NETWORK_FAILURE);
            }

            if (baseUrl == null)
                baseUrl = defaultBaseUrl;

            var fullUrl = $"{baseUrl}{requestUri}";

            if (!string.IsNullOrEmpty(Settings.AccessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);

            HttpResponseMessage httpResponse = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                httpResponse = await client.GetAsync(fullUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                var contentLength = httpResponse.Content.Headers.ContentLength;
                cancellationToken.ThrowIfCancellationRequested();
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (GuestHelper.CheckGuest())
                        return OperationResult<T>.CreateFailure("Unauthorized");

                    Debug.WriteLine($"Start extend token Get: {requestUri}");
                    bool success = false;
                    if (extendTask == null)
                        extendTask = ExtendTokenAsync();
                    if (!extendTask.IsCompleted)
                        success = await extendTask;
                    else
                        success = extendTask.Result;
                    if (success)
                        return await GetWithProgressAsync<T>(requestUri, progress, baseUrl, cancellationToken);
                }
            

                if (httpResponse.IsSuccessStatusCode)
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var download = await httpResponse.Content.ReadAsStreamAsync())
                        {
                            if (progress == null || !contentLength.HasValue)
                            {
                                await download.CopyToAsync(ms);
                            }
                            else
                            {
                                // Convert absolute progress (bytes downloaded) into relative progress (0-1)
                                var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                                var buffer = new byte[81920];
                                long totalBytesRead = 0;
                                int bytesRead;
                                while ((bytesRead = await download.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                                {
                                    await ms.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                                    totalBytesRead += bytesRead;
                                    //Debug.WriteLine((float)totalBytesRead / contentLength.Value);
                                    progress?.Report((float)totalBytesRead / contentLength.Value);
                                }

                                progress.Report(1);
                            }
                        }

                        var serializer = new JsonSerializer();
                        ms.Position = 0;
                        var sReader = new StreamReader(ms, new UTF8Encoding());
                        using (var jsonReader = new JsonTextReader(sReader))
                        {
                            var res = serializer.Deserialize<T>(jsonReader);
                            return OperationResult<T>.CreateSuccessResult(res);
                        }
                }
            }
            else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                return OperationResult<T>.CreateFailure("Unauthorized");
            else
            {
                Debug.WriteLine($"GET\t{fullUrl}\tError: {httpResponse.StatusCode}");
                if (requestUri != ApiUrls.Url_M_ExceptionLogger)
                {
                    var content = await httpResponse.Content?.ReadAsStringAsync();
                    await App.ExceptionSyncronizationManager.LogException(new ExceptionTable
                    {
                        Method = nameof(GetWithProgressAsync),
                        Page = nameof(HttpHelper),
                        StackTrace = $"StatusCode: {httpResponse.StatusCode}",
                        Exception = "Response not successful",
                        Data = $"Endpoint: {fullUrl}\nResponse: {content}"
                    });
                }
                return OperationResult<T>.CreateFailure("Endpoint error");
            }

            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync<T>(ex, "GET", fullUrl);
            }
            finally
            {
                httpResponse?.Dispose();
            }
        }

        async Task<OperationResult<T>> ParseResponseAsync<T>(HttpResponseMessage httpResponse, string requestType, string url, string jsonModel = null)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonResult = await httpResponse.Content.ReadAsStringAsync();
                return await Deseralize<T>(jsonResult, requestType, url, jsonModel);
            }
            else if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                return OperationResult<T>.CreateFailure("Unauthorized");
            else
            {
                Debug.WriteLine($"{requestType}\t {url}\tError: {httpResponse.StatusCode}\tContent: {jsonModel}");
                if (!url.Contains(ApiUrls.Url_M_ExceptionLogger))
                {
                    var content = await httpResponse.Content?.ReadAsStringAsync();
                    await App.ExceptionSyncronizationManager.LogException(new ExceptionTable
                    {
                        Method = $"{requestType}",
                        Page = nameof(HttpHelper),
                        StackTrace = $"StatusCode: {httpResponse.StatusCode}",
                        Exception = "Response not successful",
                        Data = $"JsonContent:{jsonModel}\nEndpoint: {url} \nResponse: {content}"
                    });
                }
                return OperationResult<T>.CreateFailure("Endpoint error");
            }
        }

        async Task<OperationResult<T>> Deseralize<T>(string jsonResult, string requestType, string url, string jsonModel = null)
        {
            T result = default(T);
            if (jsonResult is T)//string, no need to deserialize 
            {
                result = (T)Convert.ChangeType(jsonResult, typeof(string));
                return OperationResult<T>.CreateSuccessResult(result);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(jsonResult))
                    throw new Exception("Empty response");
                result = JsonConvert.DeserializeObject<T>(jsonResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(requestType);
                Debug.WriteLine(url);
                Debug.WriteLine($"Error deserializing type {typeof(T)}: {jsonResult}");
                Debug.WriteLine(ex.Message);
                if (!url.Contains(ApiUrls.Url_M_ExceptionLogger))
                {
                    await App.ExceptionSyncronizationManager.LogException(new ExceptionTable
                    {
                        Method = $"{requestType}",
                        Page = nameof(HttpHelper),
                        StackTrace = ex.StackTrace,
                        Exception = ex.Message,
                        Data = $"Error deserializing type {typeof(T)}: {jsonResult}\nEndpoint: {url}\nPostContent:{jsonModel}"
                    });
                }
                return OperationResult<T>.CreateFailure("Error reading response value", ex);
            }
            return OperationResult<T>.CreateSuccessResult(result);
        }

        async Task<OperationResult<T>> HandleExceptionAsync<T>(Exception ex, string requestType = null, string url = null)
        {
            if (ex is OperationCanceledException || ex is TaskCanceledException)
                return OperationResult<T>.CreateFailure(AppConstant.CANCELLED);

            if (ex is HttpRequestException || ex is WebException)
            {
                NavigateToErrorPage();
                return OperationResult<T>.CreateFailure(AppConstant.NETWORK_FAILURE);
            }

            if (!url.Contains(ApiUrls.Url_M_ExceptionLogger))
            {
                await App.ExceptionSyncronizationManager.LogException(new ExceptionTable
                {
                    Method = $"{requestType}, response type {typeof(T)}",
                    Page = nameof(HttpHelper),
                    StackTrace = ex.StackTrace,
                    Exception = ex.Message,
                    Data = $"Request Exception\nEndpoint: {url}"
                });
            }

            //Debug.WriteLine($"{client.BaseAddress}\tError: {httpResponse?.StatusCode}\tContent: {jsonModel}");
            return OperationResult<T>.CreateFailure("Endpoint error", ex);
        }

        async Task NavigateToErrorPage()
        {
            if (App.AreSectorPages || errorTaskInProgress)
                return;

            errorTaskInProgress = true;

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (PopupNavigation.PopupStack.Any())
                    await PopupNavigation.PopAllAsync(false);

                if (Device.RuntimePlatform == Device.iOS) //ios is not able to display page if there's alert visible
                {
                    var helper = DependencyService.Get<IAlertsHelper>();
                    await helper.CloseAll();
                }
                await PopupNavigation.PushAsync(new NetworkErrorPopup(), true);
            });

            await Task.Delay(3000);
            errorTaskInProgress = false;
        }

        async Task<bool> ExtendTokenAsync()
        {
            Debug.WriteLine($"Start Extend Task: Access token before: {Settings.AccessToken}");

            var extendModel = new { rtoken = Settings.RenewalToken };
            var result = await PostAsync<LoginResponseModel>(ApiUrls.Url_JwtAuth_extendtoken, extendModel, AppSetting.Base_Url);

            //Reset extend task after 2 min so that is able to be rerun if needed
            Task.Run(async () => {
                await Task.Delay(TimeSpan.FromMinutes(2));
                extendTask = null;
            });
           
            if (result.ValidateResponse(false))
            {
                Settings.AccessToken = result.Result.accessToken;
                Settings.RenewalToken = result.Result.renewalToken;
                Debug.WriteLine($"Start Extend Task: Access token after: {Settings.AccessToken}");
                return true;
            }

            await App.ExceptionSyncronizationManager.LogException(new ExceptionTable
            {
                Method = nameof(ExtendTokenAsync),
                Page = nameof(HttpHelper),
                StackTrace = "Failed to extend token",
                Exception = "Failed to extend token",
                Data = $"UserId:{Settings.UserID}\nAccessToken:{Settings.AccessToken}"
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                GeneralHelper.LogOut();
            });

            return false;
        }

        public async Task CheckTokenAsync()
        {
            if (GuestHelper.IsGuest)
                return;

            if (string.IsNullOrEmpty(Settings.AccessToken) || !CrossConnectivity.Current.IsConnected)
                return;

            if (extendTask == null)
                extendTask = ExtendTokenAsync();
            if (!extendTask.IsCompleted)
                await extendTask;
        }
    }
}
