using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.Droid.DependancyObjects;

[assembly: Xamarin.Forms.Dependency(typeof(SavePicture))]
namespace SloperMobile.Droid.DependancyObjects
{
	public class SavePicture : ISavePicture
	{
		public async Task<string> SavePictureToDisk(string filename, byte[] imageData)
		{
			var dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
			var pictures = dir.AbsolutePath + "/" + AppSetting.APP_COMPANY;
			var directory = new Java.IO.File(pictures);
			if (!directory.Exists())
				directory.Mkdirs();

			//adding a time stamp time file name to allow saving more than one image... otherwise it overwrites the previous saved image of the same name
			string name = filename + System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
			string filePath = System.IO.Path.Combine(pictures, name);
			try
			{
				System.IO.File.WriteAllBytes(filePath, imageData);
				//mediascan adds the saved image into the gallery
				var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
				mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(filePath)));
				Xamarin.Forms.Forms.Context.SendBroadcast(mediaScanIntent);
			}
			catch (System.Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}

			return filePath;
		}

		public async Task<System.IO.Stream> GetImage(string path)
		{
			// Open the photo and put it in a Stream to return       
			var memoryStream = new MemoryStream();
			using (var source = File.OpenRead(path))
			{
				await source.CopyToAsync(memoryStream);
			}

			return memoryStream;
		}
	}
}