using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.Views.MyPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyPreferencesPage : ContentPage
    {
        public MyPreferencesPage()
        {
            InitializeComponent();
        }

        private void OnDateOfBirthFocused(object sender, FocusEventArgs e)
        {
            DateOfBirthEntry.IsVisible = false;
            DOBDatePicker.IsVisible = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                DateOfBirthEntry.Unfocus();
                DOBDatePicker.Focus();
                DateOfBirthEntry.Keyboard = null;
            });
        }

        private void DoneButtonHide(object sender, FocusEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                btnDone.IsVisible = false;
            });
        }
        private void DoneButtonShow(object sender, FocusEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                btnDone.IsVisible = true;
            });
        }
    }
}