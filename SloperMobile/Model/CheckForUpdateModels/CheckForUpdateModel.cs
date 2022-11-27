namespace SloperMobile.Model.CheckForUpdateModels
{
	public class CheckForUpdateModel
	{
		public int areas_modified { get; set; }
		public int crags_modified { get; set; }
		public int guidebooks_modified { get; set; }
		public int sectors_modified { get; set; }
		public int routes_modified { get; set; }
        public int maps_modified { get; set; }
        public int map_regions_modified { get; set; }
        public int grades_modified { get; set; }
        public int trails_modified { get; set; }
        public int parkings_modified { get; set; }
		public string updated_date { get; set; }
	}
}
