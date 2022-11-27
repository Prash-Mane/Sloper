using System.Runtime.Serialization;
using Xamarin.Forms;

namespace SloperMobile.Model
{
	public class RouteDataModel
	{
		public string RouteIndex { get; set; }
		public string TitleText { get; set; }
		public string SubText { get; set; }
		public string Rating { get; set; }

        [IgnoreDataMember]
		public ImageSource Steepness1 { get; set; }

        [IgnoreDataMember]
        public ImageSource Steepness2 { get; set; }

        [IgnoreDataMember]
        public ImageSource Steepness3 { get; set; }

        [IgnoreDataMember]
        public ImageSource StarImage { get; set; }

        public bool IsShowStarCount = false;
		public string RouteTechGrade { get; set; }
		public string RouteGradeColor { get; set; }
		public int RouteId { get; set; }
		public string ProductId { get; set; }
        public int GrageBucketId { get; set; }

		public bool HasSteepness3 { get;set; }
		public bool HasStarImage { get; set; }
	}
}
