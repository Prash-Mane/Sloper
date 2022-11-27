using System.Windows.Input;
using Xamarin.Forms;

namespace SloperMobile
{
    public partial class ProfileHeader : Grid
    {
        public static readonly BindableProperty HeaderTextProperty = BindableProperty.Create(
            nameof(HeaderText), 
            typeof(string), 
            typeof(ProfileHeader), 
            string.Empty);

        public static readonly BindableProperty HeaderColorProperty = BindableProperty.Create(
            nameof(HeaderColor),
            typeof(Color),
            typeof(ProfileHeader),
            Color.Black);

        public static readonly BindableProperty HeaderSizeProperty = BindableProperty.Create(
            nameof(HeaderSize),
            typeof(int),
            typeof(ProfileHeader),
            10);

        public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
            nameof(SelectedColor),
            typeof(Color),
            typeof(ProfileHeader),
            Color.Blue);

        public static readonly BindableProperty UnselectedColorProperty = BindableProperty.Create(
            nameof(UnselectedColor),
            typeof(Color),
            typeof(ProfileHeader),
            Color.Silver);

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
           nameof(IsSelected),
           typeof(bool),
           typeof(ProfileHeader),
           false,
           propertyChanged: IsSelectedPropertyChanged);

        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create(
            nameof(SelectedCommand),
            typeof(ICommand),
            typeof(ProfileHeader));

        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public ProfileHeader()
        {
            InitializeComponent();

            DefaultSelectedCommand = new Command(OnDefaultSelectedExecute);

            var gesture = new TapGestureRecognizer
            {
                Command = DefaultSelectedCommand
            };

            GestureRecognizers.Add(gesture);
        }

        private ICommand DefaultSelectedCommand { get; set; }

        public int HeaderId { get; set; }

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        public Color HeaderColor
        {
            get => (Color)GetValue(HeaderColorProperty);
            set => SetValue(HeaderColorProperty, value);
        }

        public int HeaderSize
        {
            get => (int)GetValue(HeaderSizeProperty);
            set => SetValue(HeaderSizeProperty, value);
        }

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public Color UnselectedColor
        {
            get => (Color)GetValue(UnselectedColorProperty);
            set => SetValue(UnselectedColorProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        static void IsSelectedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (ProfileHeader)bindable;

            if (view != null)
                view.Selector.BackgroundColor = view.IsSelected ? view.SelectedColor : view.UnselectedColor;
        }

        private void OnDefaultSelectedExecute()
        {
            if (SelectedCommand?.CanExecute(HeaderId) ?? false)
            {
                SelectedCommand.Execute(HeaderId);
            }
        }
    }
}
