// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SloperMobile.iOS
{
	[Register ("MapCallout")]
	partial class MapCallout
	{
		[Outlet]
		UIKit.UIButton btnCragDetails { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIImageView imgBg { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UILabel lblArea { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UILabel lblCrag { get; set; }

		[Outlet]
		UIKit.UIView viewBtmCorner { get; set; }

		[Outlet]
		UIKit.UIView viewWhiteBg { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (viewBtmCorner != null) {
				viewBtmCorner.Dispose ();
				viewBtmCorner = null;
			}

			if (viewWhiteBg != null) {
				viewWhiteBg.Dispose ();
				viewWhiteBg = null;
			}

			if (imgBg != null) {
				imgBg.Dispose ();
				imgBg = null;
			}

			if (lblArea != null) {
				lblArea.Dispose ();
				lblArea = null;
			}

			if (lblCrag != null) {
				lblCrag.Dispose ();
				lblCrag = null;
			}

			if (btnCragDetails != null) {
				btnCragDetails.Dispose ();
				btnCragDetails = null;
			}
		}
	}
}
