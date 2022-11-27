using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TISSUE_CATEGORY_TISSUE_TYPE_LINK")]
    public class IssueCategoryIssueTypeLinkTable
	{
		[PrimaryKey, AutoIncrement]
		public int? id { get; set; }
		public long issue_category_id { get; set; }
        public long issue_type_id { get; set; }
        public long sort_order { get; set; }
    }
}
