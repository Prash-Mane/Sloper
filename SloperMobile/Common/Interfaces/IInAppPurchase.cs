using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
	public interface IInAppPurchase
	{
        Task<bool> PurchaseAppAsync(string productId, int appProductId);
        Task<bool> CreatePurchaseAsync(int sloperId, int productTypeId, string productId, int appProductId);
		//Task<bool> IsPurchasedAsync(string productId);
	}
}
