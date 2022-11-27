using System;
using Prism.Navigation;
using SloperMobile.Common.Interfaces;
using SloperMobile.ViewModel;
using SloperMobile.Common.Extentions;
using Xamarin.Forms;
using System.Collections.Generic;
using SloperMobile.Model;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Common.Constants;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;

namespace SloperMobile.ViewModel.WelcomeOverviewViewModels
{
    public class OverviewSlidesViewModel : BaseViewModel, INavigatingAware
    {
        private IList<CarouselPagesModel> carouselPages;
        private IList<WelcomeSlideModel> slides;
        private int selectedItemPosition;
        private string commandText;
        private bool isindicatorVisible;
        private Thickness margin;
        public Command CloseCommand { get; set; }
        public Command NextCommand { get; set; }
        public Command PrevCommand { get; set; }
        public OverviewSlidesViewModel(
            INavigationService navigationService,
            IExceptionSynchronizationManager exceptionManager,
            IHttpHelper httpHelper = null) : base(navigationService, exceptionManager, httpHelper)
        {
            HasFade = true;
            CloseCommand = new Command(OnClosePressed);
            NextCommand = new Command(ExecuteOnNext);
            PrevCommand = new Command(ExecuteOnBack);
        }
        #region Properties
        public IList<CarouselPagesModel> CarouselPages
        {
            get => carouselPages;
            set => SetProperty(ref carouselPages, value);
        }
        public IList<WelcomeSlideModel> Slides
        {
            get => slides;
            set => SetProperty(ref slides, value);
        }

        public int SelectedItemPosition
        {
            get => selectedItemPosition;
            set
            {
                SetProperty(ref selectedItemPosition, value);
                RaisePropertyChanged("IsIndicatorVisible");
                RaisePropertyChanged("CommandText"); 
                RaisePropertyChanged("BottomMargin");
            }
        }

        public bool IsIndicatorVisible
        {
            get { return SelectedItemPosition == 0 ? isindicatorVisible = true : isindicatorVisible = (SelectedItemPosition == Slides.Count - 1 ? false : true); }
            set => SetProperty(ref isindicatorVisible, value);
        }

        public Thickness BottomMargin
        {
            get { return margin = Slides == null ? new Thickness(0, 0, 0, 185) : new Thickness(0, 0, 0, Convert.ToDouble(Slides[SelectedItemPosition].BottomMargin, CultureInfo.InvariantCulture)); }
            set => SetProperty(ref margin, value);
        }

        public string CommandText
        {
            get { return SelectedItemPosition == 0 ? commandText = "GET STARTED" : commandText = (SelectedItemPosition == Slides.Count - 1 ? "CONTINUE" : "NEXT"); }
            set => SetProperty(ref commandText, value);
        }
        #endregion

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            try
            {
                List<CarouselPagesModel> lst_page = new List<CarouselPagesModel>();
                var appassembly = typeof(App).GetTypeInfo().Assembly;
                Stream stream = appassembly.GetManifestResourceStream($"SloperMobile.Embedded.WelcomeSildes.WelcomeSlide.json");
                string myJson;
                using (var reader = new StreamReader(stream))
                {
                    myJson = reader.ReadToEnd();
                }
                var _slides = JsonConvert.DeserializeObject<List<WelcomeSlideModel>>(myJson);
                Slides = new List<WelcomeSlideModel>(_slides.OrderBy(sl => sl.SortOrder)); 
                foreach (WelcomeSlideModel slide in Slides)
                {

                    lst_page.Add(
                        new CarouselPagesModel
                        {
                            BackGroundImage = slide.SlideName
                        }
                    );
                }
                CarouselPages = lst_page;
                /*
                CarouselPages = new List<CarouselPagesModel> {
                    new CarouselPagesModel
                    {
                        BackGroundImage = lst_page[0].BackGroundImage
                    }
                };*/
            }
            catch (System.Exception exception)
            {
                await exceptionManager.LogException(new ExceptionTable
                {
                    Method = nameof(this.OnNavigatingTo),
                    Page = this.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Exception = exception.Message,
                    Data = $"sloperId = {parameters[NavigationParametersConstants.SloperIdParameter]}, guideBookId = {parameters[NavigationParametersConstants.GuideBookIdParameter]}"
                });
            }
        }
        private async void OnClosePressed()
        {
            await navigationService.NavigateFromMenuAsync<HomeViewModel>();
        }
        private async void ExecuteOnNext()
        {

            var index = SelectedItemPosition;

            if (index == Slides.Count - 1)
            {
                await navigationService.NavigateFromMenuAsync<HomeViewModel>();
            }
            /*
            else
            {
                if (CarouselPages.Count > index + 1)
                {
                    for (int i = CarouselPages.Count - 1; i >= index + 1; i--)
                    {
                        CarouselPages.RemoveAt(i);
                    }
                }
                List<CarouselPagesModel> lst_page = new List<CarouselPagesModel>();
                for (int i = 0; i <= index + 1; i++)
                {
                    lst_page.Add(new CarouselPagesModel
                    {
                        BackGroundImage = Slides[i].SlideName
                    });
                }
                CarouselPages = lst_page;
                SelectedItemPosition = index + 1;
            }*/
        }
        private void ExecuteOnBack()
        {
            /*
            var index = SelectedItemPosition;
            if (index > 0)
                SelectedItemPosition = index - 1;
            */
        }
    }
}
