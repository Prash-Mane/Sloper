using ExifLib;
using SloperMobile.Common.Enumerators;

namespace SloperMobile.Model
{
    public class AscentImageModel
    {
        public ImageType ImageType { get; set; }
        public byte[] ImageBytes { get; set; }
        public ExifOrientation ImageOrientation { get; set; }
        public long AscentId { get; set; }
        public long RouteId { get; set; }
        public long appId { get; set; }
		public int FiveStarAscentCheck { get; set; }
	}
}
