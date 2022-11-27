namespace SloperMobile.Model.TopoModels
{
	public class TopoDrawingModel
	{
		/// <summary>
		/// RouteID
		/// </summary>
		public int id { get; set; }
		/// <summary>
		/// RouteName
		/// </summary>
		public string name { get; set; }
		public string grade { get; set; }
		public string gradeBucket { get; set; }
		public TopoLineModel line { get; set; }
	}
}
