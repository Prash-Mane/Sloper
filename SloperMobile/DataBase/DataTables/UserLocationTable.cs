using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SloperMobile
{
    [Table("UserLocationTable")]
    public class UserLocationTable
    {
        [PrimaryKey, AutoIncrement]
        public int? id { get; set; }
        [ForeignKey(typeof(UserTrailRecordsTable))]
        public int user_trail_id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime time_utc { get; set; }
    }
}
