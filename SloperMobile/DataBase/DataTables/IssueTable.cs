using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TISSUE")]
    public class IssueTable
	{
		[PrimaryKey, Indexed]
		public long issue_id { get; set; }
        public long route_id { get; set; }
        public long user_id { get; set; }
        public string date_reported { get; set; }
        public long issue_category_id { get; set; }
        public long issue_type_id { get; set; }
        public long issue_type_detail_id { get; set; }
        public string bolt_numbers { get; set; }
        public string image { get; set; }
        public string comments { get; set; }
        public string reported_by { get; set; }
    }
}
