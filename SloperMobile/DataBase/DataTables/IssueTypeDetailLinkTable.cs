using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TYPE_TISSUE_TYPE_DETAIL_LINK")]
    public class IssueTypeDetailLinkTable
	{
		[PrimaryKey,AutoIncrement]
		public int? id { get; set; }
		public long issue_type_id { get; set; }
        public long issue_type_detail_id { get; set; }
        public long sort_order { get; set; }
    }
}
