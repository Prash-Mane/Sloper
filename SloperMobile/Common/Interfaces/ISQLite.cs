using SQLite;

namespace SloperMobile.Common.Interfaces
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection(); //not used atm

	    SQLiteAsyncConnection GetAsyncConnection();
        //Added by Ravi 08-08-2018
        SQLiteAsyncConnection GetAsyncOldDbConnection();
        void DeleteOldDb();
    }
}
