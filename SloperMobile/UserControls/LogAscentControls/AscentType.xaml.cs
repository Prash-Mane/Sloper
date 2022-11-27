using System;
using Xamarin.Forms;

namespace SloperMobile.UserControls
{
	public partial class AscentType : StackLayout
    {
        public StackLayout ObjImgAcentTypeNxt { get { return ImgAcentTypeNxt; } }
        public AscentType()
        {
            InitializeComponent();

            //set default ascent type to Onsight
			Onsight.BackgroundColor = Color.FromHex("#FF8E2D");
		}
        public void SetFrameColor(object sender, EventArgs e)
        {
            //reset all buttons
            Onsight.BackgroundColor = Color.Black;
            Flash.BackgroundColor = Color.Black;
            Redpoint.BackgroundColor = Color.Black;
            Repeat.BackgroundColor = Color.Black;
            ProjectBurn.BackgroundColor = Color.Black;
            OneHang.BackgroundColor = Color.Black;

            //set clicked button to active selection
            var ascframe = (Frame)sender;
            ascframe.BackgroundColor = Color.FromHex("#FF8E2D");
        }
    }
}
