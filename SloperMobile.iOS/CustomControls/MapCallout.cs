using Foundation;
using ObjCRuntime;
using System;
using UIKit;
using SloperMobile.UserControls.CustomControls;
using SloperMobile.DataBase.DataTables;
using CoreGraphics;

namespace SloperMobile.iOS
{
    public partial class MapCallout : UIView
    {
        public MapCallout (IntPtr handle) : base (handle) { }

        public static MapCallout Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("MapCallout", null, null);
            var v = Runtime.GetNSObject<MapCallout>(arr.ValueAt(0));
            return v;
        }

        public void InitView(CragExtended model) 
        {
            if (model == null)
                return;

            //lblArea.Layer.ShadowRadius = lblCrag.Layer.ShadowRadius = 1.3f;
            //lblArea.Layer.ShadowOpacity = lblCrag.Layer.ShadowOpacity = 1f;
            //lblArea.Layer.ShadowOffset = lblCrag.Layer.ShadowOffset = new System.Drawing.SizeF(1f, 1f);
            //lblArea.Layer.ShadowColor = lblCrag.Layer.ShadowColor = UIColor.FromRGB(25, 25, 25).CGColor;

            lblArea.Text = model.area_name;
            lblCrag.Text = model.crag_name;
            var imageBytes = model.crag_image?.GetImageBytes();
            if (imageBytes != null)
                imgBg.Image = UIImage.LoadFromData(NSData.FromArray(imageBytes));

            btnCragDetails.Layer.BorderWidth = viewBtmCorner.Layer.BorderWidth = viewWhiteBg.Layer.BorderWidth = 0.5f;
            btnCragDetails.Layer.BorderColor = viewBtmCorner.Layer.BorderColor = viewWhiteBg.Layer.BorderColor = UIColor.DarkGray.CGColor;

            viewBtmCorner.Transform = CGAffineTransform.MakeRotation((nfloat)Math.PI / 4f);
        }
    }
}