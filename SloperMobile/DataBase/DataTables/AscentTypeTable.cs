using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("TASCENT_TYPE")]
	public class AscentTypeTable
	{
        [PrimaryKey, AutoIncrement, Indexed]
		public int? ascent_type_id { get; set; }
		public string ascent_type_description { get; set; }
	}
}
