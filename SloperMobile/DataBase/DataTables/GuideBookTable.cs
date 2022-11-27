using System;
using Newtonsoft.Json;
using SQLite;

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_GUIDEBOOK")]
	public class GuideBookTable
	{
        [PrimaryKey, Indexed]
        [JsonProperty("guidebook_id")]
        public long GuideBookId { get; set; }

        [JsonProperty("guidebook_name")]
        public string GuideBookName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("short_code")]
        public int? ShortCode { get; set; }

        [JsonProperty("app_identifier")]
        public int? AppIdentifier { get; set; }

        [JsonProperty("owner_user_id")]
        public int OwnerUserId { get; set; }

        [JsonProperty("global_id")]
        public Guid GlobalId { get; set; }
        
        [JsonProperty("company_id")]
        public int CompanyId { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("date_modified")]
        public DateTime DateModified { get; set; }

        public bool Unlocked { get; set; }

		[JsonProperty("date_unlock_expires")]
		public DateTime DateUnlockExpires { get; set; }

        [JsonProperty("app_product_id")]
        public string ProductId { get; set; }

        [JsonProperty("guidebook_square_image")]
        public string GuidebookSquareImage { get; set; }

        [JsonProperty("guidebook_portrait_image")]
        public string GuidebookPortraitImage { get; set; }

        [JsonProperty("guidebook_landscape_image")]
        public string GuidebookLandscapeImage { get; set; }

		[JsonProperty("guidebook_cover_image_portrait")]
		public string GuidebookPortraitCoverImage { get; set; }

        [JsonProperty("avg_rating")]
        public double AverageRating { get; set; }

        public bool is_app_store_ready { get; set; } = true;

        public bool enable_purchase_app { get; set; } //considered always to be true, other screnarios are not covered

        public bool enable_purchase_guidebook { get; set; }

        public bool enable_purchase_crag { get; set; }

        public bool is_free { get; set; }
    }
}
