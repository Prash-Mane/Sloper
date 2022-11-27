using SloperMobile.Common.Constants;
namespace SloperMobile.UserControls
{
    public partial class HeaderUC
    {
        public HeaderUC()
        {
            InitializeComponent();
            Margin = new Xamarin.Forms.Thickness(0, SizeHelper.StatusBarHeight, 0, 0);
        }
    }
}
