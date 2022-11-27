using Prism.Navigation;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Interfaces;
using SloperMobile.CustomControls;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using SloperMobile.Model;
using SloperMobile.Model.PointModels;
using SloperMobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XLabs.Platform.Device;
using Xamarin.Forms.PlatformConfiguration;

namespace SloperMobile.Views.CragPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CragSectorsPage : ContentPage
    {
		bool isLoaded;
		public CragSectorsPage()
        {
            InitializeComponent();
            App.AreSectorPages = true;
            NavigationPage.SetHasNavigationBar(this, false);
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (isLoaded)
				return;
			isLoaded = true;

			if (BindingContext is BaseViewModel vm)
			{
				vm.IsMenuVisible = Navigation.NavigationStack.Count == 1;
				vm.IsBackButtonVisible = !vm.IsMenuVisible;
			}

		}
    }
}
