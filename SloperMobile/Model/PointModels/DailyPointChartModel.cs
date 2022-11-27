namespace SloperMobile.Model.PointModels
{
	public class DailyPointChartModel
	{
		public string Name { get; set; }
		public double PointPercentage { get; set; }
		public string ClimbDate { get; set; }

		public DailyPointChartModel(string name, double pointPercentage, string climbDate)
		{
			this.Name = name;
			this.PointPercentage = pointPercentage;
			this.ClimbDate = climbDate;
		}
	}
}
