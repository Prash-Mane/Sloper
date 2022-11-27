using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SloperMobile.DataBase.DataTables;
using Plugin.Geolocator.Abstractions;

namespace SloperMobile.Model.GuideBookModels
{
    public class GuideBook : BindableBase
    {
        public GuideBook() { }

        public GuideBook(GuideBookTable dbGB)
        {
            GuideBookId = dbGB.GuideBookId;
            GuideBookName = dbGB.GuideBookName;
            Description = dbGB.Description;
            //OwnerUserId = dbGB.OwnerUserId;
            Unlocked = dbGB.Unlocked;
            Author = dbGB.Author;
            DateModified = $"Updated {dbGB.DateModified.ToString("MMMM")} {dbGB.DateModified.Year}";
            GuidebookSquareImage = dbGB.GuidebookSquareImage == "" ? ImageSource.FromFile("default_sloper_outdoor_square") : dbGB.GuidebookSquareImage.GetImageSource();
            GuidebookPortraitImage = dbGB.GuidebookPortraitImage == "" ? ImageSource.FromFile("default_sloper_outdoor_portrait") : dbGB.GuidebookPortraitImage.GetImageSource();
            GuidebookLandscapeImage = dbGB.GuidebookLandscapeImage == "" ? ImageSource.FromFile("default_sloper_outdoor_landscape") : dbGB.GuidebookLandscapeImage.GetImageSource();
            GuidebookPortraitCoverImage = string.IsNullOrEmpty(dbGB.GuidebookPortraitCoverImage) ? ImageSource.FromFile("default_gb_sloper_outdoor_portrait") : dbGB.GuidebookPortraitCoverImage.GetImageSource();
            Rating = dbGB.AverageRating;
        }

        //public enum GuideBookType { Downloaded, Popular, New, Nearest }

        public long GuideBookId { get; set; }
        private string _guidebookname;
        public string GuideBookName
        {
            get { return string.IsNullOrEmpty(_guidebookname) ? _guidebookname : _guidebookname.ToUpper(); }
            set { _guidebookname = value; RaisePropertyChanged("GuideBookName"); }
        }
        public string Description { get; set; }
        //public int OwnerUserId { get; set; }
        public bool Unlocked { get; set; }
        public ImageSource GuidebookSquareImage { get; set; }
        private ImageSource _portraitImage;
        public ImageSource GuidebookPortraitImage
        {
            get { return _portraitImage; }
            set { _portraitImage = value; RaisePropertyChanged("GuidebookPortraitImage"); }
        }

        public ImageSource GuidebookLandscapeImage { get; set; }

        private ImageSource _portraitCoverImage;
        public ImageSource GuidebookPortraitCoverImage
        {
            get { return _portraitCoverImage; }
            set { _portraitCoverImage = value; RaisePropertyChanged("GuidebookPortraitCoverImage"); }
        }

        public string Author { get; set; }
        //public string DatePublished { get; set; }
        public string DateModified { get; set; }
        public double Rating { get; set; }
        //public int Sortorder { get; set; }
        //public GuideBookType GBType { get; set; }

        public Position GBPosition { get; set; }
        //private bool _isvisibledownloadall;
        //public bool IsVisibleDownloadAll
        //{
        //    get { return _isvisibledownloadall; }
        //    set { _isvisibledownloadall = value; RaisePropertyChanged("IsVisibleDownloadAll"); }
        //}
        //private bool _isvisibleremoveall;
        //public bool IsVisibleRemoveAll
        //{
        //    get { return _isvisibleremoveall; }
        //    set { _isvisibleremoveall = value; RaisePropertyChanged("IsVisibleRemoveAll"); }
        //}
    }
}
