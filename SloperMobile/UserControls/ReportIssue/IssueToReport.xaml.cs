using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.UserControls.ReportIssue
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class IssueToReport : StackLayout
    {
        public IssueToReport()
        {
            InitializeComponent();
        }
        public void SetFrameColor(object sender, EventArgs e)
        {
            AllBolts.BackgroundColor = Color.Black;
            SingleBolt.BackgroundColor = Color.Black;
            Anchors.BackgroundColor = Color.Black;
            RockQuality.BackgroundColor = Color.Black;
            var issueframe = (Frame)sender;
            issueframe.BackgroundColor = Color.FromHex("#FF8E2D");
        }
    }
}