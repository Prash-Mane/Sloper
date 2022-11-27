using Xamarin.Forms;

namespace SloperMobile.UserControls
{
    public partial class AscentRating : StackLayout
    {
        public StackLayout ObjImgAcentRatingPrv { get { return ImgAscRatingPrv; } }
        public StackLayout ObjImgAcentRatingNxt { get { return ImgAscRatingNxt; } }
        public AscentRating()
        {
            InitializeComponent();
        }
    }
}
