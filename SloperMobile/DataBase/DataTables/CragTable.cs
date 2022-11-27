using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using SloperMobile.Model;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace SloperMobile.DataBase.DataTables
{
    public enum CragStatus 
    {
        [Display(Description="In Season")]
        inSeason,
        [Display(Description="Out of Season")]
        outOfSeason,
        [Display(Description="Closed")]
        closed
    }

	[Table("T_CRAG")]
	public class CragTable
	{
        #region Decoded Properties

        private string cragName;
        public string crag_name
        {
            get => WebUtility.HtmlDecode(cragName);
            set => cragName = value;
        }

        private string cragGeneralInfo;
        public string crag_general_info
        {
            get => WebUtility.HtmlDecode(cragGeneralInfo);
            set => cragGeneralInfo = value;
        }

        private string photoCredit;
        public string photo_credit
        {
            get => WebUtility.HtmlDecode(photoCredit);
            set => photoCredit = value;
        }

        private string photoCaption;
        public string photo_caption
        {
            get => WebUtility.HtmlDecode(photoCaption);
            set => photoCaption = value;
        }

        #endregion

        [PrimaryKey, Indexed]
		public int crag_id { get; set; }
        //public string crag_name { get; set; }
        //long?
        public int map_id { get; set; } //not used?
        public string season { get; set; } //not used?
		public string weather_provider_code { get; set; } //not used?
		public string weather_provider_name { get; set; } //not used?
		public string area_name { get; set; } //---> is sometimes read from area_id. Maybe we can assign it locally?
		public int crag_type { get; set; } //not used?
		public string crag_sector_map_name { get; set; } //not used?
		public string crag_gridref { get; set; } //not used?
		public string crag_nearest_town { get; set; } //not used?
		public string crag_is_favourite { get; set; } //not used?
		public int? crag_map_zoom { get; set; } //not used?
		public int? crag_map_id { get; set; } //not used?
		public long crag_guide_book { get; set; }
		public float? crag_parking_intitude { get; set; } //not used?
		public float? crag_parking_latitude { get; set; } //not used?
		public string crag_info_short { get; set; } //not used?
		public float? crag_latitude { get; set; }
		public float? crag_longitude { get; set; }
		public int area_id { get; set; }
		public string crag_access_info { get; set; } //not used
		//public string crag_general_info { get; set; }
		public string crag_parking_info { get; set; }
		public DateTime date_modified { get; set; } //not used
		public string tap_rect_in_area_map { get; set; } //not used
		public string climbing_angles { get; set; } //not used
		public string orientation { get; set; } //not used
		public string sun_from { get; set; } //not used
		public string sun_until { get; set; } //not used
		public string walk_in_angle { get; set; } //not used
		public string walk_in_mins { get; set; } //not used
		public string trail_info { get; set; } //not used
		public long approach_map_id { get; set; } //not used
		public int approach_map_image_id { get; set; } //not used
		public string approach_map_image_name { get; set; } //not used
		public string version_number { get; set; } //not used
		public bool is_enabled { get; set; }
		public int crag_sort_order { get; set; }
		public string route_grades { get; set; }
		public bool is_downloaded { get; set; }
        public int? crag_role_id { get; set; }

		//[JsonProperty("is_google_enabled")]
		//public bool IsGoogleEnabled { get; set; } //both are set from single value from endpoint. We can get rid of one
		
		//[JsonProperty("is_itunes_enabled")]
		//public bool IsiTunesEnabled { get; set; }
        public bool is_app_store_ready { get; set; }

        [JsonIgnore]
		public bool Unlocked { get; set; }
        public string status { get; set; }

        [JsonProperty("app_product_id")]
        public string ProductId { get; set; } //not used

        [JsonProperty("date_unlock_expires")]
		public DateTime? DateUnlockExpires { get; set; } //not used

        public string country { get; set; } //not used

        public bool HasLocation { get => crag_latitude.HasValue && crag_latitude != 0 && crag_longitude.HasValue && crag_longitude != 0; }

        [OneToMany]
        public List<ParkingTable> Parkings { get; set; }

        [OneToMany]
        public List<TrailTable> Trails { get; set; }
    }


	[Table("T_CRAG")]
	public class CragExtended : CragTable
	{
		[Ignore]
		public string crag_image { get; set; }
        [Ignore]
        public string crag_portrait_url { get; set; }
        [Ignore]
        public string crag_landscape_url { get; set; }

        //[Ignore]
        //public string crag_portrait_image { get; set; }

        //[Ignore]
        //public string crag_landscape_image { get; set; }
    }
}
