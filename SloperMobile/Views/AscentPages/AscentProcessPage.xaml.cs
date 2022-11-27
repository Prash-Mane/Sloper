using System;
using Acr.UserDialogs;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.AscentPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AscentProcessPage : CarouselPage, INavigationAware
	{
		private readonly IExceptionSynchronizationManager exceptionManager;
		private int TopBarHeight = 0;
        private bool isDiamondSelected;
        private double GridBounds = 18;
        public double FooterUC_Height = 40, BackHeaderUC_Height = 40;
        public MapListModel CurrentSector { get; set; }
		private readonly IRepository<TopoTable> topoRepository;
		private bool clicked;

		public AscentProcessPage(
			IRepository<CragImageTable> cragImageRepository,
			IExceptionSynchronizationManager exceptionManager,
			IRepository<TopoTable> topoRepository,
			IGetImageBytes imageBytesService)
		{
			InitializeComponent();
			NavigationPage.SetHasNavigationBar(this, false);
			this.topoRepository = topoRepository;
			this.exceptionManager = exceptionManager;
		}

		void tapImageNext_Tapped(object sender, EventArgs e)
		{
			try
			{
				if (clicked)
				{
					return;
				}

				clicked = true;
				var index = Children.IndexOf(CurrentPage);
				if (index == -1)
				{
					return;
				}

				if (index < Children.Count)
					SelectedItem = Children[index + 1];
				clicked = false;
			}
			catch (Exception exception)
			{
			}
			finally
			{
				clicked = false;
			}
		}

		void tapImagePrev_Tapped(object sender, EventArgs e)
		{
			try
			{
				if (clicked)
				{
					return;
				}

				clicked = true;
				var index = Children.IndexOf(CurrentPage);
				if (index == -1)
				{
					return;
				}

				if (index > Children.Count)
				{
					SelectedItem = Children[index + 1];
				}
				if (index < Children.Count)
					SelectedItem = Children[index - 1];
				clicked = false;

			}
			catch (Exception exception)
			{

			}
			finally
			{
				clicked = false;
			}
		}

		protected async override void OnCurrentPageChanged()
		{
			try
			{
				base.OnCurrentPageChanged();
				var index = Children.IndexOf(CurrentPage);
				SelectedItem = Children[index];

                ((BaseViewModel)CurrentPage.BindingContext).IsBackButtonVisible = index == 0;
			}
			catch (Exception)
			{
			}
			finally
			{
				UserDialogs.Instance.HideLoading();
			}
		}

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            try
            {

			foreach (var child in Children)
			{
				(child as INavigationAware)?.OnNavigatingTo(parameters);
				(child?.BindingContext as INavigationAware)?.OnNavigatingTo(parameters);
				}
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatedTo),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message
				});
			}
		}

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            try
            {
                var CurrentSector = (MapListModel)parameters[NavigationParametersConstants.CurrentSectorParameter];
                if (AppSetting.APP_TYPE == "indoor")
                {
                    Children.RemoveAt(3);
                }

                this.CurrentSector = CurrentSector;
                var tapImageNext = new TapGestureRecognizer();
                tapImageNext.Tapped += tapImageNext_Tapped;
                var tapImagePrev = new TapGestureRecognizer();
                tapImagePrev.Tapped += tapImagePrev_Tapped;
                if (Device.RuntimePlatform == Device.iOS)
                {
                    TopBarHeight = 18;
                }
                else
                {
                    TopBarHeight = 40;
                }

                ascType.ObjImgAcentTypeNxt.GestureRecognizers.Add(tapImageNext);
                ascDate.ObjImgAcentDateNxt.GestureRecognizers.Add(tapImageNext);
                ascDate.ObjImgAcentDatePrv.GestureRecognizers.Add(tapImagePrev);

                ascRating.ObjImgAcentRatingNxt.GestureRecognizers.Add(tapImageNext);
                ascRating.ObjImgAcentRatingPrv.GestureRecognizers.Add(tapImagePrev);
                ascClimbingAngle.ObjImgClmAngleNxt.GestureRecognizers.Add(tapImageNext);
                ascClimbingAngle.ObjImgClmAnglePrv.GestureRecognizers.Add(tapImagePrev);

				ascHoldType.ObjAscentHoldTypeNxt.GestureRecognizers.Add(tapImageNext);
				ascHoldType.ObjAscentHoldTypePrv.GestureRecognizers.Add(tapImagePrev);
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnNavigatingTo),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"CurrentRouteId = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.CurrentRouteIdParameter])}, CurrentSector = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.CurrentSectorParameter])}, topoImageResponse = {Newtonsoft.Json.JsonConvert.SerializeObject(parameters[NavigationParametersConstants.TopoImageResponseParameter])}"
				});
			}
		}
	}
}