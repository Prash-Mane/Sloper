using System.Threading.Tasks;
using SloperMobile.DataBase.DataTables;

namespace SloperMobile.Common.Interfaces
{
	public interface IExceptionSynchronizationManager
	{
		Task LogException(ExceptionTable exception, bool saveToDb = true);
	}
}