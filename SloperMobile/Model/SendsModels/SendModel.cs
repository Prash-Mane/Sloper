using System;
using System.Net;
using ExifLib;
using Prism.Mvvm;

namespace SloperMobile.Model.SendsModels
{
    public class SendModel : BindableBase
    {
        private DateTime date_Climbed;
        private bool isDeleteVisible;
        private string dateClimbed;

        public int Ascent_Id { get; set; }
        public int User_Id { get; set; }
        public int Route_Id { get; set; }
        public int Ascent_Type_Id { get; set; }
        public int Route_Type_Id { get; set; }
        public int Grade_Id { get; set; }
        public int Tech_Grade_Id { get; set; }
        public int Image_id { get; set; }
        public int sector_id { get; set; }
        public int grade_type_id { get; set; }
        public long crag_id { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public string Video { get; set; }
        public string Climbing_Angle { get; set; }
        public string Hold_Type { get; set; }
        public string Route_Style { get; set; }
        public string Guid { get; set; }
        public DateTime Date_Modified { get; set; }
        public string Ascent_Type_Description { get; set; }
        public string Tech_Grade_Description { get; set; }
        //public string route_name { get; set; }
        public string sector_name { get; set; }
        public string crag_name { get; set; }
        public string tech_grade { get; set; }
        public string topoJsonData { get; set; }
        public string userSelectedImage { get; set; }
        public ExifOrientation Orientation { get; set; }

        public bool IsDeleteVisible
        {
            get
            {
                return isDeleteVisible;
            }
            set
            {
                SetProperty(ref isDeleteVisible, value);
            }
        }

        public string DateClimbed
        {
            get { return dateClimbed; }
            set
            {
                dateClimbed = value; RaisePropertyChanged();

            }
        }

        public DateTime Date_Climbed
        {
            get { return date_Climbed; }
            set
            {
                date_Climbed = value;
                DateClimbed = value.ToString("MM/dd/yy");
            }
        }

        #region Decoded Properties

        private string routeName;
        public string route_name
        {
            get => WebUtility.HtmlDecode(routeName);
            set => routeName = value;
        }

        #endregion

    }
}
