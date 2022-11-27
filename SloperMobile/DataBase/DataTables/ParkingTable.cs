using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SloperMobile.DataBase.DataTables;
namespace SloperMobile
{
    [Table("ParkingTable")]
    public class ParkingTable
    {
        [PrimaryKey, Indexed]
        public int id { get; set; }
        [ForeignKey(typeof(CragTable))]
        public int crag_id { get; set; }
        [ManyToOne(nameof(crag_id), nameof(crag))]
        public CragTable crag { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
