using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_TOPO")]
	public class TopoTable
	{
		[PrimaryKey, AutoIncrement]
		public int? topo_id { get; set; }
		public int sector_id { get; set; }
		public string topo_json { get; set; }
		public string upload_date { get; set; }
		public int sort_order { get; set; }
	}
}
