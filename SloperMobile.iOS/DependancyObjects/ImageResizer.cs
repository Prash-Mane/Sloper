using SloperMobile.Common.Interfaces;
using SloperMobile.iOS.DependancyObjects;
using System;
using System.Drawing;
using System.IO;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(ImageResizer))]
namespace SloperMobile.iOS.DependancyObjects
{
    public class ImageResizer : IImageResizer
    {
        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            // Load the bitmap
            UIImage originalImage = ImageFromByteArray(imageData);
            
            var oriHeight = originalImage.Size.Height;
            var oriWidth = originalImage.Size.Width;
            nfloat rHeight = 0;
            nfloat rWidth = 0;

            if (oriHeight > oriWidth)
            {
                rHeight = height;
                nfloat p = oriHeight / height;
                rWidth = oriWidth / p;
            }
            else
            {
                rWidth = width;
                nfloat p = oriWidth / width;
                rHeight = oriHeight / p;
            }
            
            width = (float)rWidth;
            height = (float)rHeight;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            originalImage.Draw(new RectangleF(0, 0, width, height));
            var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            var bytesImagen = resizedImage.AsJPEG().ToArray();
            resizedImage.Dispose();
            return bytesImagen;
        }

        private UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            UIKit.UIImage image;
            try
            {
                image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }

            return image;
        }
    }
}
