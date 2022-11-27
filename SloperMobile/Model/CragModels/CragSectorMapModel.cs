using System.Collections.Generic;
using Xamarin.Forms;
using System;

namespace SloperMobile.Model
{
    public class MapData
    {
        public int map_id { get; set; }
        public int crag_id { get; set; }
        public string map_name { get; set; }
        public string map_image { get; set; }
        public DateTime map_date_modified { get; set; }
        public double width { get; set; }
        public double height { get; set; }
		public int sort_order { get; set; }
		public List<MapRegion> map_region { get; set; }      
        public bool is_enabled { get; set; }  
    }

    public class MapRegion
    {
        public int crag_id { get; set; }
        public int sector_id { get; set; }
        public int map_id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }      
    }
}
