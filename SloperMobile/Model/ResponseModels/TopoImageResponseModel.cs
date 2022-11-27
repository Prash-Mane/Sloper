using System.Collections.Generic;
using SloperMobile.Model.TopoModels;

namespace SloperMobile.Model.ResponseModels
{
	public class TopoImageResponseModel
	{
		public TopoImageModel image { get; set; }
		public IList<TopoDrawingModel> drawing { get; set; }
	}
}
