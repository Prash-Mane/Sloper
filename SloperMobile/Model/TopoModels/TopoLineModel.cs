using System.Collections.Generic;

namespace SloperMobile.Model.TopoModels
{
	public class TopoLineModel
	{
		public List<TopoPointsModel> points { get; set; }
		public List<TopoPointsTextModel> pointsText { get; set; }
		public TopoStyleModel style { get; set; }
		public TopoMarkerModel marker { get; set; }
	}
}
