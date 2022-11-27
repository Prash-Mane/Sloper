using System;
using SQLite;

namespace SloperMobile
{
    [Table("ReceiptTable")]
    public class ReceiptTable
    {
        [PrimaryKey, AutoIncrement]
        public int? id { get; set; }
        public int userId { get; set; }
        public string receiptData { get; set; }
    }
}
