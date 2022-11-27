using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Mvvm;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.ViewModel.GuideBookViewModels;
using SloperMobile.ViewModel.MyViewModels;
using SloperMobile.ViewModel.SubscriptionViewModels;
using SloperMobile.ViewModel.UserViewModels;
using SloperMobile.Views;
using SloperMobile.Views.CragPages;
using SloperMobile.Views.FlyoutPages;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.MasterDetailViewModels
{
	public class MainMasterDetailViewModel : BindableBase
	{
		private readonly IRepository<CragExtended> cragRepository;
        private readonly IRepository<AreaTable> areaRepository;
        private readonly INavigationService navigationService;
        private readonly IUserDialogs userDialogs;
		private readonly IExceptionSynchronizationManager exceptionManager;

        public static MainMasterDetailViewModel Instance { get; private set; }

		public MainMasterDetailViewModel(
			INavigationService navigationService,
			IRepository<CragExtended> cragRepository,
            IRepository<AreaTable> areaRepository,
			IExceptionSynchronizationManager exceptionManager,
			IUserDialogs userDialogs)
		{
			this.navigationService = navigationService;
			this.cragRepository = cragRepository;
            this.areaRepository = areaRepository;
            this.userDialogs = userDialogs;
			this.exceptionManager = exceptionManager;
			FillMenuItems();
			TapBackCommand = new Command(TapOnBackImage);
            //Cache.IsGoFromSplashScreen = false;
            Instance = this;

            App.ChangeMenuPresenter = (isPresented) =>
            {
                IsGestureEnabled = isPresented;
            };
			ChangeColor();
		}


		private ObservableCollection<MasterPageItemModel> menuList;
		/// <summary>
		/// Get and set the user menu list
		/// </summary>
		public ObservableCollection<MasterPageItemModel> MenuList
		{
			get { return menuList; }
			set { menuList = value; RaisePropertyChanged(); }
		}

		#region DelegateCommand

		public ICommand TapBackCommand { get; set; }
        public ICommand HeaderCommand { get => new Command(GoHome); }

        #endregion

        //private MasterPageItemModel selecteMenu;
        //not used?
        //public MasterPageItemModel SelectedMenu
        //{
        //	get { return selecteMenu; }
        //	set
        //	{
        //		selecteMenu = value;
        //              if (value?.Title != null) //why??
        //		{
        //			var menuDetails = cragRepository.GetAsync(x => x.is_enabled && x.is_downloaded).Result;
        //			if (menuDetails.Count > 0)
        //			{
        //				var selectedItems = menuDetails?.FirstOrDefault(s => s.crag_name == selecteMenu.Title);
        //			}
        //		}
        //		RaisePropertyChanged();
        //	}
        //}

        public async void FillMenuItems()
		{
			try
			{
				CommonSelectedItemChangeColor();
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Line = 91,
					Method = nameof(this.FillMenuItems),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}

		//TODO: REFACTOR THIS CODE!
		public void ChangeColor()
		{
			MessagingCenter.Subscribe<GoogleMapPinPage>(this, "ChangeSelectedItemColor", async (sender) =>
			{
				try
				{
					CommonSelectedItemChangeColor();
				}
				catch (Exception ex)
				{
					await exceptionManager.LogException(new ExceptionTable
					{
						Line = 118,
						Method = nameof(this.ChangeColor),
						Page = this.GetType().Name,
						StackTrace = ex.StackTrace,
						Exception = ex.Message,
					});
				}
			});

			MessagingCenter.Subscribe<MainFlyoutPage>(this, "ChangeSelectedItemColorMenu", async (sender) =>
			{
				try
				{
					CommonSelectedItemChangeColor();
				}
				catch (Exception ex)
				{
					await exceptionManager.LogException(new ExceptionTable
					{
						Line = 137,
						Method = nameof(this.ChangeColor),
						Page = this.GetType().Name,
						StackTrace = ex.StackTrace,
						Exception = ex.Message,
					});
				}
			});
		}

		private async void CommonSelectedItemChangeColor()
		{
			try
			{
			    var tempMenuList = new List<MasterPageItemModel>();
                if (Settings.ActiveCrag != 0)
                {
                   
                    var crag = await cragRepository.GetAsync(Settings.ActiveCrag);
                    var cragMenuItem = new MasterPageItemModel
                    {
                        Title = crag.crag_name.ToUpper(),
                        ItemId = crag.crag_id,
                        ContentHeight = 15,
                        IsContentVisible = true,
                        Contents = (AppSetting.APP_TYPE == "indoor") ? crag.crag_parking_info : crag.area_name,
                        TargetType = typeof(CragDetailsViewModel),
                        ActiveTextColor = Color.FromHex("#FF8E2D")
                    };
                    tempMenuList.Add(cragMenuItem);
                }

                tempMenuList.Add(new MasterPageItemModel
				{
                    Title = "GUIDEBOOKS",
					IconSource = "",
					ContentHeight = 0,
					IsContentVisible = false,
                    TargetType = typeof(GuideBookViewModel),
                    ActiveTextColor = Color.White,
				});

                tempMenuList.Add(new MasterPageItemModel
                {
                    Title = "MAPS",
                    IconSource = "",
                    ContentHeight = 0,
                    IsContentVisible = false,
                    TargetType = typeof(GoogleMapPinsViewModel),
                    ActiveTextColor = Color.White,
                });

                tempMenuList.Add(new MasterPageItemModel
                {
                    Title = "DOWNLOADS",
                    IconSource = "",
                    ContentHeight = 0,
                    IsContentVisible = false,
                    TargetType = typeof(ManageDownloadsViewModel),
                    ActiveTextColor = Color.White,
                });

                if (Settings.ActiveCrag != 0)
                {
                    tempMenuList.Add(new MasterPageItemModel
                    {
                        Title = "UPDATES",
                        IconSource = "",
                        ContentHeight = 0,
                        IsContentVisible = false,
                        TargetType = typeof(CheckForUpdatesViewModel),
                        ActiveTextColor = Color.White,
                    });
                }

                tempMenuList.Add(new MasterPageItemModel
                {
                    Title = "SETTINGS",
                    IconSource = "",
                    ContentHeight = 0,
                    IsContentVisible = false,
                    TargetType = typeof(MyProfileViewModel),
                    ActiveTextColor = Color.White,
                });

                tempMenuList.Add(new MasterPageItemModel
                {

                    Title = "ABOUT",
                    IconSource = "",
                    ContentHeight = 0,
                    IsContentVisible = false,
                    TargetType = typeof(TermsViewModel),
                    ActiveTextColor = Color.White,
                });

                tempMenuList.Add(new MasterPageItemModel
                {

                    Title = "LOGOUT",
                    IconSource = "",
                    ContentHeight = 0,
                    IsContentVisible = false,
                    ActiveTextColor = Color.White,
                });

                MenuList = new ObservableCollection<MasterPageItemModel>(tempMenuList);

            }

			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.CommonSelectedItemChangeColor),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}

		private async void TapOnBackImage()
		{
			try
			{
				Cache.MasterPage.IsPresented = false;
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Line = 251,
					Method = nameof(this.TapOnBackImage),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}


		private IList<MasterPageItemModel> items;
		public IList<MasterPageItemModel> Items
		{
			get
			{
				return items;
			}
			set
			{
				SetProperty(ref items, value);
			}
		}

        private bool isGestureEnabled = true;

        public bool IsGestureEnabled
        {
            get
            {
                return isGestureEnabled;
            }
            set
            {
                SetProperty(ref isGestureEnabled, value);
            }
        }

        private bool isPresentedMenu;

		public bool IsPresentedMenu
		{
			get
			{
				return isPresentedMenu;
			}
			set
			{
				SetProperty(ref isPresentedMenu, value);
			}
		}

		private MasterPageItemModel selectedmenuItem;
		public MasterPageItemModel SelectedMenuItem
		{
			get
			{
				return selectedmenuItem;
			}
			set
			{
				SetProperty(ref selectedmenuItem, value);
			}
		}

		public ICommand MenuSelectedItemCommand
		{
			get
			{
				return new Command<MasterPageItemModel>(async item =>
				{
                    try
                    {
                        if (item.TargetType == null && item.Title == "LOGOUT")
                        {
                            GeneralHelper.LogOut();
                            return;
                        }


                        if (item.ItemId != default(int))
                        {
                            Settings.MapSelectedCrag = item.ItemId;
                        }

                        var navigationParameters = new NavigationParameters();
                        navigationParameters.Add("selectedCragId", item.ItemId);
                        await navigationService.NavigateFromMenuAsync(item.TargetType, navigationParameters);
                        SelectedMenuItem = null;
                        IsPresentedMenu = false;
                    }
                    catch (Exception exception)
					{
						await exceptionManager.LogException(new ExceptionTable
						{
							Method = nameof(this.MenuSelectedItemCommand),
							Page = this.GetType().Name,
							StackTrace = exception.StackTrace,
							Exception = exception.Message,
							Data = Newtonsoft.Json.JsonConvert.SerializeObject(item)
						});
					}
				});
			}
		}

        async void GoHome() 
        {
            //if (!menuList.Any(mi => mi.ItemId != 0)) //no crags available
                //return;

            IsPresentedMenu = false;
            navigationService.NavigateFromMenuAsync(typeof(HomeViewModel));
        }
	}
}
