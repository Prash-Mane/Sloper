using System.Windows.Input;
using Prism.Mvvm;
using Xamarin.Forms;

namespace SloperMobile.Model
{
	public class RepeaterItem : BindableBase
	{
		private ImageSource imageSource;
		public ImageSource ImageSourceItems
		{
			get
			{
				return imageSource;
			}
			set
			{
				SetProperty(ref imageSource, value);
			}
		}

		private string labelText;
		public string LabelText
		{
			get
			{
				return labelText;
			}
			set
			{
				SetProperty(ref labelText, value);
			}
		} 
       
        private string labelInfoText;
		public string LabelInfoText
		{
			get
			{
				return labelInfoText;
			}
			set
			{
				SetProperty(ref labelInfoText, value);
			}
		}
		
		private ICommand commandToRun;
		public ICommand CommandToRun
		{
			get
			{
				return commandToRun;
			}
			set
			{
				SetProperty(ref commandToRun, value);
			}
		}
	}
}
