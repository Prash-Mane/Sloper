using System;
using System.Collections.Generic;
using SloperMobile.DataBase.DataTables;
namespace SloperMobile
{
    public class BucketComparer : IEqualityComparer<BucketTable>
    {
        public bool Equals(BucketTable x, BucketTable y)
        {
            return x.bucket_name == y.bucket_name && x.hex_code == y.hex_code;
        }

        public int GetHashCode(BucketTable obj)
        {
            return obj.bucket_name.GetHashCode() + obj.hex_code.GetHashCode();
        }
    }
}
