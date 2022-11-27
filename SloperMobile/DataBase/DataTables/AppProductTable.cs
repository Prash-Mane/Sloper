using Newtonsoft.Json;
using SQLite;
using System;

namespace SloperMobile.DataBase.DataTables
{
    //TODO: Remove from SQLite maybe since it's not used
    [Table("TAPP_PRODUCT")]
    public class AppProductTable
    {
		[PrimaryKey]
        [JsonProperty("app_product_id")]
        public int AppProductId { get; set; }

        [JsonProperty("app_product_price")]
        public double AppProductPrice { get; set; }

        [JsonProperty("app_product_type_id")]
        public int AppProductTypeId { get; set; } //1 - App, 2 - Guidebook, 3 - Crag
        
        [JsonProperty("service_product_id")]
        public string ServiceProductId { get; set; }

        [JsonProperty("date_modified")]
        public DateTime DateModified { get; set; }

        [JsonProperty("sloperId")]
        public long SloperId { get; set; }

        //[JsonProperty("is_google_enabled")]
        //public bool IsGoogleEnabled { get; set; }

        //[JsonProperty("is_itunes_enabled")]
        //public bool IsiTunesEnabled { get; set; }

        [JsonProperty("is_app_store_ready")]
        public bool IsAppStoreReady { get; set; }

        [JsonProperty("subscription_range")]
        public SubscriptionRange SubscriptionRange { get; set; }
    }

    public enum ProductTypes { App = 1, Guidebook, Crag }

    public enum SubscriptionRange { Monthly = 0, HalfYear, Yearly }
}
