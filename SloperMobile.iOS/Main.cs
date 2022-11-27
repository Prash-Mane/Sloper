using System;
using UIKit;

namespace SloperMobile.iOS
{
	public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
			try
			{
				// if you want to use a different Application Delegate class from "AppDelegate"
				// you can specify it here.
				UIApplication.Main(args, null, "AppDelegate");
			}
			catch (Exception ex)
			{
				App.ExceptionSyncronizationManager.LogException(new DataBase.DataTables.ExceptionTable
				{
					Data = $"args = {Newtonsoft.Json.JsonConvert.SerializeObject(args)}",
					Page = "Application",
					Method = "Main",
					StackTrace = ex.StackTrace,
                    Exception = ex.Message
				});
                throw;
			}
		}
    }
}