using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.iOS.DependancyObjects;
using System;
using System.IO;
using SQLite;
using Xamarin.Forms;
using System.Diagnostics;

[assembly: Dependency(typeof(SQLite_iOS))]
namespace SloperMobile.iOS.DependancyObjects
{
	public class SQLite_iOS : ISQLite
	{
		static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
		string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder

		public SQLiteAsyncConnection GetAsyncConnection()
		{
			var sqliteFilename = AppSetting.APP_DBNAME;
			var path = Path.Combine(libraryPath, sqliteFilename);

			if (!File.Exists(path))
			{
				File.Create(path);

				////todo: XAM-1121
				//DateTime lastDbReadTime = default(DateTime);
				//string lastUsedDbPath = null;
				//foreach (var dbFilePaths in Directory.EnumerateFiles(libraryPath, "*.db3"))
				//{
				//    var accessTime = File.GetLastAccessTime(dbFilePaths);
				//    if (accessTime > lastDbReadTime) {
				//        lastDbReadTime = accessTime;
				//        lastUsedDbPath = dbFilePaths;
				//    }
				//}
				//if (lastUsedDbPath != null) { 
				//    //ADD MIGRATION LOGIC HERE
				//    //We want to copy table entries from old to DB to new one if it is possible
				//}
			}

			//Logic to remove old dbs:
			//foreach (var dbFilePath in Directory.EnumerateFiles(libraryPath, "*.db3"))
			//{
			//    if (Path.GetFileName(dbFilePath) == sqliteFilename)
			//        continue;

			//    try
			//    {
			//        File.Delete(dbFilePath);
			//    }
			//    catch
			//    {
			//        Debug.WriteLine($"{dbFilePath} was not removed");
			//    }
			//}

			var conn = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

			// Return the database connection 
			return conn;
		}

		public SQLiteConnection GetConnection()
		{

			var sqliteFilename = AppSetting.APP_DBNAME;
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
			var path = Path.Combine(libraryPath, sqliteFilename);

			// This is where we copy in the prepopulated database
			Console.WriteLine(path);
			if (!File.Exists(path))
			{
				File.Create(path);
			}

			var conn = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);

			// Return the database connection 
			return conn;
		}

		public SQLiteAsyncConnection GetAsyncOldDbConnection()
		{
			string lastUsedDbPath = null;
			DateTime lastDbReadTime = default(DateTime);
			foreach (var dbFilePaths in Directory.EnumerateFiles(libraryPath, "*.db3"))
			{
				if (Path.GetFileName(dbFilePaths) != AppSetting.APP_DBNAME)
				{
					var accessTime = File.GetLastAccessTime(dbFilePaths);
					if (accessTime > lastDbReadTime)
					{
						lastDbReadTime = accessTime;
						lastUsedDbPath = dbFilePaths;
					}
				}

			}
			if (lastUsedDbPath != null)
				return new SQLiteAsyncConnection(lastUsedDbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
			else
				return null;
		}
		public void DeleteOldDb()
		{
			foreach (var dbFilePath in Directory.EnumerateFiles(libraryPath, "*.db3"))
			{
				if (Path.GetFileName(dbFilePath) == AppSetting.APP_DBNAME)
					continue;
				try
				{
					File.Delete(dbFilePath);
				}
				catch
				{
					Debug.WriteLine($"{dbFilePath} was not removed");
				}
			}
		}
	}
}
