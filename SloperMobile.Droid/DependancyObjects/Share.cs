using Android.Content;
using Java.IO;
using SloperMobile.Common.Interfaces;
using SloperMobile.Droid.DependancyObjects;
using Xamarin.Forms;

[assembly: Dependency(typeof(Share))]
namespace SloperMobile.Droid.DependancyObjects
{
	public class Share : IShare
	{
		public void ShareImage(string message, string imagePath)
		{
			var intent = new Intent(Intent.ActionSend);
			intent.PutExtra(Intent.ExtraText, message);
			intent.SetType("image/*");

			var media = new File(imagePath);
			var uri = Android.Net.Uri.FromFile(media);
			intent.PutExtra(Intent.ExtraStream, uri);
			Forms.Context.StartActivity(Intent.CreateChooser(intent, "Share Image"));
		}
	}
}