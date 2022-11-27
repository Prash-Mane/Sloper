using Android.Graphics;
using SloperMobile.Common.Interfaces;
using SloperMobile.Droid.DependancyObjects;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(ImageResizer))]
namespace SloperMobile.Droid.DependancyObjects
{
    public class ImageResizer : IImageResizer
    {
        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            // Load the bitmap 
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            
            float rHeight = 0;
            float rWidth = 0;
            var oriHeight = originalImage.Height;
            var oriWidth = originalImage.Width;
            
            if (oriHeight > oriWidth)
            {
                rHeight = height;
                float p = oriHeight / height;
                rWidth = oriWidth / p;
            }
            else
            {
                rWidth = width;
                float p = oriWidth / width;
                rHeight = oriHeight / p;
            }
            
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)rWidth, (int)rHeight, false);
            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            }
        }
    }
}