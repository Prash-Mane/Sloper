using System;

using Xamarin.Forms;

namespace SloperMobile.UserControls
{
	public partial class AscentClimbingAngle : StackLayout
    {
        public StackLayout ObjImgClmAnglePrv { get { return ImgClmAnglePrv; } }
        public StackLayout ObjImgClmAngleNxt { get { return ImgClmAngleNxt; } }
        public AscentClimbingAngle()
        {
            InitializeComponent();
        }
        public void SetFrameColor(object sender, EventArgs e)
        {
            var angleframe = (Frame)sender;
            if (angleframe.BackgroundColor == Color.Black)
            {
                angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
            }
            else
            {
                angleframe.BackgroundColor = Color.Black;
            }
        }
    }
}
