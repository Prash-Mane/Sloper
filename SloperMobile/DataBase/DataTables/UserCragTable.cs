using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("TUSER_CRAG")]
	public class UserCragTable
	{
		[PrimaryKey, Indexed]
		public int id { get; set; }
		public int crag_id { get; set; }
		public bool is_default { get; set; }
	}
}
