using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.Navigation;

namespace SloperMobile.Views.UserPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserPasscodePage : ContentPage, IDestructible
    {
        //public static BindableProperty PinProperty = BindableProperty.Create("Pin", typeof(string), typeof(UserPasscodePage), defaultBindingMode: BindingMode.OneWayToSource);
        //public string Pin
        //{
        //    get
        //    {
        //        return (string)GetValue(PinProperty);
        //    }
        //    set
        //    {
        //        SetValue(PinProperty, value);
        //    }
        //}

        public UserPasscodePage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            //Pin = string.Empty;
            Pin1.TextChanged += Pin1_TextChanged;
            Pin2.TextChanged += Pin2_TextChanged;
            Pin3.TextChanged += Pin3_TextChanged;
            Pin4.TextChanged += Pin4_TextChanged;
            Pin1.Focus();
        }

        private void Pin4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Pin4.Text.Length > 0)
                Pin4.Unfocus();
            else
                Pin1.Focus();
            //Pin = Pin1.Text + Pin2.Text + Pin3.Text + Pin4.Text;
        }

        private void Pin3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Pin3.Text.Length > 0)
                Pin4.Focus();
            else
                Pin2.Focus();
            //Pin = Pin1.Text + Pin2.Text + Pin3.Text + Pin4.Text;
        }

        private void Pin2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Pin2.Text.Length > 0)
                Pin3.Focus();
            else
                Pin1.Focus();
            //Pin = Pin1.Text + Pin2.Text + Pin3.Text + Pin4.Text;
        }

        private void Pin1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Pin1.Text.Length > 0)
                Pin2.Focus();
            //Pin = Pin1.Text + Pin2.Text + Pin3.Text + Pin4.Text;
        }

        public void Destroy()
        {
            Pin1.TextChanged -= Pin1_TextChanged;
            Pin2.TextChanged -= Pin2_TextChanged;
            Pin3.TextChanged -= Pin3_TextChanged;
            Pin4.TextChanged -= Pin4_TextChanged;
        }
    }
}