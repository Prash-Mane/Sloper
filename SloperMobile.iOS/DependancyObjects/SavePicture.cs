using System;
using System.IO;
using System.Threading.Tasks;
using AssetsLibrary;
using Foundation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.iOS.DependancyObjects;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(SavePicture))]
namespace SloperMobile.iOS.DependancyObjects
{
	public class SavePicture : ISavePicture
    {
        public static ALAssetsLibrary lib = new ALAssetsLibrary();
        public static ALAssetsGroup current_album = null;
        public static string file_path = String.Empty, filePath= string.Empty;
        public async Task<string> SavePictureToDisk(string filename, byte[] imageData)
        {
            return await WriteFileOnDevice(imageData, filename);
        }

        private async Task<string> WriteFileOnDevice(byte[] imgData, string fileName)
        {
            try
            {
                lib.AddAssetsGroupAlbum(AppSetting.APP_COMPANY, g => Console.WriteLine("Succeeded"), e => Console.WriteLine("Error: " + e));
                NSDictionary dict = new NSDictionary();
                var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var directoryname = Path.Combine(documentsDirectory, AppSetting.APP_COMPANY);
                Directory.CreateDirectory(directoryname);
                string jpgFilename = fileName + System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".ig";

                var imgpath = Path.Combine(documentsDirectory, jpgFilename);                
                var image = new UIImage(NSData.FromArray(imgData));

	            var assetUrl = await lib.WriteImageToSavedPhotosAlbumAsync(image.AsJPEG(), dict);
				file_path = assetUrl.AbsoluteUrl.ToString();
				if (File.Exists(imgpath))
				{
					File.Delete(imgpath);
					if (!File.Exists(imgpath))
					{
						Console.WriteLine("Deleted");
					}
				}

				lib.Enumerate(ALAssetsGroupType.Album, HandleALAssetsLibraryGroupsEnumerationResultsDelegate, (obj) => { });
	            return file_path;
            }
            catch (Exception ex)
            {
            }

	        return filePath;
        }

        static void HandleALAssetsLibraryGroupsEnumerationResultsDelegate(ALAssetsGroup group, ref bool stop)
        {
            if (group == null)
            {
                stop = true;
                return;
            }
            if (group.Name == AppSetting.APP_COMPANY)
            {
                stop = true;
                current_album = group;
                AddImageToAlbum();
            }
        }

        static void AddImageToAlbum()
        {
            if (current_album != null && !String.IsNullOrEmpty(file_path))
            {
                lib.AssetForUrl(new Foundation.NSUrl(file_path), delegate (ALAsset asset)
                {
                    if (asset != null)
                    {
                        current_album.AddAsset(asset);
                    }
                    else
                    {
                        Console.WriteLine("ASSET == NULL.");
                    }
                }, delegate (NSError assetError)
                {
                    Console.WriteLine(assetError.ToString());
                });
            }
        }

		public async Task<Stream> GetImage(string path)
		{
			var memoryStream = new MemoryStream();
			using (var source = File.OpenRead(path))
			{
				await source.CopyToAsync(memoryStream);
			}

			return memoryStream;
		}
	}
}