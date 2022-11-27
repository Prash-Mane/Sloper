using System;
using Newtonsoft.Json;

namespace SloperMobile
{
    public class WelcomeSlideModel
    {
        [JsonProperty("slidename")]
        public string SlideName { get; set; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }

        [JsonProperty("margin")]
        public int BottomMargin { get; set; }
    }
}
