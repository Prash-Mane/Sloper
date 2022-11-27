using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_GRADE")]
	public class GradeTable
	{
		[PrimaryKey, AutoIncrement]
		public int? ID { get; set; }
		public int area_id { get; set; }
		public int crag_id { get; set; }
		public int sector_id { get; set; }
		public int grade_bucket_id { get; set; }
		public int grade_bucket_id_count { get; set; }
	}
}
