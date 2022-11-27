using Newtonsoft.Json;

namespace SloperMobile.Model.PurchaseModels
{
	public class UnlockCragResponse
    {
        public int[] Crags { get; set; }
        public int[] Guidebooks { get; set; }

		[JsonProperty("apppurchased")]
		public bool AppPurchased{ get; set; }

        [JsonProperty("freeDownloadedCrags")]
        public int[] FreeDownloadedCrags { get; set; }
	}
}
