using System;
using System.Text;

namespace SloperMobile.Common.Helpers
{
	public class DateTimeHelper
	{
		public static string ConvertDate(DateTime utcDate)
		{
			var localDateNow = DateTime.Now;
			var localDate = utcDate.ToLocalTime();

			var timeSpan = localDateNow > localDate ? localDateNow - localDate : localDate - localDateNow;

			var totalDays = (int)timeSpan.TotalDays;

			if (totalDays > 364) return localDate.ToString("MMM d yyyy");
			if (totalDays > 7) return localDate.ToString("MMM d");
			if (totalDays == 1) return string.Format("{0} day ago", totalDays);
			if (totalDays > 0) return string.Format("{0} days ago", totalDays);

			//1 day.. 2 days.. 3 days ago etc. 7th day and beyond -the format would be July 9.Instead of 7 days ago



			var totalHours = (int)timeSpan.TotalHours;
            if (totalHours > 0) return string.Format("{0} hour{1} ago", totalHours, totalHours == 1 ? "" : "s");

			var totalMinutes = (int)timeSpan.Minutes;
			if (totalMinutes > 0) return string.Format("{0} min ago", totalMinutes);

			var totalSeconds = (int)timeSpan.Seconds;
			if (totalSeconds > 0) return string.Format("{0} sec ago", totalSeconds);

			return "1 sec ago";
		}

		public static string ConvertDateHMS(DateTime utcDate)
		{
			var localDateNow = DateTime.Now;
			var localDate = utcDate.ToLocalTime();

			var timeSpan = localDateNow > localDate ? localDateNow - localDate : localDate - localDateNow;

            return ConvertTimespanHMS(timeSpan);
		}

        public static string ConvertTimespanHMS(TimeSpan timeSpan)
        {
            var totalHours = (int)timeSpan.TotalHours;
            var totalMinutes = (int)timeSpan.Minutes;
            var totalSeconds = (int)timeSpan.Seconds;

            var result = new StringBuilder();

            if (totalHours > 0) result.Append($" {totalHours}h"); //return $"{totalHours}h {totalMinutes}m";
            if (totalMinutes > 0) result.Append($" {totalMinutes}m"); //return $"{totalMinutes}m {totalSeconds}s";
            if (totalSeconds > 0 && totalHours == 0 && totalMinutes == 0) result.Append($" {totalSeconds}s"); //return $"{totalSeconds}s";   

            return result.ToString();
        }

		public static long TodayFileTime(int days = 0)
		{
			var dateTime = DateTime.Now;
			var todayDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays(days);

			// Converts the local DateTime to the UTC time.
			DateTime utcdt = todayDate.ToUniversalTime();

			return utcdt.ToFileTimeUtc();
		}


		public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
		{
			int diff = dt.DayOfWeek - startOfWeek;
			if (diff < 0)
			{
				diff += 7;
			}
			return dt.AddDays(-1 * diff).Date;
		}


	}




}

