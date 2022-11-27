using SloperMobile.Model;
using SloperMobile.Model.IssueModels;
using SloperMobile.Model.ResponseModels;
using System.Threading.Tasks;

namespace SloperMobile.Common.Interfaces
{
	public interface ISynchronizationManager
	{
		Task<AscentReponseModel> SendAscentDataAsync(AscentPostModel model);
		Task<IssueResponseModel> SendIssueDataAsync(IssueModel model);
		Task<int> SendImageDataAsync(AscentImageModel imageData);
        Task SaveReceipt(string receipt);
	}
}
