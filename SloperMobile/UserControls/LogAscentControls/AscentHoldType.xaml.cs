using System;
using Xamarin.Forms;

namespace SloperMobile.UserControls
{
	public partial class AscentHoldType : StackLayout
	{
		public StackLayout ObjAscentHoldTypePrv { get { return ImgAscentHoldTypePrv; } }
		public StackLayout ObjAscentHoldTypeNxt { get { return ImgAscentHoldTypeNxt; } }
		public AscentHoldType()
        {
            InitializeComponent();
        }

        public void SetFrameColor(object sender, EventArgs e)
        {
            var holdframe = (Frame)sender;
            if (holdframe.BackgroundColor == Color.Black)
            {
                holdframe.BackgroundColor = Color.FromHex("#FF8E2D");
            }
            else
            {
                holdframe.BackgroundColor = Color.Black;
            }
        }
    }
}
