using SQLite;
using System.Net;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_AREA")]
	public class AreaTable
	{
        private string areaName;
        public string area_name
        {
            get => WebUtility.HtmlDecode(areaName);
            set => areaName = value;
        }

		[PrimaryKey, Indexed]
		public int area_id { get; set; }
		public bool is_enabled { get; set; }
		public string area_city { get; set; }
		public float area_latitude { get; set; }
		public float area_intitude { get; set; }
		public int area_map_zoom { get; set; }
		//public string area_name { get; set; }
		public string detailed_info { get; set; }
		public string area_crag_map_image_name { get; set; }
		public string area_static_map { get; set; }
		public string area_crag_map_name { get; set; }
		public string general_info { get; set; }
		public string area_crag_map_iframe_src { get; set; }
		public int version_number { get; set; }
		public int sort_order { get; set; }
	}
}
