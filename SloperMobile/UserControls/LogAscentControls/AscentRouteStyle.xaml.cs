using System;
using Xamarin.Forms;

namespace SloperMobile.UserControls
{
    public partial class AscentRouteStyle : StackLayout
    {
        //public StackLayout ObjAscentRouteStylePrv { get { return ImgAscentRouteStylePrv; } }
        //public StackLayout ObjAscentRouteStyleNxt { get { return ImgAscentRouteStyleNxt; } }
        public AscentRouteStyle()
        {
            InitializeComponent();
        }
        public void SetFrameColor(object sender, EventArgs e)
        {
            var angleframe = (Frame)sender;
            if (angleframe.AutomationId == "technical")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    angleframe.BackgroundColor = Color.Black;
                }
            }


            if (angleframe.AutomationId == "sequential")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    angleframe.BackgroundColor = Color.Black;
                }
            }


            if (angleframe.AutomationId == "powerful")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    angleframe.BackgroundColor = Color.Black;
                }
            }


            if (angleframe.AutomationId == "sustained")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    angleframe.BackgroundColor = Color.Black;
                }
            }

            if (angleframe.AutomationId == "onemove")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    angleframe.BackgroundColor = Color.Black;
                }
            }

            if (angleframe.AutomationId == "exposed")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    angleframe.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    angleframe.BackgroundColor = Color.Black;
                }
            }

            if (angleframe.AutomationId == "everything")
            {
                if (angleframe.BackgroundColor == Color.Black)
                {
                    //Technical.BackgroundColor = Color.FromHex("#FF8E2D");
                    //Sequential.BackgroundColor = Color.FromHex("#FF8E2D");
                    //Powerful.BackgroundColor = Color.FromHex("#FF8E2D");
                    //Sustained.BackgroundColor = Color.FromHex("#FF8E2D");
                    //Onemove.BackgroundColor = Color.FromHex("#FF8E2D");
                    Everything.BackgroundColor = Color.FromHex("#FF8E2D");
                }
                else
                {
                    //Technical.BackgroundColor = Color.Black;
                    //Sequential.BackgroundColor = Color.Black;
                    //Powerful.BackgroundColor = Color.Black;
                    //Sustained.BackgroundColor = Color.Black;
                    //Onemove.BackgroundColor = Color.Black;
                    Everything.BackgroundColor = Color.Black;
                }
            }
        }
    }
}
