using SloperMobile.DataBase.DataTables;
using SloperMobile.Model.PurchaseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
	public interface IPurchasedCheckService
	{
        Task UpdateStateAsync(IEnumerable<CragExtended> crags = null, IEnumerable<GuideBookTable> guidebooks = null);
	}
}
