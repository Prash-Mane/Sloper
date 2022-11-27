using Prism.Mvvm;
using Xamarin.Forms;

namespace SloperMobile.Model
{
	public class SubscriptionModel : BindableBase
	{
		private bool areGuidebooksVisible;

		public bool AreGuidebooksVisible
		{
			get => areGuidebooksVisible;
			set => SetProperty(ref areGuidebooksVisible, value);
		}

		private bool areCragsVisible;

		public bool AreCragsVisible
		{
			get => areCragsVisible;
			set => SetProperty(ref areCragsVisible, value);
		}

		private int guideBooksCount;

		public int GuideBooksCount
		{
			get => guideBooksCount;
			set => SetProperty(ref guideBooksCount, value);
		}

		private int cragsCount;

		public int CragsCount
		{
			get => cragsCount;
			set => SetProperty(ref cragsCount, value);
		}

		private int sectorsCount;

		public int SectorsCount
		{
			get => sectorsCount;
			set => SetProperty(ref sectorsCount, value);
		}

		private int routesCount;

		public int RoutesCount
		{
			get => routesCount;
			set => SetProperty(ref routesCount, value);
		}

		private string subscriptionItemName;

		public string SubscriptionItemName
		{
			get => subscriptionItemName;
			set => SetProperty(ref subscriptionItemName, value);
		}

		private string subscriptionItemPrice;

		public string SubscriptionItemPrice
		{
			get => subscriptionItemPrice;
			set => SetProperty(ref subscriptionItemPrice, value);
		}

		private string subscriptionItemTypeDescription;

		public string SubscriptionItemTypeDescription
		{
			get => subscriptionItemTypeDescription;
			set => SetProperty(ref subscriptionItemTypeDescription, value);
		}

		private string subscriptionItemType;

		public string SubscriptionItemType
		{
			get => subscriptionItemType;
			set => SetProperty(ref subscriptionItemType, value);
		}

		private string description;

		public string Description
		{
			get => description;
			set => SetProperty(ref description, value);
		}

		private string subscriptionItemTypeAuthors;

		public string SubscriptionItemTypeAuthors
		{
			get => subscriptionItemTypeAuthors;
			set => SetProperty(ref subscriptionItemTypeAuthors, value);
		}

		public bool IsTermsTextVisible => Device.RuntimePlatform == Device.iOS;
	}
}