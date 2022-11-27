using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SloperMobile.DataBase.DataTables;
namespace SloperMobile
{
    [Table("TrailTable")]
    public class TrailTable : ITrail
    {
        [PrimaryKey, Indexed]
        public int id { get; set; }
        [ForeignKey(typeof(CragTable))]
        public int crag_id { get; set; }
        public string LocationsJson { get; set; }
        //public List<LocationTable> Locations { get; set; }

        [ForeignKey(typeof(SectorTable))]
        public int? sector_start_id { get; set; }
        [ForeignKey(typeof(SectorTable))]
        public int? sector_end_id { get; set; }
        public int? parking_start_id { get; set; }
        public int? parking_end_id { get; set; }

        public string HexColor { get; set; } = "#0000FF"; //just idea if we want to have color based on trail. Other option is to display color based on average height, but that's more complicated
    }

    //todo: remove
    public interface ITrail
    {
        int? sector_start_id { get; }
        int? sector_end_id { get; }
    }
}
