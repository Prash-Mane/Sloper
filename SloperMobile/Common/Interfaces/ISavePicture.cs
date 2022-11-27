using System.IO;
using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
	public interface ISavePicture
    {
        Task<string> SavePictureToDisk(string filename, byte[] imageData);
		Task<Stream> GetImage(string path);

	}
}
