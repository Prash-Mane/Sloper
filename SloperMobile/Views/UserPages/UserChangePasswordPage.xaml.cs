using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.UserPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UserChangePasswordPage : ContentPage
	{
		public UserChangePasswordPage()
		{
			InitializeComponent ();
            NavigationPage.SetHasNavigationBar(this, false);
        }
	}
}