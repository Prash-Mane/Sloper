using Xamarin.Forms;

namespace SloperMobile.UserControls.CustomControls
{
	public class LabelSizeFontToFitEffect : RoutingEffect
	{
        public int Lines { get; set; } = 2;

		public LabelSizeFontToFitEffect() : base("SloperMobile.LabelSizeFontToFit") { }
	}
}
