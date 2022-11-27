using System;
using System.Threading.Tasks;
using SloperMobile.Common.Interfaces;
using SloperMobile.iOS.DependancyObjects;

[assembly: Xamarin.Forms.Dependency(typeof(PurchaseValidationDependency))]
namespace SloperMobile.iOS.DependancyObjects
{
    //TODO: This has to be changed. If it returns false, than purchase can be rolled back. We should perform our server-side purchase logic here
	public class PurchaseValidationDependency : IPurchaseValidation
	{
		public Task<bool> VerifyPurchase(string signedData, string signature, string productId = null, string transactionId = null)
		{
			ValidatePurchase?.Invoke(signedData);
			return Task.FromResult(true);
		}

		public Action<string> ValidatePurchase { get; set; }
	}
}