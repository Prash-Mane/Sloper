using System;
using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_EXCEPTION")]
	public class ExceptionTable
	{
		[PrimaryKey, AutoIncrement]
		public int? Id { get; set; }

		[Newtonsoft.Json.JsonProperty("page_name")]
		public string Page { get; set; }

		[Newtonsoft.Json.JsonProperty("method")]
		public string Method { get; set; }

		[Newtonsoft.Json.JsonProperty("line_no")]
		public int Line { get; set; }

		[Newtonsoft.Json.JsonProperty("exception")]
		public string Exception { get; set; }

		[Newtonsoft.Json.JsonProperty("stack_trace")]
		public string StackTrace { get; set; }

		[Newtonsoft.Json.JsonProperty("data")]
		public string Data { get; set; }

		[Newtonsoft.Json.JsonProperty("date")]
		public DateTime? Date { get; set; }

		[Newtonsoft.Json.JsonProperty("user_id")]
		public int? UserId { get; set; }

		[Newtonsoft.Json.JsonProperty("device")]
		public string Device { get; set; }

		[Newtonsoft.Json.JsonProperty("os")]
		public string OS { get; set; }

		[Newtonsoft.Json.JsonProperty("os_version")]
		public string OSVersion { get; set; }

        [Newtonsoft.Json.JsonProperty("app_name")]
        public string AppName { get; set; }

        [Newtonsoft.Json.JsonProperty("app_version")]
        public string AppBuildVersion { get; set; }
	}
}
