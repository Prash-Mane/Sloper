using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("APP_SETTING")]
	public class AppSettingTable
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		[NotNull]
		public string UPDATED_DATE { get; set; }
		public bool IS_INITIALIZED { get; set; }
	}
}
