using System;
using System.Net;
using SQLite;
using System.Collections.Generic;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_SECTOR")]
	public class SectorTable
	{

        #region Decoded Properties

        private string sectorName;
        public string sector_name
        {
            get => WebUtility.HtmlDecode(sectorName);
            set => sectorName = value;
        }

        #endregion

        [PrimaryKey, Indexed]
		public int sector_id { get; set; }
		public int crag_id { get; set; }
		public DateTime date_modified { get; set; }
		public bool is_enabled { get; set; }
		public int map_id { get; set; }
		public string sector_info { get; set; }
		public string sector_info_short { get; set; }
		public string sector_map_rect_h { get; set; }
		public string sector_map_rect_w { get; set; }
		public string sector_map_rect_x { get; set; }
		public string sector_map_rect_y { get; set; }
		//public string sector_name { get; set; }
		public int sort_order { get; set; }
		public string tap_rect_in_parent_map { get; set; }
		//public string topo_name { get; set; }
		//public int topo_type_id { get; set; }
		//public double scale { get; set; }
		public string sector_orientation { get; set; }
		public string version_number { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
		public string angles { get; set; }
		public string angles_top_2 { get; set; }
		public string top2_steepness { get; set; }


        [OneToMany("sector1_id")]
        public List<TrailTable> TrailStarts { get; set; }

        [OneToMany("sector2_id")]
        public List<TrailTable> TrailEnds { get; set; }

        List<TrailTable> trailsCombined;
        public List<TrailTable> TrailsCombined { 
            get {
                if (trailsCombined == null)
                    trailsCombined = TrailStarts.Concat(TrailEnds).ToList();
                return trailsCombined;
            } 
        }
    }

    //Fields from JSON are different. Need to inspect. 19 Apr 2018
    //public class RootObject
    //{
    //    public string area_id { get; set; }
    //    public string crag_id { get; set; }
    //    public string sector_id { get; set; }
    //    public string route_id { get; set; }
    //    public bool is_enabled { get; set; }
    //    public string equipper_date { get; set; }
    //    public string equipper_name { get; set; }
    //    public string first_ascent_date { get; set; }
    //    public string first_ascent_name { get; set; }
    //    public string grade_bucket_id { get; set; }
    //    public string grade_name { get; set; }
    //    public int grade_name_sort_order { get; set; }
    //    public string grade_type_id { get; set; }
    //    public string rating_name { get; set; }
    //    public string route_type { get; set; }
    //    public string route_type_id { get; set; }
    //    public string tech_grade { get; set; }
    //    public int tech_grade_sort_order { get; set; }
    //    public string start_x { get; set; }
    //    public string start_y { get; set; }
    //    public string route_length { get; set; }
    //    public string route_name { get; set; }
    //    public string route_info { get; set; }
    //    public string date_modified { get; set; }
    //    public string date_created { get; set; }
    //    public string route_set_date { get; set; }
    //    public string sort_order { get; set; }
    //    public string graded_list_order { get; set; }
    //    public string version_number { get; set; }
    //    public string number_of_bolts { get; set; }
    //    public string route_safety_notice { get; set; }
    //    public string angles { get; set; }
    //    public string angles_top_1 { get; set; }
    //    public string hold_type_top_1 { get; set; }
    //    public string route_style_top_1 { get; set; }
    //    public string rating { get; set; }
    //}

    //Sample data:
    //"area_id":"67",
    //"crag_id":"10363",
    //"sector_id":"10142",
    //"route_id":"10436",
    //"is_enabled":true,
    //"equipper_date":"2011-11-09",
    //"equipper_name":"Steve Holeczi",
    //"first_ascent_date":"2012-05-01",
    //"first_ascent_name":"Gery Unterasinger",
    //"grade_bucket_id":"4",
    //"grade_name":"YDS Sport (a/b/c/d)",
    //"grade_name_sort_order":15,
    //"grade_type_id":"15",
    //"rating_name":"0",
    //"route_type":"Sport",
    //"route_type_id":"1",
    //"tech_grade":"5.12c",
    //"tech_grade_sort_order":210,
    //"start_x":"733",
    //"start_y":"704",
    //"route_length":"35",
    //"route_name":"Like a Boss",
    //"route_info":"A bit loose still for the first 5 bolts but gets really good higher up. Endurance crux through 3 bolts in the middle, a good no hand rest before the upper crux for the last 10m. At the last bolt, traverse hard left with the bolt at your face.",
    //"date_modified":"2017-08-10",
    //"date_created":"2017-01-01",
    //"route_set_date":"2016-05-03",
    //"sort_order":"10",
    //"graded_list_order":"0",
    //"version_number":"",
    //"number_of_bolts":"14",
    //"route_safety_notice":"",
    //"angles":"4",
    //"angles_top_1":"0",
    //"hold_type_top_1":"0",
    //"route_style_top_1":"0",
    //"rating":"0"
}
