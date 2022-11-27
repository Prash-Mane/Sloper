using System;
using Foundation;
using SloperMobile.Common.Interfaces;
using SloperMobile.iOS.DependancyObjects;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Share))]
namespace SloperMobile.iOS.DependancyObjects
{
	public class Share : IShare
	{
		public async void ShareImage(string message, string images)
		{
			try
			{
				var assetsLibrary = new AssetsLibrary.ALAssetsLibrary();
				assetsLibrary.AssetForUrl(new NSUrl(images), (asset) =>
					{
						try
						{
							var image = new UIImage(asset.DefaultRepresentation.GetFullScreenImage());
							var img = NSObject.FromObject(image);
							var mess = NSObject.FromObject(message);
							var activityItems = new[] { mess, img };
							var activityController = new UIActivityViewController(activityItems, null);
							var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;
							while (topController.PresentedViewController != null)
							{
								topController = topController.PresentedViewController;
							}

							topController.PresentViewController(activityController, true, () => { });
						}
						catch (Exception exception)
						{

						}
					}, (failure) =>
					{
					}
				);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}