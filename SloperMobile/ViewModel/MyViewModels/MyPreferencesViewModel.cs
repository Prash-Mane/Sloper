using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Connectivity.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Navigation;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using SloperMobile.Common.Extentions;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.UserControls;
using SloperMobile.Views.FlyoutPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile.ViewModel.MyViewModels
{
	public class MyPreferencesViewModel : BaseViewModel
	{
		private readonly IRepository<UserInfoTable> userInfoRepository;
		//private readonly IConnectivity connectivity;
		private readonly IUserDialogs userDialogs;
		private string _profileImage = String.Empty;
		private IMedia _mediaPicker;
		byte[] imageBytes = null;
		private UserInfoTable userInfoObj;

		public MyPreferencesViewModel(INavigationService navigationService,
									IRepository<UserInfoTable> userInfoRepository,
									//IConnectivity connectivity,
									IUserDialogs userDialogs,
									IExceptionSynchronizationManager exceptionManager,
									IHttpHelper httpHelper)
            : base(navigationService, exceptionManager, httpHelper)
		{
			this.userInfoRepository = userInfoRepository;
			//this.connectivity = connectivity;
			this.userDialogs = userDialogs;
			FirstYearClimb = new List<int>();
			IsDateOfBirthEntryVisible = true;
			IsDateOfBirthDatePickerVisible = false;
			DOB = DateTime.Today;
			UserInfoTable = new UserInfoTable();
			BindPickers();
			OnPagePreparing();
		}

		#region Properties
		private bool _isDateOfBirthEntryVisible;
		public bool IsDateOfBirthEntryVisible
		{
			get { return _isDateOfBirthEntryVisible; }
			set { SetProperty(ref _isDateOfBirthEntryVisible, value); }
		}

		private bool _isDateOfBirthDatePickerVisible;
		public bool IsDateOfBirthDatePickerVisible
		{
			get { return _isDateOfBirthDatePickerVisible; }
			set { SetProperty(ref _isDateOfBirthDatePickerVisible, value); }
		}

		private List<string> _genders;
		public List<string> Genders
		{
			get { return _genders; }
			set { SetProperty(ref _genders, value); }
		}

		private string _selectedGender;
		public string SelectedGender
		{
			get { return _selectedGender; }
			set { SetProperty(ref _selectedGender, value); }
		}

		private List<string> _unitOfMeasure;
		public List<string> UnitOfMeasure
		{
			get { return _unitOfMeasure; }
			set { SetProperty(ref _unitOfMeasure, value); }
		}

		private string _selectedUnitOfMeasure;
		public string SelectedUnitOfMeasure
		{
			get { return _selectedUnitOfMeasure; }
			set { SetProperty(ref _selectedUnitOfMeasure, value); }
		}

		private List<int> _firstYearClimb;
		public List<int> FirstYearClimb
		{
			get { return _firstYearClimb; }
			set { SetProperty(ref _firstYearClimb, value); }
		}

		private int _selectedFirstYearClimb;
		public int SelectedFirstYearClimb
		{
			get { return _selectedFirstYearClimb; }
			set { SetProperty(ref _selectedFirstYearClimb, value); }
		}

		private ImageSource _profilePicture;
		public ImageSource ProfilePicture
		{
			get { return _profilePicture; }
			set { SetProperty(ref _profilePicture, value); }
		}

		private string _firstName;
		public string FirstName
		{
			get { return _firstName; }
			set { SetProperty(ref _firstName, value?.Trim()); }
		}

		private string _lastName;
		public string LastName
		{
			get { return _lastName; }
			set { SetProperty(ref _lastName, value?.Trim()); }
		}

		private DateTime _dOB;
		public DateTime DOB
		{
			get { return _dOB; }
			set { SetProperty(ref _dOB, value); }
		}

		private decimal? _height;
		public decimal? Height
		{
			get { return _height; }
			set { SetProperty(ref _height, value); }
		}

		private decimal? _weight;
		public decimal? Weight
		{
			get { return _weight; }
			set { SetProperty(ref _weight, value); }
		}

		private UserInfoTable _userInfoTable;
		public UserInfoTable UserInfoTable
		{
			get { return _userInfoTable; }
			set { SetProperty(ref _userInfoTable, value); }
		}

		private List<string> _heightUnits;
		public List<string> HeightUnits
		{
			get { return _heightUnits; }
			set { SetProperty(ref _heightUnits, value); }
		}

		private string _selectedHeightUnits;
		public string SelectedHeightUnits
		{
			get { return _selectedHeightUnits; }
			set { SetProperty(ref _selectedHeightUnits, value); }
		}

		private List<string> _weightUnits;
		public List<string> WeightUnits
		{
			get { return _weightUnits; }
			set { SetProperty(ref _weightUnits, value); }
		}

        private List<string> _temperatureUnits;
        public List<string> TemperatureUnits
        {
            get { return _temperatureUnits; }
            set { SetProperty(ref _temperatureUnits, value); }
        }

        private string _selectedTemperatureUnits;
        public string SelectedTemperatureUnits
        {
            get { return _selectedTemperatureUnits; }
            set { SetProperty(ref _selectedTemperatureUnits, value); }
        }

        private string _selectedWeightUnits;
		public string SelectedWeightUnits
		{
			get { return _selectedWeightUnits; }
			set { SetProperty(ref _selectedWeightUnits, value); }
		}

		private List<string> _privacyClimbingCommunity;
		public List<string> PrivacyClimbingCommunity
		{
			get { return _privacyClimbingCommunity; }
			set { SetProperty(ref _privacyClimbingCommunity, value); }
		}

		private string _selectedPrivacyClimbingCommunity;
		private string filePath;

		public string SelectedPrivacyClimbingCommunity
		{
			get { return _selectedPrivacyClimbingCommunity; }
			set { SetProperty(ref _selectedPrivacyClimbingCommunity, value); }
		}
		private string _email;
		public string Email
		{
			get { return _email; }
			set { SetProperty(ref _email, value); }
		}
		#endregion

		#region Delegate Commands
		public ICommand OnEditPictureCommand
		{
			get
			{
				return new Command(OnEditPictureClick);
			}
		}
		public ICommand UpdateProfileCommand
		{
			get
			{
				return new Command(OnUpdateProfileClick);
			}
		}
		#endregion

		#region Methods
		private async void BindPickers()
		{
			try
			{
				List<string> genderList = new List<string> { "Male", "Female" };
				Genders = genderList;

				List<string> measureList = new List<string> { "ft", "m" };
				UnitOfMeasure = measureList;

				List<string> heightUnits = new List<string> { "in", "cm" };
				HeightUnits = heightUnits;

				List<string> weightUnits = new List<string> { "lbs", "kg" };
				WeightUnits = weightUnits;

                List<string> temperatureUnits = new List<string> { "Fahrenheit", "Celsius" };
                TemperatureUnits = temperatureUnits;

                List<string> privacyClimbingCommunity = new List<string> { "Yes", "No" };
				PrivacyClimbingCommunity = privacyClimbingCommunity;
				SelectedPrivacyClimbingCommunity = privacyClimbingCommunity.Find(x => x.Contains("Yes"));

				for (int min = DateTime.Now.Year; min >= 1900; min--)
				{
					FirstYearClimb.Add(min);
				}

				IsDateOfBirthEntryVisible = false;
				IsDateOfBirthDatePickerVisible = true;
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.BindPickers),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}
		private async void OnPagePreparing()
		{
			try
			{
				//if (!connectivity.IsConnected)
				//{
					//await navigationService.NavigateAsync<NetworkErrorViewModel>();
					//await exceptionManager.LogException(new ExceptionTable
					//{
					//	Method = nameof(this.OnPagePreparing),
					//	Page = this.GetType().Name,
					//	StackTrace = JsonConvert.SerializeObject(MasterNavigationPage.Instance?.Navigation.NavigationStack),
					//	Exception = "Network error page"
					//});
				//	return;
				//}
				userDialogs.ShowLoading("Loading...");

				//IsShowFooter = GeneralHelper.IsCragsDownloaded && IsMenuVisible;

				string app_uom = AppSetting.APP_UOM.ToLower();

				//var httpClinetHelper = App.HttpClient;
				//httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_GetUserInfo, Settings.AccessToken);
				//var response = await httpClinetHelper.Get<EditProfileModel>(null);
				var response = await userInfoRepository.GetAsync(Settings.UserID);
				if (response != null)
				{
					try
					{
						Settings.UserID = response.UserID;
                        FirstName = response.FirstName;
						LastName = response.LastName;
						Email = response.Email;
						Height = response.Height;
					}
					catch (Exception ex)
					{
						await exceptionManager.LogException(new ExceptionTable
						{
							Method = nameof(this.OnPagePreparing),
							Page = this.GetType().Name,
							StackTrace = ex.StackTrace,
							Exception = ex.Message,
							Data = $"dbData = {JsonConvert.SerializeObject(response)}"
						});
					}

					// figure out height UOM
					if (!string.IsNullOrWhiteSpace(response.height_uom))
						SelectedHeightUnits = response.height_uom;
					else if (app_uom == "imperial")
						SelectedHeightUnits = "in";
					else
						SelectedHeightUnits = "cm";

					// if we are displaying inches convert to imperial
					if (SelectedHeightUnits == "in" && Height.HasValue)
						Height = Math.Round(Height.Value * 0.393701m);

					Weight = response.Weight;

					// figure out weight UOM
					if (!string.IsNullOrWhiteSpace(response.weight_uom))
						SelectedWeightUnits = response.weight_uom;
					else if (app_uom == "imperial")
						SelectedWeightUnits = "lbs";
					else
						SelectedWeightUnits = "kg";

					// if we are displaying lbls, convert to imperial
					if (SelectedWeightUnits == "lbs" && Weight.HasValue)
						Weight = Math.Round(Weight.Value * 2.20462m);

					if (response.FirstYearClimb.HasValue)
						SelectedFirstYearClimb = response.FirstYearClimb.Value;
					if (!string.IsNullOrWhiteSpace(response.Gender))
						SelectedGender = response.Gender;
					if (response.DOB != null)
					{
						DOB = Convert.ToDateTime(response.DOB);
						IsDateOfBirthEntryVisible = false;
						IsDateOfBirthDatePickerVisible = true;
					}
					else
					{
						IsDateOfBirthEntryVisible = true;
						IsDateOfBirthDatePickerVisible = false;
					}

					if (!string.IsNullOrWhiteSpace(response.UnitOfMeasure))
						SelectedUnitOfMeasure = response.UnitOfMeasure;
					else if (app_uom == "imperial")
						SelectedUnitOfMeasure = "ft";
					else
						SelectedUnitOfMeasure = "m";

                    var temperatureUom = !string.IsNullOrWhiteSpace(response.temperature_uom) ? response.temperature_uom : app_uom;
                    SelectedTemperatureUnits = temperatureUom == "imperial" ? "Fahrenheit" : "Celsius";

                    if (!string.IsNullOrWhiteSpace(response.ProfilePicture))
					{
						try
						{
							imageBytes = Convert.FromBase64String(response.ProfilePicture);
						}
						catch (Exception)
						{
							imageBytes = Convert.FromBase64String(response.ProfilePicture?.Split(',')[1]);
						}
						ProfilePicture = ImageSource.FromStream(() => new MemoryStream(imageBytes));
						Cache.profileImage = response.ProfilePicture;
					}
					else
					{
						ProfilePicture = "icon_profile_large";
						Cache.profileImage = string.Empty;
					}

					SelectedPrivacyClimbingCommunity = (response.PrivacyClimbingCommunity ?? false) ? "Yes" : "No";
				}
				userDialogs.HideLoading();
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnPagePreparing),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"userId = {Settings.UserID}"
				});

				userDialogs.HideLoading();
			}
		}

		private async void OnEditPictureClick()
		{
			var action = await Application.Current.MainPage.DisplayActionSheet("Set Profile Picture", "Cancel", null, "Select Photo", "Take Photo");
			if (action == "Select Photo")
				await SelectPicture();
			else if (action == "Take Photo")
				await TakePicture();
			else if (action == "Cancel")
				return;
		}

		private async void OnUpdateProfileClick()
		{
			try
			{
				//if (!connectivity.IsConnected)
				//{
				//	//await navigationService.NavigateAsync<NetworkErrorViewModel>();
				//	await exceptionManager.LogException(new ExceptionTable
				//	{
				//		Method = nameof(this.OnUpdateProfileClick),
				//		Page = this.GetType().Name,
				//		StackTrace = JsonConvert.SerializeObject(MasterNavigationPage.Instance?.Navigation.NavigationStack),
				//		Exception = "Network error page"
				//	});

				//	return;
				//}

				userDialogs.ShowLoading("Saving...");
				var isValidate = await IsProfileValidation();
				if (!isValidate)
				{
					userDialogs.HideLoading();
					return;
				}

				//httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_UpdateUserInfo, Settings.AccessToken);

				UserInfoTable = await userInfoRepository.GetAsync(Convert.ToInt32(Settings.UserID));
				//UserInfoTable.UserID = Convert.ToInt32(Settings.UserIDSettings);
				UserInfoTable.DisplayName = $"{FirstName.Trim()} {LastName.Trim()}";
				UserInfoTable.FirstName = FirstName.Trim();
				UserInfoTable.LastName = LastName.Trim();
				//UserInfoTable.Email = Email;

				var imageResizer = DependencyService.Get<IImageResizer>();
				if (!string.IsNullOrWhiteSpace(_profileImage))
				{
					var imgBytes = Convert.FromBase64String(_profileImage);
					imgBytes = imageResizer.ResizeImage(imgBytes, 1024, 1024);
					UserInfoTable.ProfilePicture = Convert.ToBase64String(imgBytes);
				}
				else if (imageBytes != null)
				{
					imageBytes = imageResizer.ResizeImage(imageBytes, 1024, 1024);
					UserInfoTable.ProfilePicture = Convert.ToBase64String(imageBytes);
				}
				else
					UserInfoTable.ProfilePicture = (!string.IsNullOrWhiteSpace(Cache.profileImage)) ? Cache.profileImage.Split(';')[1].Replace("base64,", "") : Cache.profileImage;

				UserInfoTable.Gender = SelectedGender;

				if (DOB.Date != DateTime.Today)
					UserInfoTable.DOB = DOB;
				else
					UserInfoTable.DOB = null;

				Settings.UnitOfMeasure = UserInfoTable.UnitOfMeasure = SelectedUnitOfMeasure;
				if (!string.IsNullOrWhiteSpace(SelectedHeightUnits) && SelectedHeightUnits.Equals("in"))
					UserInfoTable.Height = Height / Convert.ToDecimal(0.393701, CultureInfo.InvariantCulture);
				else
					UserInfoTable.Height = Height;

				if (!string.IsNullOrWhiteSpace(SelectedWeightUnits) && SelectedWeightUnits.Equals("lbs"))
					UserInfoTable.Weight = Weight / Convert.ToDecimal(2.20462, CultureInfo.InvariantCulture);
				else
					UserInfoTable.Weight = Weight;

				UserInfoTable.FirstYearClimb = SelectedFirstYearClimb;
				UserInfoTable.height_uom = SelectedHeightUnits;
				UserInfoTable.weight_uom = SelectedWeightUnits;

                var temperatureUom = _selectedTemperatureUnits == "Fahrenheit" ? Metrics.imperial : Metrics.metric;
                UserInfoTable.temperature_uom = temperatureUom.ToString().ToLower();

                UserInfoTable.PrivacyClimbingCommunity = SelectedPrivacyClimbingCommunity == "Yes";

                var response = await httpHelper.PostAsync<UserInfoTable>(ApiUrls.Url_SloperUser_UpdateUserInfo, UserInfoTable);
                if (response.ValidateResponse())
				{
					Settings.UserID = UserInfoTable.UserID;
					Settings.DisplayName = UserInfoTable.DisplayName != Settings.DisplayName ? UserInfoTable.DisplayName : Settings.DisplayName;
					await userInfoRepository.UpdateAsync(UserInfoTable);
                    var Params = new NavigationParameters();
					Params.Add(NavigationParametersConstants.MemberProfileId, Settings.UserID);
					Params.Add(NavigationParametersConstants.MemberProfileName, Settings.DisplayName);
					await navigationService.GoBackAsync(Params);
				}
				userDialogs.HideLoading();
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.OnUpdateProfileClick),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
					Data = $"UserInfoTable = {JsonConvert.SerializeObject(UserInfoTable)}"
				});

				userDialogs.HideLoading();
			}
		}

		private async Task<bool> IsProfileValidation()
		{
			if (string.IsNullOrWhiteSpace(FirstName))
			{
				userDialogs.HideLoading();
				await userDialogs.AlertAsync("First Name required, try again.", "Profile Error", "OK");
				return false;
			}
			else if (string.IsNullOrWhiteSpace(LastName))
			{
				userDialogs.HideLoading();
				await userDialogs.AlertAsync("Last Name required, try again.", "Profile Error", "OK");
				return false;
			}
			return true;
		}

		private async Task<OperationResult<UserInfoTable>> HttpGetUserInfo()
		{
			//httpClinetHelper.ChangeTokens(ApiUrls.Url_SloperUser_GetUserInfo, Settings.AccessToken);
            var response = await httpHelper.GetAsync<UserInfoTable>(null);
			return response;
		}
		#endregion

		#region Photos
		private async void Setup()
		{
			if (_mediaPicker != null)
			{
				return;
			}

			await CrossMedia.Current.Initialize();
			_mediaPicker = CrossMedia.Current;
		}

		private async Task SelectPicture()
		{
			Setup();
			//ProfilePicture = null;
			try
			{
				var mediaFile = await this._mediaPicker.PickPhotoAsync();
				if (mediaFile == null)
				{
					return;
				}

				//ProfilePicture = ImageSource.FromStream(mediaFile.GetStream);
				var memoryStream = new MemoryStream();
				await mediaFile.GetStream().CopyToAsync(memoryStream);
				var imageAsByte = memoryStream.ToArray();

				//	            filePath = await DependencyService.Get<ISavePicture>().SavePictureToDisk("ProfileImage", imageAsByte);
				_profileImage = Convert.ToBase64String(imageAsByte);

				userDialogs.ShowLoading("Loading...");
				await Application.Current.MainPage.Navigation.PushModalAsync(new CropView(imageAsByte, Refresh));
				userDialogs.HideLoading();
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.SelectPicture),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}

		private async Task TakePicture()
		{
			Setup();
			//ProfilePicture = null;
			try
			{
				var mediaFile = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
				{
					DefaultCamera = CameraDevice.Front
				});

				if (mediaFile == null)
				{
					return;
				}
				//ProfilePicture = ImageSource.FromStream(mediaFile.GetStream);

				var memoryStream = new MemoryStream();
				await mediaFile.GetStream().CopyToAsync(memoryStream);
				byte[] imageAsByte = memoryStream.ToArray();
				_profileImage = Convert.ToBase64String(imageAsByte);
				//	            filePath = await DependencyService.Get<ISavePicture>().SavePictureToDisk("ProfileImage", imageAsByte);
				await Application.Current.MainPage.Navigation.PushModalAsync(new CropView(imageAsByte, Refresh));
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.TakePicture),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}

		private async void Refresh()
		{
			try
			{
				if (App.CroppedImage != null)
				{
					Stream stream = new MemoryStream(App.CroppedImage);
					ProfilePicture = ImageSource.FromStream(() => stream);
					_profileImage = Convert.ToBase64String(App.CroppedImage);
				}
			}
			catch (Exception ex)
			{
				await exceptionManager.LogException(new ExceptionTable
				{
					Method = nameof(this.Refresh),
					Page = this.GetType().Name,
					StackTrace = ex.StackTrace,
					Exception = ex.Message,
				});
			}
		}
		#endregion
	}
}
