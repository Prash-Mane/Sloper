using Acr.UserDialogs;
using System.IO;
using Android.Graphics;
using Com.Theartofdev.Edmodo.Cropper;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using SloperMobile.Droid.Renderers;
using SloperMobile.UserControls;
using System.Threading.Tasks;

[assembly: ExportRenderer(typeof(CropView), typeof(CropViewRenderer))]
namespace SloperMobile.Droid.Renderers
{
    public class CropViewRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            var page = Element as CropView;
            if (page != null)
            {
                var cropImageView = new CropImageView(Context);
                cropImageView.SetFixedAspectRatio(true);
                cropImageView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                Bitmap bitmp = BitmapFactory.DecodeByteArray(page.Image, 0, page.Image.Length);
                cropImageView.SetImageBitmap(bitmp);

                var stackLayout = new StackLayout { Children = { cropImageView } };

                var rotateButton = new Button { Text = "Rotate" };

                rotateButton.Clicked += (sender, ex) =>
                {
                    cropImageView.RotateImage(90);
                };
                stackLayout.Children.Add(rotateButton);

                var finishButton = new Button { Text = "Finished" };
                finishButton.Clicked += (sender, ex) =>
                {
                    UserDialogs.Instance.ShowLoading("Saving Image...");
                    Task.Delay(1000);
                    Bitmap cropped = cropImageView.CroppedImage;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        cropped.Compress(Bitmap.CompressFormat.Png, 100, memory);
                        App.CroppedImage = memory.ToArray();
                    }
                    page.DidCrop = true;
                    UserDialogs.Instance.Loading().Hide();
                    page.Navigation.PopModalAsync();
                };
                stackLayout.Children.Add(finishButton);
                page.Content = stackLayout;
            }
        }
    }
}

