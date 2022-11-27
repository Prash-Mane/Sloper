using System;
using System.IO;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Interfaces;
using SloperMobile.Droid.DependancyObjects;
using SQLite;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.Generic;

[assembly: Dependency(typeof(SQLite_Android))]
namespace SloperMobile.Droid.DependancyObjects
{
    public class SQLite_Android : ISQLite
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            var sqliteFilename = AppSetting.APP_DBNAME;
            var path = Path.Combine(documentsPath, sqliteFilename);

            if (!File.Exists(path))
            {
                File.Create(path);

                //todo: XAM-1121
                //DateTime lastDbReadTime = default(DateTime);
                //string lastUsedDbPath = null;
                //foreach (var dbFilePaths in Directory.EnumerateFiles(documentsPath, "*.db3"))
                //{
                //    var accessTime = File.GetLastAccessTime(dbFilePaths);
                //    if (accessTime > lastDbReadTime)
                //    {
                //        lastDbReadTime = accessTime;
                //        lastUsedDbPath = dbFilePaths;
                //    }
                //}
                //if (lastUsedDbPath != null)
                //{
                //    //ADD MIGRATION LOGIC HERE
                //    //We want to copy table entries from old to DB to new one if it is possible
                //    var db1 = new SQLiteAsyncConnection(lastUsedDbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
                //    var db2 = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
                //    string queryString = $"SELECT name FROM sqlite_master WHERE type = 'table' order by name asc";
                //    var t1d1 = db1.QueryAsync<TableName>(queryString).Result;
                //    foreach (TableName tblname in t1d1)
                //    {

                //        //var data = db1.Table<YourModel>().ToList();
                //        //db2.InsertAll(data);
                //    }
                //}
            }

            ////Logic to remove old dbs:
            //foreach (var dbFilePath in Directory.EnumerateFiles(documentsPath, "*.db3"))
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
            return conn;
        }

        public SQLiteConnection GetConnection()
        {
            var sqliteFilename = AppSetting.APP_DBNAME;
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, sqliteFilename);
            Console.WriteLine(path);
            if (!File.Exists(path)) File.Create(path);
            var conn = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            return conn;
        }

        public SQLiteAsyncConnection GetAsyncOldDbConnection()
        {
            string lastUsedDbPath = null;
			DateTime lastDbReadTime = default(DateTime);
			foreach (var dbFilePaths in Directory.EnumerateFiles(documentsPath, "*.db3"))
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
            foreach (var dbFilePath in Directory.EnumerateFiles(documentsPath, "*.db3"))
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