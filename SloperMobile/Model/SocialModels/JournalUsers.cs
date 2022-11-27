using System;
using Prism.Mvvm;
using SloperMobile.Common.Constants;

namespace SloperMobile.Model.SocialModels
{
    public class JournalUsers : BindableBase
    {
        public int UserID { get; set; }
        public string DisplayName { get; set; }
        private bool _isfollowing;
        public bool IsFollowing
        {
            get { return _isfollowing; }
            set { _isfollowing = value; RaisePropertyChanged("IsFollowing"); RaisePropertyChanged("FollowText"); RaisePropertyChanged("BackColor"); RaisePropertyChanged("ForeColor"); }
        }

        public string ProfileImageUrl
        {
            get => ApiUrls.Url_M_ProfileAvatarThumbnail(UserID, DateTime.Now);
        }
        public string FollowText { get => IsFollowing ? "FOLLOWING" : "FOLLOW"; }
        public string BackColor { get => IsFollowing ? "#FF8E2D" : "Transparent"; }
        public string ForeColor { get => IsFollowing ? "White" : "Black"; }

        public bool IsFollowingBtnVisible { get; set; } = true;
    }
}
