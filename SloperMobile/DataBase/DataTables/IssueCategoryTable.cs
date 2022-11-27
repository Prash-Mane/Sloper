using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TISSUE_CATEGORY")]
    public class IssueCategoryTable
	{
		[PrimaryKey, Indexed]
		public long issue_category_id { get; set; }
        public string issue_category_name { get; set; }
        public int sort_order { get; set; }
    }
}
