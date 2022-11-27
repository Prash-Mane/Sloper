using System;
using System.Net;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace SloperMobile.Model
{
    public class TickListModel : BindableBase
	{
        //public string route_name { get; set; }
        public string grade_name { get; set; }
        public int Grade_Id { get; set; }
        public int RouteID { get; set; }
        public int sector_id { get; set; }

	    [JsonProperty("crag_id")]
	    public int CragId { get; set; }

		private DateTime date_Created;

        public DateTime Date_Created
        {
            get { return date_Created; }
            set
            {
                date_Created = value;
                DateCreated = value.ToString("MM/dd/yy");
            }
        }

        private string dateCreated;

        public string DateCreated
        {
            get { return dateCreated; }
            set
            {
                dateCreated = value; RaisePropertyChanged();

            }
        }

		private bool isDeleteVisible;
		public bool IsDeleteVisible
		{
			get
			{
				return isDeleteVisible;
			}
			set
			{
				SetProperty(ref isDeleteVisible, value);
			}
		}

        #region Decoded Properties

        private string routeName;
        public string route_name
        {
            get => WebUtility.HtmlDecode(routeName);
            set => routeName = value;
        }

        #endregion
	}
}
