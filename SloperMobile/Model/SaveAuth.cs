namespace SloperMobile.Model
{
	public class SaveAuth
	{
		public bool IsAuth { get; set; }
		public long UserId { get; set; }
		public string AuthKey { get; set; }
		public string AuthSecret { get; set; }
		public string AuthtokenSecret { get; set; }
		public string Authtoken { get; set; }
		public string AuthType { get; set; }
	}
}