using SloperMobile.Model.CragModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
    public interface IRemoveCragManager
    {
        IEnumerable<CragInfoModel> RemovingQueue { get; }
        /// <summary>
        /// Adds crags to remove queue and starts removing
        /// </summary>
        void AddCragsToRemove(IEnumerable<CragInfoModel> crags);
        /// <summary>
        /// Is called automatically when items are added. Use manual call to retry on error only
        /// </summary>
        void RemoveItemsInQueue();
        //void StopRemovingCrag(int cragId);
    }
}
