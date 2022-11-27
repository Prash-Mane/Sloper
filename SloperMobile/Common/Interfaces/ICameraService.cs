using System.IO;
using System.Threading.Tasks;
using Plugin.Media.Abstractions;

namespace SloperMobile.Common.Interfaces
{
	public interface ICameraService
	{
		Task<bool> CheckPermissions();
        Task<MediaFile> TakePhotoAsync(string folderName, string pageName);
        Task<MediaFile> PickFileAsync();
	}
}
