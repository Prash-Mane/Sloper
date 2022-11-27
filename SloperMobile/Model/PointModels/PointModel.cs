using System;
using System.Net;

namespace SloperMobile.Model.PointModels
{
	public class PointModel
	{
		private string routeName;

		public string route_name
		{
            get => WebUtility.HtmlDecode(routeName);
            set => routeName = value;
        }

		public string tech_grade { get; set; }
		public int? points { get; set; }

		private DateTime dateclimbed;
		public DateTime date_climbed
		{
			get { return dateclimbed; }
			set { dateclimbed = value; }
		}
	}
}
