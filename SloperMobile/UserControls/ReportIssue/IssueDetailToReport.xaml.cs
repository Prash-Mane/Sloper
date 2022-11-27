using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace SloperMobile.UserControls.ReportIssue
{
    public partial class IssueDetailToReport : ContentView
    {
        public IssueDetailToReport()
        {
            InitializeComponent();
        }
        public void SetFrameColor(object sender, EventArgs e)
        {
            IssueTypeDetail_1.BackgroundColor = Color.Black;
            IssueTypeDetail_2.BackgroundColor = Color.Black;
            IssueTypeDetail_3.BackgroundColor = Color.Black;
            IssueTypeDetail_4.BackgroundColor = Color.Black;
            var issueframe = (Frame)sender;
            issueframe.BackgroundColor = Color.FromHex("#FF8E2D");
        }
    }
}
