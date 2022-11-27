using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.UserControls.ReportIssue
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IssueTypeToReport : ContentView
    {
        public IssueTypeToReport()
        {
            InitializeComponent();
        }
        public void SetFrameColor(object sender, EventArgs e)
        {
            IssueType_1.BackgroundColor = Color.Black;
            IssueType_2.BackgroundColor = Color.Black;
            IssueType_3.BackgroundColor = Color.Black;
            IssueType_4.BackgroundColor = Color.Black;
            var issueframe = (Frame)sender;
            issueframe.BackgroundColor = Color.FromHex("#FF8E2D");
        }
    }
}