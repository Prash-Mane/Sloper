using Xamarin.Forms;

namespace SloperMobile.Model.RankingModels
{
	public class ShowRankingModel
	{
		public int Rank { get; set; }
		public string Name { get; set; }
		public string Points { get; set; }
		public Color HighlightTextColor { get; set; }
        public int UserID { get; set; }
    }
}
