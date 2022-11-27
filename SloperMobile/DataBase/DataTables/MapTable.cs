using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("MapTable")]
	public class MapTable
	{
		[PrimaryKey, Indexed]
        public int map_id { get; set; }
        public int crag_id { get; set; }
        public string map_name { get; set; }
        public string imagedata { get; set; }
        public double height { get; set; }
        public double width { get; set; }
		public int sort_order { get; set; }
        public bool is_enabled { get; set; }
	}
}
