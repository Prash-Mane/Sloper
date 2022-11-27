// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace SloperMobile.iOS
{
    [Register ("SectorCallout")]
    partial class SectorCallout
    {
        [Outlet]
        UIKit.UIView viewBtmCorner { get; set; }


        [Outlet]
        UIKit.UIView viewWhiteBg { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView imgBg { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSector { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (imgBg != null) {
                imgBg.Dispose ();
                imgBg = null;
            }

            if (lblSector != null) {
                lblSector.Dispose ();
                lblSector = null;
            }

            if (viewBtmCorner != null) {
                viewBtmCorner.Dispose ();
                viewBtmCorner = null;
            }

            if (viewWhiteBg != null) {
                viewWhiteBg.Dispose ();
                viewWhiteBg = null;
            }
        }
    }
}