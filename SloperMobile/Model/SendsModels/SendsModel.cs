namespace SloperMobile.Model.SendsModels
{
	public class SendsModel
	{
		public int app_id { get; set; }
		public string start_date { get; set; }
		public string end_date { get; set; }
		public int Ascent_Id { get; set; }
        public int? user_id { get; set; }
	}
}
