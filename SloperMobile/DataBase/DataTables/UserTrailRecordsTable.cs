using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SloperMobile.DataBase.DataTables;

namespace SloperMobile
{
    [Table("UserTrailRecordsTable")]
    public class UserTrailRecordsTable
    {
        [PrimaryKey, AutoIncrement]
        public int? id { get; set; }
        public int user_id { get; set; }
        [ForeignKey(typeof(CragTable))]
        public int crag_id { get; set; }
        [OneToMany]
        public List<UserLocationTable> Records { get; set; }
        public DateTime start_time { get; set; } = DateTime.UtcNow;
        public DateTime? end_time { get; set; }
        [ForeignKey(typeof(SectorTable))]
        public int? sector_start_id { get; set; }
        [ForeignKey(typeof(SectorTable))]
        public int? sector_end_id { get; set; }
        [ForeignKey(typeof(ParkingTable))]
        public int? parking_start_id { get; set; }
        [ForeignKey(typeof(ParkingTable))]
        public int? parking_end_id { get; set; }
        //what if it's for parking that's not in the database? set parking_id to -1?
    }
}
