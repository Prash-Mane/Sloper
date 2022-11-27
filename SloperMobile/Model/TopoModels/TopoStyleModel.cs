using System.Collections.Generic;

namespace SloperMobile.Model.TopoModels
{
	public class TopoStyleModel
	{
		public string type { get; set; }
		public string width { get; set; }
		public string color { get; set; }
		public List<float> dashPattern { get; set; }
		public bool is_dark_checked { get; set; }
		public TopoShadowModel shadow { get; set; }
	}
}
