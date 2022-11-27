namespace SloperMobile.Model.IssueModels
{
	public class IssueModel
	{
		public int route_id { get; set; }
		public int issue_category_id { get; set; }
		public int issue_type_id { get; set; }
		public int issue_type_detail_id { get; set; }
		public string bolt_numbers { get; set; }
		public string image { get; set; }
		public string comments { get; set; }
	}
}
