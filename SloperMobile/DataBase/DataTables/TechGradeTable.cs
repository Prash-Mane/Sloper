using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_TECH_GRADE")]
	public class TechGradeTable
	{
		[PrimaryKey, AutoIncrement]
		public int? id { get; set; }
		public int tech_grade_id { get; set; }
		public int grade_type_id { get; set; }
		public string tech_grade { get; set; }
		public int sort_order { get; set; }
		public int grade_bucket_id { get; set; }
		public string sloper_points { get; set; }
	}
}
