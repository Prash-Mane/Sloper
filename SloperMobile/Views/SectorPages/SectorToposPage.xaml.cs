using System;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using Autofac;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.ResponseModels;
using SloperMobile.Model.TopoModels;
using SloperMobile.ViewModel.SectorViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.SectorPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SectorToposPage : CarouselPage, INavigationAware
	{
		private readonly IContainerRegistry container;
        private readonly IRepository<TopoTable> topoRepository;
		private readonly IUserDialogs userDialogs;
        private INavigationService navigationService;
        private Action<NavigationParameters> navigateBack;
        private MapListModel CurrentSector;
		private int pageIndex;
		private int routeId = 0;
		private bool singleRoute;
        private bool isCragUnlocked;
        private long guideBookId;
        private bool isNavigatedFromSectorImage;

        public SectorToposPage(
            IContainerRegistry container,
			IRepository<TopoTable> topoRepository,
			IUserDialogs userDialogs)
		{
			InitializeComponent();
			this.container = container;
			this.topoRepository = topoRepository;
			this.userDialogs = userDialogs;
			userDialogs.ShowLoading("Loading...");
		}
		public void OnNavigatedFrom(NavigationParameters parameters)
        {
            App.ChangeMenuPresenter(true);
		}

		public void OnNavigatedTo(NavigationParameters parameters)
		{
			navigateBack = (Action<NavigationParameters>)parameters[NavigationParametersConstants.BackCommandParameter];
			InitialiTopoCarouselChildrens();
		}
	
		public void OnNavigatingTo(NavigationParameters parameters)
		{
            App.ChangeMenuPresenter(false);
			var CurrentSector = (MapListModel)parameters[NavigationParametersConstants.SelectedSectorObjectParameter];
			var routeId = (int)parameters[NavigationParametersConstants.RouteIdParameter];
			var singleRoute = (bool)parameters[NavigationParametersConstants.SingleRouteParameter];
            this.navigationService = (INavigationService)parameters[NavigationParametersConstants.NavigatonServiceParameter];
            this.isCragUnlocked = (bool)parameters[NavigationParametersConstants.IsUnlockedParameter];
            this.guideBookId = (long)parameters[NavigationParametersConstants.GuideBookIdParameter];
            this.isNavigatedFromSectorImage = (bool)parameters[NavigationParametersConstants.IsNavigatedFromSectorImageParameter];
            this.CurrentSector = CurrentSector;
			this.routeId = Convert.ToInt32(routeId);
			this.singleRoute = singleRoute;
			pageIndex = 0;
		}

		private async void InitialiTopoCarouselChildrens()
		{
			var topolistData = await topoRepository.FindAsync(sec => sec.sector_id == CurrentSector.SectorId);
			var topoimgages = JsonConvert.DeserializeObject<List<TopoImageResponseModel>>(topolistData.topo_json).Where(x => !string.IsNullOrEmpty(x.image.data)).ToList();
			if (routeId > 0)
			{
				//Finding all topos with selected route
				var allToposWithCurrentRoute = topoimgages?.Select(item =>
				{
					var drawing = item.drawing.FirstOrDefault(route => Convert.ToInt32(route.id) == routeId);
					return drawing != null ? new TopoImageResponseModel
					{
						drawing = new TopoDrawingModel[] { drawing },
						image = item.image
					} : null;
				}).ToList();

				if (allToposWithCurrentRoute.All(item => item == null))
				{
					SetupTopoPage(null, 1);
					return;
				}

				var topos = allToposWithCurrentRoute.Where(items => items != null).ToList();
				//Creating pages with specified topos
				foreach (var topoImage in topos)
				{
					SetupTopoPage(topoImage, topos.Count);
				}
			}
			else
			{
				//TODO: Temporary fix, while data from the Api is invalid. We don't need to have image data, while there is no routes on it and there is no topos.
				if(topoimgages.All(item => item.drawing == null || item.drawing.Count == 0))
				{
					userDialogs.HideLoading();
					userDialogs.Toast(new ToastConfig("")
					{
						Message = " No Topos Found!",
						BackgroundColor = System.Drawing.Color.FromArgb(128, 60, 0),
						MessageTextColor = System.Drawing.Color.White,
						Duration = TimeSpan.FromSeconds(3)
					});

					await navigationService.GoBackAsync();
					return;
				}

				//load all carousel page with images when click on image
				foreach (var topores in topoimgages)
				{
					SetupTopoPage(topores, topoimgages.Count);
				}
			}
		}

		private void SetupTopoPage(TopoImageResponseModel topoImage, int childrenCount)
		{
			var page = App.ContainerProvider.Resolve<SectorPages.SectorTopoDetailsPage>() as SectorPages.SectorTopoDetailsPage;
			if (ViewModelLocator.GetAutowireViewModel(page) == null)
				ViewModelLocator.SetAutowireViewModel(page, true);
			var navigatioParameters = new NavigationParameters();
			navigatioParameters.Add(NavigationParametersConstants.SelectedSectorObjectParameter, CurrentSector);
			navigatioParameters.Add(NavigationParametersConstants.RouteIdParameter, routeId);
			navigatioParameters.Add(NavigationParametersConstants.SingleRouteParameter, singleRoute);
			navigatioParameters.Add(NavigationParametersConstants.PageIndexParameter, pageIndex);
			navigatioParameters.Add(NavigationParametersConstants.TopoImageResponseParameter, topoImage);
			navigatioParameters.Add(NavigationParametersConstants.ChildrenCountParameter, childrenCount);
            navigatioParameters.Add(NavigationParametersConstants.IsUnlockedParameter, isCragUnlocked);
            navigatioParameters.Add(NavigationParametersConstants.GuideBookIdParameter, guideBookId);
            navigatioParameters.Add(NavigationParametersConstants.IsNavigatedFromSectorImageParameter, isNavigatedFromSectorImage);
			(page as INavigationAware)?.OnNavigatingTo(navigatioParameters);
			(page.BindingContext as SectorTopoDetailsViewModel)?.OnNavigatingTo(navigatioParameters);
			this.Children.Add(page);
			pageIndex++;
            (page as INavigationAware)?.OnNavigatedTo(navigatioParameters);
        }
	}
}