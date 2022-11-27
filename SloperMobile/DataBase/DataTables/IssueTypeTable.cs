using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TISSUE_TYPE")]
    public class IssueTypeTable
	{
		[PrimaryKey, Indexed]
		public long issue_type_id { get; set; }
        public string issue_type_name { get; set; }
        public long sort_order { get; set; }
    }
}
