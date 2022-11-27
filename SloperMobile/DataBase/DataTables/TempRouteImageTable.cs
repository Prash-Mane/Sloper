using ExifLib;
using SloperMobile.Common.Enumerators;
using SQLite;										

namespace SloperMobile.DataBase.DataTables
{
	[Table("T_TEMP_IMAGE")]
	public class TempRouteImageTable
	{			
		[PrimaryKey, AutoIncrement]
		public int? ImageId { get; set; }
		public long? RouteId { get; set; }
		public long? AscentId { get; set; }
		public string ImageBase64 { get; set; }
		public ImageType ImageType { get; set; }   
		public ExifOrientation ImageOrientation { get; set; }
		public long appId { get; set; }
		public int FiveStarAscentCheck { get; set; }
	}
}
