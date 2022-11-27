using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.Collections.Generic;
using Newtonsoft.Json;
using SloperMobile.Common.Constants;
using SloperMobile.ViewModel.MasterDetailViewModels;

namespace SloperMobile.Common.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        public static int UserID
        {
            get => AppSettings.GetValueOrDefault(nameof(UserID), default(int));
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(UserID), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(UserID));
                    AppSettings.AddOrUpdateValue(nameof(UserID), value);
                }
            }
        }

        public static string DisplayName
        {
            get => AppSettings.GetValueOrDefault(nameof(DisplayName), string.Empty);
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(DisplayName), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(DisplayName));
                    AppSettings.AddOrUpdateValue(nameof(DisplayName), value);
                }
            }
        }

        public static string AccessToken
        {
            get => AppSettings.GetValueOrDefault(nameof(AccessToken), string.Empty);
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(AccessToken), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(AccessToken));
                    AppSettings.AddOrUpdateValue(nameof(AccessToken), value);
                }
            }
        }

        public static string RenewalToken
        {
            get => AppSettings.GetValueOrDefault(nameof(RenewalToken), string.Empty);
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(RenewalToken), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(RenewalToken));
                    AppSettings.AddOrUpdateValue(nameof(RenewalToken), value);
                }
            }
        }

        public static int ActiveCrag
        {
            get
            {
	            try
	            {
                    return AppSettings.GetValueOrDefault(nameof(ActiveCrag), default(int));
	            }
	            catch (Exception e)
	            {
                    AppSettings.Remove(nameof(ActiveCrag));
                    AppSettings.AddOrUpdateValue(nameof(ActiveCrag), default(int));
                    return AppSettings.GetValueOrDefault(nameof(ActiveCrag), default(int));
				}
			}
            set
			{
				try
				{
                    AppSettings.AddOrUpdateValue(nameof(ActiveCrag), value);
                    MainMasterDetailViewModel.Instance.FillMenuItems();
				}
				catch (Exception e)
				{
                    AppSettings.Remove(nameof(ActiveCrag));
                    AppSettings.AddOrUpdateValue(nameof(ActiveCrag), value);
				}
			}
        }

        public static int ClimbingDays
        {
	        get
	        {
		        try
		        {
                    return AppSettings.GetValueOrDefault(nameof(ClimbingDays), 0);
		        }
		        catch (Exception e)
		        {
                    AppSettings.Remove(nameof(ClimbingDays));
                    AppSettings.AddOrUpdateValue(nameof(ClimbingDays), 0);
                    return AppSettings.GetValueOrDefault(nameof(ClimbingDays), 0);
		        }
	        }
	        set
			{
				try
				{
                    AppSettings.AddOrUpdateValue(nameof(ClimbingDays), value);
				}
				catch (Exception e)
				{
                    AppSettings.Remove(nameof(ClimbingDays));
                    AppSettings.AddOrUpdateValue(nameof(ClimbingDays), value);
				}
			}
        }

        public static bool IsMapInstructionInit
        {
            get
			{
				try
				{
                    return AppSettings.GetValueOrDefault(nameof(IsMapInstructionInit), true);
				}
				catch (Exception e)
				{
                    AppSettings.Remove(nameof(IsMapInstructionInit));
                    AppSettings.AddOrUpdateValue(nameof(IsMapInstructionInit), true);
                    return AppSettings.GetValueOrDefault(nameof(IsMapInstructionInit), true);
				}
			}
            set
			{
				try
				{
                    AppSettings.AddOrUpdateValue(nameof(IsMapInstructionInit), value);
				}
				catch (Exception e)
				{
                    AppSettings.Remove(nameof(IsMapInstructionInit));
                    AppSettings.AddOrUpdateValue(nameof(IsMapInstructionInit), value);
				}
			}
        }

		public static int MapSelectedCrag
		{
			get
			{
				try
				{
                    return AppSettings.GetValueOrDefault(nameof(MapSelectedCrag), default(int));
				}
				catch (Exception e)
				{
                    AppSettings.Remove(nameof(MapSelectedCrag));
                    AppSettings.AddOrUpdateValue(nameof(MapSelectedCrag), default(int));
                    return AppSettings.GetValueOrDefault(nameof(MapSelectedCrag), default(int));
				}
			}
			set
			{
				try
				{
                    AppSettings.AddOrUpdateValue(nameof(MapSelectedCrag), value);
				}
				catch (Exception e)
				{
                    AppSettings.Remove(nameof(MapSelectedCrag));
                    AppSettings.AddOrUpdateValue(nameof(MapSelectedCrag), value);
				}
			}
		}

        public static string Version
        {
            get => AppSettings.GetValueOrDefault(nameof(Version), string.Empty);
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(Version), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(Version));
                    AppSettings.AddOrUpdateValue(nameof(Version), value);
                }
            }
        }

        public static string FilterOptions
        {
            get => AppSettings.GetValueOrDefault(nameof(FilterOptions), string.Empty);
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(FilterOptions), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(FilterOptions));
                    AppSettings.AddOrUpdateValue(nameof(FilterOptions), value);
                }
            }
        }

        public static string UnitOfMeasure
        {
            get
            {
                return AppSettings.GetValueOrDefault(nameof(UnitOfMeasure), string.Empty);
            }
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(UnitOfMeasure), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(UnitOfMeasure));
                    AppSettings.AddOrUpdateValue(nameof(UnitOfMeasure), value);
                }
            }
        }

        public static bool FBLogIn
        {
            get
            {
                try
                {
                    return AppSettings.GetValueOrDefault(nameof(FBLogIn), false);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(FBLogIn));
                    AppSettings.AddOrUpdateValue(nameof(FBLogIn), false);
                    return AppSettings.GetValueOrDefault(nameof(FBLogIn), false);
                }
            }
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(FBLogIn), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(FBLogIn));
                    AppSettings.AddOrUpdateValue(nameof(FBLogIn), value);
                }
            }
        }

        public static bool GPLogIn
        {
            get
            {
                try
                {
                    return AppSettings.GetValueOrDefault(nameof(GPLogIn), false);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(GPLogIn));
                    AppSettings.AddOrUpdateValue(nameof(GPLogIn), false);
                    return AppSettings.GetValueOrDefault(nameof(GPLogIn), false);
                }
            }
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(GPLogIn), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(GPLogIn));
                    AppSettings.AddOrUpdateValue(nameof(GPLogIn), value);
                }
            }
        }

        //todo: remove
        public static bool AppPurchased
        {
            get
            {
                try
                {
                    return AppSettings.GetValueOrDefault(nameof(AppPurchased), false);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(AppPurchased));
                    AppSettings.AddOrUpdateValue(nameof(AppPurchased), false);
                    return AppSettings.GetValueOrDefault(nameof(AppPurchased), false);
                }
            }
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(AppPurchased), value);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(AppPurchased));
                    AppSettings.AddOrUpdateValue(nameof(AppPurchased), value);
                }
            }
        }

        public static int AvailableFreeCrags
        {
            get
            {
                try
                {
                    return AppSettings.GetValueOrDefault(nameof(AvailableFreeCrags), 0);
                }
                catch (Exception e)
                {
                    AppSettings.Remove(nameof(AvailableFreeCrags));
                    AppSettings.AddOrUpdateValue(nameof(AvailableFreeCrags), 0);
                    return AppSettings.GetValueOrDefault(nameof(AvailableFreeCrags), 0);
                }
            }
            set
            {
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(AvailableFreeCrags), value);
                }
                catch (Exception)
                {
                    AppSettings.Remove(nameof(AvailableFreeCrags));
                    AppSettings.AddOrUpdateValue(nameof(AvailableFreeCrags), value);
                }
            }
        }

        public static List<int> FreeCragIds
        {
            get
            {
                try
                {
                    var jsonObj = AppSettings.GetValueOrDefault(nameof(FreeCragIds), AppConstant.EmptyJsonArray);
                    return JsonConvert.DeserializeObject<List<int>>(jsonObj);
                }
                catch (Exception)
                {
                    AppSettings.Remove(nameof(FreeCragIds));
                    AppSettings.AddOrUpdateValue(nameof(FreeCragIds), AppConstant.EmptyJsonArray);
                    return FreeCragIds;
                }
            }
            set
            {
                var jsonObj = JsonConvert.SerializeObject(value ?? new List<int>());
                try
                {
                    AppSettings.AddOrUpdateValue(nameof(FreeCragIds), jsonObj);
                }
                catch (Exception)
                {
                    AppSettings.Remove(nameof(FreeCragIds));
                    AppSettings.AddOrUpdateValue(nameof(FreeCragIds), jsonObj);
                }
            }
        }
		
		public static string AppPackageName
		{
			get
			{
				try
				{
					return AppSettings.GetValueOrDefault(nameof(AppPackageName), string.Empty);
				}
				catch (Exception e)
				{
					AppSettings.Remove(nameof(AppPackageName));
					AppSettings.AddOrUpdateValue(nameof(AppPackageName), string.Empty);
					return AppSettings.GetValueOrDefault(nameof(AppPackageName), string.Empty);
				}
			}
			set
			{
				try
				{
					AppSettings.AddOrUpdateValue(nameof(AppPackageName), value);
				}
				catch (Exception e)
				{
					AppSettings.Remove(nameof(AppPackageName));
					AppSettings.AddOrUpdateValue(nameof(AppPackageName), value);
				}
			}
		}
	}
}
