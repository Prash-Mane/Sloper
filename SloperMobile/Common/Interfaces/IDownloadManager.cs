using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SloperMobile.Model.CragModels;
namespace SloperMobile
{
    public interface IDownloadManager
    {
        IEnumerable<CragInfoModel> DownloadingQueue { get; }
        /// <summary>
        /// Adds crags to download queue and starts downloading
        /// </summary>
        void AddCragsToDownload(IEnumerable<CragInfoModel> crags);
        /// <summary>
        /// Is called automatically when items are added. Use manual call to retry on error only
        /// </summary>
        Task DownloadItemsInQueue();
        void StopDownloadingCrag(int cragId);
        void StopAll();
        event EventHandler<(string message, decimal percents)> PopupProgressChanged;
        void IsDisplayToast(bool value);
    }
}
