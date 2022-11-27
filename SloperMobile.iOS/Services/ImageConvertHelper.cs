using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Threading.Tasks;
using UIKit;

namespace SloperMobile.iOS
{
    public static class ImageConvertHelper
    {
        static IImageSourceHandler GetHandler(ImageSource source)
        {
            if (source is UriImageSource)
            {
                return new ImageLoaderSourceHandler();
            }
            if (source is FileImageSource)
            {
                return new FileImageSourceHandler();
            }
            if (source is StreamImageSource)
            {
                return new StreamImagesourceHandler(); // naming is inconsitent here
            }
            return null;
        }

        public static async Task<UIImage> GetNaviteImage(ImageSource source)
        {
            return await GetHandler(source).LoadImageAsync(source);
        }
    }
}
