using System;

namespace SloperMobile.Model
{
	public class ReportedIssueModel
    {
        private DateTime datereported;
        public string ReportedBy { get; set; }
        public string DateCreated
        {
            get { return datereported.ToString("MM/dd/yy"); }
            set { datereported =Convert.ToDateTime(value); }            
        }

        //public string IssueName { get; set; }
        public int RouteId { get; set; }
		public int IssueId { get; set; }
        public string IssueCategory { get; set; }
        public string IssueType { get; set; }
    }
}
