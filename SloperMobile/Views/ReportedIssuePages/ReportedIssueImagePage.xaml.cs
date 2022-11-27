using System.IO;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.ReportedIssuePages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportedIssueImagePage : ContentPage, INavigatedAware
	{
		private ZoomableScrollView parent;
		private double initialHeight;
		private double initialWidth;
		private byte[] imageBytes;
		private float height;
		private float width;
		private float globalHeight;
		private float globalWidth;
		private float yRatio;
		private BaseViewModel viewModel;
		private double scaleIn = 1;
		private double scaleOut = 1;

		public ReportedIssueImagePage()
        {
            InitializeComponent();
		}

		public void OnNavigatedFrom(NavigationParameters parameters)
		{
		}

		public void OnNavigatedTo(NavigationParameters parameters)
		{
			imageBytes = (byte[]) parameters[NavigationParametersConstants.ImageBytesParameter];
			height = (float) parameters[NavigationParametersConstants.ImageHeightParameter];
			width = (float) parameters[NavigationParametersConstants.ImageWidthParameter];

			yRatio = (float) (ImageGrid.Height / height);
			globalHeight = (float) ImageGrid.Height;
			globalWidth = (float) width * yRatio;
			initialHeight = globalHeight;
			initialWidth = globalWidth;
			ImageGrid.HeightRequest = globalHeight;
            if (imageBytes != null)
            {
                ScrollableImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                ScrollableImage.HeightRequest = globalHeight;
            }
		}
	}
}
