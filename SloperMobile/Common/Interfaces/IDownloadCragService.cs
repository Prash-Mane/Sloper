using System;
using System.Threading;
using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
	public interface IDownloadCragService
	{
		Task<bool> DownloadAsync(int selectedCragId, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> GetAndSaveGradesAsync();
        Task<bool> GetAndSaveBucketsAsync();
        Task<bool> GetAndSaveTechGradesAsync();

		//void UpdateMenuList();
        event EventHandler<(string message, decimal percents)> ProgressChanged;
	}
}
