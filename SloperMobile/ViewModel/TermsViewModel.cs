using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Helpers;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;

namespace SloperMobile.ViewModel
{
	public class TermsViewModel : BaseViewModel
    {
		private readonly IUserDialogs userDialogs;
		private readonly IRepository<CragExtended> cragRepository;

        public string DisclaimerContent { get; set; }

        public string CopyrightContent { get; set; }

        public string PrivacyContent { get; set; }


        public TermsViewModel(
            INavigationService navigationService,
			IUserDialogs userDialogs,
            IRepository<CragExtended> cragRepository) : base(navigationService)
        {
			this.userDialogs = userDialogs;
			this.cragRepository = cragRepository;
            PageHeaderText = "SLOPER INC" + " (v" + Settings.Version + ")";
            PageSubHeaderText = "User Terms & Conditions";
            //FooterVisible();             
			//IsMenuVisible = Cache.IsGoFromSplashScreen == true ? false : true;
			//IsBackButtonVisible = Cache.IsGoFromSplashScreen == true ? true : false;
            Offset = Common.Enumerators.Offsets.Header;
            SetPageContent();
		}

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            userDialogs.HideLoading();
        }

        //private async void FooterVisible()
        //{
        //    IsShowFooter = GeneralHelper.IsCragsDownloaded;
        //}

        private void SetPageContent()
        {
            DisclaimerContent = "\nThe inclusion of topos or of a crag or routes upon them does not imply a right of access to the crag or the right to climb upon it. Neither the authors or contributors accept any liability whatsoever for injury or damage caused to (or by) climbers, third parties, or property arising from its use. Users must rely on their own judgement and are recommended to obtain suitable insurance against injury to person, property and third party risks.Sloper Inc. (“Sloper”) endorses the participation statement that Rock climbing, hill walking and mountaineering are activities with a danger of personal injury or death. Participants in these activities should be aware of and accept those risks and be responsible for their own activities and involvement.\n\nBy using this topo app you are deemed to have read and accepted these statements.\n";

            CopyrightContent = "\nPowered by www.sloperclimbing.com topo apps Platform.\n\nThis application is the intellectual property and copyright of Sloper Inc (“Sloper”).\n\nThe authors assert their database copyright over the content of this guidebook (c).\n";

            PrivacyContent = "\nYour privacy is important to us at Sloper Inc. (“Sloper”)\n\nThe information you provide is intended to enhance your experience while using the Sloper App.\n\nWe will not sell your private information.\n\nSloper only collects personal information about you if you voluntarily provide it and share it with Sloper. You will have an opportunity to opt in and elaborate on demographic information about yourself in your profile. This will include items such as age, weight class, years climbing and climbing abilities. This information may be used to better your experience with Sloper and tailer the information that you are notified about.\n";
        }

    }
}
