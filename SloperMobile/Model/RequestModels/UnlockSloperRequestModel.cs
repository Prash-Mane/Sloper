using Newtonsoft.Json;
using System;

namespace SloperMobile.Model.RequestModels
{
    public class UnlockSloperRequestModel
    {
        [JsonProperty("unlock_type_id")]
        public int UnlockTypeId { get; set; }

        [JsonProperty("sloper_id")]
        public int SloperId { get; set; }

        [JsonProperty("purchase_id")]
        public string PurchaseId { get; set; }

        [JsonProperty("app_product_id")]
        public int AppProductId { get; set; }

		[JsonProperty("app_service_id")]
		public string AppServiceId { get; set; }
		
		[JsonProperty("deviceType")]
		public int DeviceType { get; set; }

		public string PurchaseToken { get; set; }
        public DateTime TransactionDateUtc { get; set; }

		[JsonProperty("appID")]
		public Int64 AppID { get; set; }

		[JsonProperty("packageName")]
		public string PackageName { get; set; }

		[JsonProperty("receiptData")]
	    public string ReceiptData { get; set; }
	}
}
