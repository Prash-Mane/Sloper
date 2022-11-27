using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_BUCKET")]
	public class BucketTable
	{
		[PrimaryKey, AutoIncrement]
		public int? ID { get; set; }
		[NotNull]
		public int grade_type_id { get; set; }
		public int grade_bucket_id { get; set; }
		public string bucket_name { get; set; }
		public string hex_code { get; set; }
		public string grade_bucket_group { get; set; }
	}
}
