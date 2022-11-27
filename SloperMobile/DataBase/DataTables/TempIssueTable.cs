using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_TEMP_ISSUE")]
	public class TempIssueTable
	{
		[PrimaryKey, AutoIncrement]
		public int? id { get; set; }
		public int route_id { get; set; }
		public int issue_category_id { get; set; }
		public int issue_type_id { get; set; }
		public int issue_type_detail_id { get; set; }
		public string bolt_numbers { get; set; }
		public string image { get; set; }
		public string comments { get; set; }
	}
}
