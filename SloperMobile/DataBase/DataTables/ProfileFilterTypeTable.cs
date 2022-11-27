using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TUSER_PROFILE_FILTER_TYPE")]
    public class ProfileFilterTypeTable
    {
        [PrimaryKey, AutoIncrement]
        public int? ID { get; set; }
        public string user_profile_filter_type_id { get; set; }
        public string user_profile_filter_type_name { get; set; }
    }
}
