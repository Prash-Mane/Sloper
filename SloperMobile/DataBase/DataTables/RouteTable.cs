using System;
using System.Net;
using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_ROUTE")]
	public class RouteTable
	{
        #region Decoded Properties

        private string routeName;
        public string route_name
        {
            get => WebUtility.HtmlDecode(routeName);
            set => routeName = value;
        }

        private string routeInfo;
        public string route_info
        {
            get => WebUtility.HtmlDecode(routeInfo);
            set => routeInfo = value;
        }

        private string equipperName;
        public string equipper_name
        {
            get => WebUtility.HtmlDecode(equipperName);
            set => equipperName = value;
        }

        private string firstAscentName;
        public string first_ascent_name
        {
            get => WebUtility.HtmlDecode(firstAscentName);
            set => firstAscentName = value;
        }

        private string routeSafetyNotice;
        public string route_safety_notice
        {
            get => WebUtility.HtmlDecode(routeSafetyNotice);
            set => routeSafetyNotice = value;
        }

        #endregion

		[PrimaryKey, Indexed]
		public int route_id { get; set; }
		public int area_id { get; set; }
		public int crag_id { get; set; }
		public int sector_id { get; set; }
		public bool is_enabled { get; set; }
		public DateTime? equipper_date { get; set; }
		//public string equipper_name { get; set; }
		public DateTime? first_ascent_date { get; set; }
		//public string first_ascent_name { get; set; }
		public int grade_bucket_id { get; set; }
		public string grade_name { get; set; }
		public int grade_name_sort_order { get; set; }
		public int grade_type_id { get; set; }
		//public string rating_name { get; set; }
		public string route_type { get; set; }
		public int route_type_id { get; set; }
		public string tech_grade { get; set; }
		public string tech_grade_sort_order { get; set; }
		public string start_x { get; set; }
		public string start_y { get; set; }
		public string route_length { get; set; }
		//public string route_name { get; set; }
		//public string route_info { get; set; }
		public DateTime date_modified { get; set; }
		public int sort_order { get; set; }
		public string graded_list_order { get; set; }
		public string version_number { get; set; }
		public string angles { get; set; }
		public string angles_top_1 { get; set; }
		public string hold_type_top_1 { get; set; }
		public string route_style_top_1	{ get; set; }
		//public string angles_top_2 { get; set; }
		//public string hold_type_top_2 { get; set; }
		//public string route_style_top_2 { get; set; }
		public float? rating { get; set; }
		public DateTime date_created { get; set; }
		public DateTime route_set_date { get; set; }
		public DateTime date_last_climbed { get; set; }
		public int? number_of_bolts { get; set; }
        //public string route_safety_notice { get; set; }
        public int? route_image_id { get; set; }
		public string author_rating { get; set; }
		public string safety_rating_type { get; set; }
		public string safety_rating { get; set; }
		public string special_gear { get; set; }
		
	}
}
