using System;
using Plugin.InAppBilling.Abstractions;

namespace SloperMobile.Common.Interfaces
{
	public interface IPurchaseValidation : IInAppBillingVerifyPurchase
	{
		Action<string> ValidatePurchase { get; set; }
	}
}