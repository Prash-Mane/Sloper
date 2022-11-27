using Prism.Mvvm;

namespace SloperMobile.Model
{
    public class Bolts : BindableBase
    {
        private string boltColor = "#333333";
		private string backgroundItemColor = "#FFFFFF";
		public int Bolt { get; set; }
		public bool IsSelected { get; set; }
        public string Color
        {
            get { return boltColor; }
            set { SetProperty(ref boltColor, value); }
        }

		public string BackgroundItemColor
		{
			get { return backgroundItemColor; }
			set { SetProperty(ref backgroundItemColor, value); }
		}
	}
}
