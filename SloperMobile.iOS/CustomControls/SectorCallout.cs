using Foundation;
using ObjCRuntime;
using System;
using UIKit;
using SloperMobile.Model;
using CoreGraphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Threading.Tasks;

namespace SloperMobile.iOS
{
    public partial class SectorCallout : UIView
    {
        public SectorCallout (IntPtr handle) : base (handle) { }

        public static SectorCallout Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("SectorCallout", null, null);
            var v = Runtime.GetNSObject<SectorCallout>(arr.ValueAt(0));
            return v;
        }

        public void InitView(SectorMapModel model)
        {
            if (model == null)
                return;

            //lblSector.Layer.ShadowRadius = 1.3f;
            //lblSector.Layer.ShadowOpacity = 1f;
            //lblSector.Layer.ShadowOffset = new CGSize(1f, 1f);
            //lblSector.Layer.ShadowColor = UIColor.FromRGB(25, 25, 25).CGColor;

            lblSector.Text = model.SectorName.ToUpper();

            if (model.SectorImageBytes != null)
                imgBg.Image = UIImage.LoadFromData(NSData.FromArray(model.SectorImageBytes));

            viewBtmCorner.Layer.BorderWidth = viewWhiteBg.Layer.BorderWidth = 0.5f;
            viewBtmCorner.Layer.BorderColor = viewWhiteBg.Layer.BorderColor = UIColor.DarkGray.CGColor;

            viewBtmCorner.Transform = CGAffineTransform.MakeRotation((nfloat)Math.PI / 4f);
        }
    }
}