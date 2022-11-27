using Newtonsoft.Json;

namespace SloperMobile.Model.ResponseModels
{
	public class SectorRouteCount
	{
		[JsonProperty("sectorCount")]
		public int SectorCount { get; set; }

		[JsonProperty("routeCount")]
		public int RouteCount { get; set; }
	}
}
