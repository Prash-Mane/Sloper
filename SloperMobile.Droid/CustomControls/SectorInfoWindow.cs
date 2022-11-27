using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace SloperMobile.Droid
{
    public class SectorInfoWindow
    {
        private ImageView infoImage;
        private TextView sectorName;

        public View GetView(SectorMapModel model)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            var view = inflater.Inflate(Resource.Layout.SectorInfoWindow, null);
            infoImage = view.FindViewById<ImageView>(Resource.Id.InfoWindowImage);
            sectorName = view.FindViewById<TextView>(Resource.Id.sectorname);
           
            InitData(model);

            return view;

        }

        private void InitData(SectorMapModel model)
        {
            if (model == null)
                return;

            sectorName.Text = model.SectorName;

            var imageBytes = model.SectorImageBytes;
            Bitmap bitmap = null;
            if (imageBytes != null)
                bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

            if (bitmap != null)
                infoImage.SetImageBitmap(bitmap);
            else
                infoImage.SetImageResource(Resource.Drawable.default_sloper_outdoor_square);
        }
    }
}
