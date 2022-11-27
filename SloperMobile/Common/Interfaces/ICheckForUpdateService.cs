using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
	public interface ICheckForUpdateService
	{
		Task<string> RunCheckForUpdates(bool isFromCheckForUpdates = false, string lastUpdate = null);
	}
}
