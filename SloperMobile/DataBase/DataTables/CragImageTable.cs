using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("TCRAG_IMAGE")]
	public class CragImageTable
	{
		[PrimaryKey]
		public int crag_id { get; set; }
		public string crag_image { get; set; }
        public string crag_portrait_url { get; set; }
        public string crag_landscape_url { get; set; }
        //public string crag_portrait_image { get; set; }
        //public string crag_landscape_image { get; set; }
    }
}
