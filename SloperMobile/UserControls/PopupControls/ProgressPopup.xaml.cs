using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SloperMobile.Common.Constants;
using System;
using Xamarin.Forms;
using System.Linq;
using Prism.Ioc;

namespace SloperMobile.UserControls.PopupControls
{
    public partial class ProgressPopup : PopupPage, IProgress<(string message, decimal progress)>
	{
        IDownloadManager downloadManager;
        INotificationService notificationService;

        public ProgressPopup()
		{
			InitializeComponent();

            CloseWhenBackgroundIsClicked = false;

            bgImage.Source = MapModelHelper.GetDefaultImage(false);

            downloadManager = (App.Current as App).Container.Resolve<IDownloadManager>();
            notificationService = DependencyService.Get<INotificationService>(DependencyFetchTarget.NewInstance);
            if (notificationService != null)
                notificationService.NotificationType = NotificationTypes.OverlayProgress;
        }

        public bool PopOnFinish { get; set; } = true;

        public ImageSource BackgroundImageSource
        {
            set => bgImage.Source = value;
        }

        public string MessageTitle
        {
            set => lblTitle.Text = value;
        }

        public bool InverseProgress { get; set; }

        public void Report((string message, decimal progress) args)
        {
            if (args.progress > 1)
                args.progress = 1;

            notificationService?.UpdateProgress(args.message, (float)args.progress * 100);

            lblMessage.Text = $"{args.message}";
            if (InverseProgress)
                imgWhite.HeightRequest = 200 * (double)args.progress;
            else
                imgWhite.HeightRequest = 200 * (double)(1 - args.progress);

            if (args.progress == 1 && PopOnFinish && PopupNavigation.PopupStack.Count > 0)
            {
                PopOnFinish = false;
                PopupNavigation.PopAllAsync(false);
            }
        }

        protected override bool OnBackButtonPressed() => true; // Disable back button

        protected override void OnAppearing()
        {
            base.OnAppearing();
            downloadManager.IsDisplayToast(false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            downloadManager.IsDisplayToast(true);
            notificationService?.EndProgress();
        }
    }
}