using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("TUSER_PROFILE_FILTER")]
    public class ProfileFilterTable
    {
        [PrimaryKey, Indexed]
        public int user_profile_filter_id { get; set; }
        public string type { get; set; }
        public string uom { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public string filter_desc { get; set; }
        public int sort_order { get; set; }
    }
}
