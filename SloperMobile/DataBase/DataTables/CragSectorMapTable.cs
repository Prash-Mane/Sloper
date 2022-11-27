using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("CragSectorMapTable")]
	public class CragSectorMapTable
	{
        [PrimaryKey, Indexed]
        public string id { get; set; }
        public int map_id { get; set; }
        public int sector_id { get; set; }
        public int crag_id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }

        [Ignore]
        public string SectorName { get; set; }
    }
}
