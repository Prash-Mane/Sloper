using System;
using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_TEMP_PROCESS")]
	public class TempAscentTable
	{
		[PrimaryKey, AutoIncrement]
		public int? id { get; set; }
		public int ascent_id { get; set; }
		public int route_id { get; set; }
		public int ascent_type_id { get; set; }
		public DateTime ascent_date { get; set; }
		public int route_type_id { get; set; }
		public int grade_id { get; set; }
		public int tech_grade_id { get; set; }
		public string rating { get; set; }
		public string photo { get; set; }
		public string comment { get; set; }
		public string video { get; set; }
		public string climbing_angle { get; set; }
		public string climbing_angle_value { get; set; }
		public string hold_type { get; set; }
		public string hold_type_value { get; set; }
		public string route_style { get; set; }
		public string route_style_value { get; set; }
		public string ImageData { get; set; }
		public string ImageName { get; set; }
	}
}
