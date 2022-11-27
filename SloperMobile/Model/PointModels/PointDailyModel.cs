using System;

namespace SloperMobile.Model.PointModels
{
	public class PointDailyModel
	{
		private DateTime dateclimbed;

		public int? points { get; set; }
		public string ascent_count { get; set; }
		public DateTime date_climbed
		{
			get { return dateclimbed; }
			set
			{
				dateclimbed = value;
			}
		}
	}
}
