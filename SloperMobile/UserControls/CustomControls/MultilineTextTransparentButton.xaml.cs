using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SloperMobile.UserControls.CustomControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MultilineTextTransparentButton : ContentView
	{
		public static BindableProperty FirstLineTextProperty =
			BindableProperty.Create(nameof(FirstLineText), typeof(string), typeof(MultilineTextTransparentButton), null, propertyChanged: OnFirstLineTextChanged);

		public static BindableProperty SecondLineTextProperty =
			BindableProperty.Create(nameof(SecondLineText), typeof(string), typeof(MultilineTextTransparentButton), null, propertyChanged: OnSecondLineTextChanged);

		public static BindableProperty TapCommandProperty =
			BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(MultilineTextTransparentButton), null);

		public static BindableProperty TapCommandParameterProperty =
			BindableProperty.Create(nameof(TapCommandParameter), typeof(int?), typeof(MultilineTextTransparentButton), null);

		public MultilineTextTransparentButton()
		{
			InitializeComponent();
			var gestureRecognizer = new TapGestureRecognizer();
			gestureRecognizer.Tapped += (s, e) => {
				if (TapCommand != null && TapCommand.CanExecute(TapCommandParameter))
				{
					TapCommand.Execute(TapCommandParameter);
				}
			};

			this.GestureRecognizers.Add(gestureRecognizer);
		}

		private static void OnSecondLineTextChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as MultilineTextTransparentButton).secondLineText.IsVisible = true;
			(bindable as MultilineTextTransparentButton).secondLineText.Text = newValue?.ToString();
		}

		private static void OnFirstLineTextChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as MultilineTextTransparentButton).firstLineText.Text = newValue.ToString();
		}

		public string FirstLineText
		{
			get { return (string)GetValue(FirstLineTextProperty); }
			set { SetValue(FirstLineTextProperty, value); }
		}

		public string SecondLineText
		{
			get { return (string)GetValue(SecondLineTextProperty); }
			set { SetValue(SecondLineTextProperty, value); }
		}

		public ICommand TapCommand
		{
			get { return (ICommand)GetValue(TapCommandProperty); }
			set { SetValue(TapCommandProperty, value); }
		}

		public int? TapCommandParameter
		{
			get { return (int?)GetValue(TapCommandParameterProperty); }
			set { SetValue(TapCommandParameterProperty, value); }

		}
	}
}