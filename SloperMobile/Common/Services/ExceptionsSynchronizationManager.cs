using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using Plugin.DeviceInfo;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using XLabs.Platform.Device;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;

namespace SloperMobile.Common.Services
{
	public class ExceptionsSynchronizationManager : IExceptionSynchronizationManager
	{
		private readonly IRepository<ExceptionTable> exceptionRepository;
		private readonly IConnectivity connectivityManager;
        private readonly IHttpHelper httpHelper;
		private bool exceptionDataSet = true;
		private IDevice device;

		public ExceptionsSynchronizationManager(
			IRepository<ExceptionTable> exceptionRepository,
			IConnectivity connectivityManager,
            IHttpHelper httpHelper)
		{
			this.exceptionRepository = exceptionRepository;
			this.connectivityManager = connectivityManager;
			this.connectivityManager.ConnectivityChanged += ConnectionChanged;
            this.httpHelper = httpHelper;
			device = XLabs.Ioc.Resolver.Resolve<IDevice>();

            ResendExceptionsToApi();
		}

		private void ConnectionChanged(object sender, ConnectivityChangedEventArgs e)
		{
            if (e.IsConnected && exceptionDataSet)
			{
				ResendExceptionsToApi();
			}
		}

		public async Task LogException(ExceptionTable exception, bool saveToDb = true)
		{
			exception.Date = DateTime.Now;
			//TODO: Need to register CrossDeviceInfo
			exception.Device = CrossDeviceInfo.Current.Model;
			exception.OS = CrossDeviceInfo.Current.Platform.ToString();
			exception.OSVersion = $"Version = {CrossDeviceInfo.Current.Version}, VersionNumber = {CrossDeviceInfo.Current.VersionNumber}";
			exception.AppBuildVersion = Settings.Version;
			exception.AppName = AppSetting.APP_COMPANY;

            //UserDialogs.Instance.HideLoading();

            //Added code below, but it may be causing a crash so we've removed it.- Steve Feb 18
            ////check to see if the data contains an image, if so, don't store it.
            //if(exception.Data.Contains("data:image")){
            //    exception.Data = "Image Data too big";
            //}

            var reportData = new Dictionary<string, string> {
                { "message", exception.Exception },
                { "trace", exception.StackTrace },
                { "device", exception.Device }
            };
            Crashes.TrackError(new Exception("Handled Exception"), reportData);

            if (connectivityManager != null && connectivityManager.IsConnected)
			{
				await SendExceptionToApi(exception);
			}
            else if (saveToDb)
			{
				exceptionDataSet = true;
				await SaveExceptionToDb(exception);
			}
		}

		private async Task<bool> SaveExceptionToDb(ExceptionTable exception)
		{
			var result = await exceptionRepository.InsertAsync(exception);
			return true;
		}

		private async void ResendExceptionsToApi()
		{
			var models = await exceptionRepository.GetAsync();
            if (models == null)
                return;

			foreach (var model in models)
			{
				await LogException(model, false);
			}

			exceptionDataSet = false;
			await exceptionRepository.DeleteAll();
		}

        private async Task<OperationResult<bool>> SendExceptionToApi(ExceptionTable model) =>
            await httpHelper.PostAsync<bool>(ApiUrls.Url_M_ExceptionLogger, model);
    }
}
