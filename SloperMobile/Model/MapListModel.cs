using Xamarin.Forms;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SloperMobile.Model
{
    public class MapListModel
    {
        public int SectorId { get; set; }
        public string SectorName { get; set; }
		public string SectorSubInfo { get; set; }
        [JsonIgnore]
		public ImageSource SectorImage { get; set; }
        [Obsolete("Always empty")]
        public string SectorLatLong { get; set; }
        public string SectorShortInfo { get; set; }
        [JsonIgnore]
        public ImageSource Steepness1 { get; set; }
        [JsonIgnore]
        public ImageSource Steepness2 { get; set; }
        //public int BucketCount1 { get; set; }
        //public int BucketCount2 { get; set; }
        //public int BucketCount3 { get; set; }
        //public int BucketCount4 { get; set; }
        //public int BucketCount5 { get; set; }
        public IList<int> BucketsCount { get; set; }
        [JsonIgnore]
        public DataTemplate BucketCountTemplate { get; set; }
        public int IsCragOrDefaultImageCount { get; set; }
        public byte[] SectorImageBytes { get; set; }
    }
}
