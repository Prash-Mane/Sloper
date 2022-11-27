using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

namespace SloperMobile.DataBase.DataTables
{
    [Table("UserInfo_Table")]
    public class UserInfoTable
    {
        [PrimaryKey]
        public int UserID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public int? FirstYearClimb { get; set; }
        public string UnitOfMeasure { get; set; }
        public string height_uom { get; set; }
        public string weight_uom { get; set; }
        public string temperature_uom { get; set; } = "metric";
        public bool? PrivacyClimbingCommunity { get; set; }
		
		[JsonProperty("number_of_free_crags")]
        public int NumberOfFreeCrags { get; set; }

		//In this manner we can store list of simple objects in the database,
		//it uses Json Serialize to serialize them and then back Deserialize
		//[JsonProperty("free_downloaded_crag_ids")] 
		//public string FreeDownloadedCragIdsBlobbed { get; set; }
	}
}
