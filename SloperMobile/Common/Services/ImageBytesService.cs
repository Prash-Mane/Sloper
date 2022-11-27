using System.Reflection;
using SloperMobile.Common.Interfaces;

namespace SloperMobile.Common.Services
{
	public class ImageBytesService : IGetImageBytes
	{
		public byte[] GetImageBytes(string imageName)
		{
			var cFilename = $"SloperMobile.Embedded.{imageName}";
			var assembly = this.GetType().GetTypeInfo().Assembly;
			byte[] buffer;
			using (var s = assembly.GetManifestResourceStream(cFilename))
			{
				var length = s.Length;
				buffer = new byte[length];
				s.Read(buffer, 0, (int)length);
			}

			return buffer;
		}
	}
}