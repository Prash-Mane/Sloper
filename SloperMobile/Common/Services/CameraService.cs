using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase.DataTables;
using Xamarin.Forms;

namespace SloperMobile.Common.Services
{
	public class CameraService : ICameraService
	{
		private readonly IUserDialogs userDialogs;
		private readonly IMedia media;
		private readonly IExceptionSynchronizationManager exceptionManager;

		public CameraService(
			IUserDialogs userDialogs,
			IExceptionSynchronizationManager exceptionManager,
			IMedia media)
		{
			this.userDialogs = userDialogs;
			this.media = media;
			this.exceptionManager = exceptionManager;
			media.Initialize();
		}

		public async Task<bool> CheckPermissions()
		{
			//var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
			var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

			if (storageStatus != PermissionStatus.Granted)//cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
			{
                if (Device.RuntimePlatform == Device.Android)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                    storageStatus = results[Permission.Storage];
                    //cameraStatus = results[Permission.Camera];
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                        storageStatus = results[Permission.Storage];
                    //cameraStatus = results[Permission.Camera];	
                    });
                }
            }

			return storageStatus != PermissionStatus.Granted;//cameraStatus != PermissionStatus.Granted && storageStatus != PermissionStatus.Granted;
		}

		public async Task<MediaFile> PickFileAsync()
		{
			var denied = await CheckPermissions();
			if (denied)
			{
				await userDialogs.AlertAsync("Permissions Denied. Please modify app permisions in settings.", "Unable to pick a file.", "OK");
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.PickFileAsync),
					Page = this.GetType().Name,
					Exception = "Permissions Denied. Please modify app permisions in settings.",
				});

				return null;
			}

			if (!media.IsPickPhotoSupported)
			{
				await userDialogs.AlertAsync("No Gallery", "Picking a photo is not supported.", "OK");
				return null;
			}

			var file = await media.PickPhotoAsync();
			return file == null ? null : file;
		}

        public async Task<MediaFile> TakePhotoAsync(string folderName, string pageName)
        {
			var denied = await CheckPermissions();
			if (denied)
			{
				await userDialogs.AlertAsync("Permissions Denied. Please modify app permisions in settings.", "Unable to take photos.", "OK");
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.TakePhotoAsync),
					Page = this.GetType().Name,
					Exception = "Permissions Denied. Please modify app permisions in settings.",
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(folderName)
				});

				return null;
			}

			if (!media.IsCameraAvailable || !media.IsTakePhotoSupported)
			{
				await userDialogs.AlertAsync("No Camera", "No camera available!", "OK");
				return null;
			}

            MediaFile file = null;
            try
            {
                if (pageName == "AscentProcess")
                {
                    file = await media.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        SaveToAlbum = true,
                        Name = $"{folderName} {DateTime.Now}",
                        Directory = AppSetting.APP_COMPANY,
                        SaveMetaData = true
                    });
                }
                else
                {
                    file = await media.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Name = $"{folderName} {DateTime.Now}",
                    });
                }

            }
            catch (Exception ex)
            {
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.TakePhotoAsync),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = Newtonsoft.Json.JsonConvert.SerializeObject(folderName)
				});
			}

			return file == null ? null : file;
		}
	}
}
