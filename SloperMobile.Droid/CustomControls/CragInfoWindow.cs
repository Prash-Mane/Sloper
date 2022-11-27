using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using SloperMobile.DataBase.DataTables;

namespace SloperMobile.Droid
{
    public class CragInfoWindow
    {
        private ImageView infoImage;
        private TextView infoAreaName;
        private TextView infoCragName;

        public View GetView(CragExtended model)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            var view = inflater.Inflate(Resource.Layout.CragInfoWindow, null);
            infoImage = view.FindViewById<ImageView>(Resource.Id.InfoWindowImage);
            infoAreaName = view.FindViewById<TextView>(Resource.Id.areaname);
            infoCragName = view.FindViewById<TextView>(Resource.Id.cragname);
         
            InitData(model);

            return view;
        }

        private void InitData(CragExtended model)
        {
            if (model == null)
                return;

            infoAreaName.Text = model.area_name;
            infoCragName.Text = model.crag_name;

            var imageBytes = model.crag_image?.GetImageBytes();
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
