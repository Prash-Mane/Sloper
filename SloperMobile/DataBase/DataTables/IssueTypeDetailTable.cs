using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TISSUE_TYPE_DETAIL")]
    public class IssueTypeDetailTable
	{
		[PrimaryKey, Indexed]
		public long issue_type_detail_id { get; set; }
        public string issue_type_detail_name { get; set; }
        public long sort_order { get; set; }
    }
}
