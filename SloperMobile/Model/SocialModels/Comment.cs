using Prism.Mvvm;
using SloperMobile.Common.Constants;
using System;

namespace SloperMobile.Model.SocialModels
{
	public class Comments : BindableBase
	{
        public int CommentId { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public string DisplayName { get; set; }
        public int UserId { get; set; }
		public DateTime profile_update_date { get; set; }
		public string ProfileImageUrl
        {
            get => ApiUrls.Url_M_ProfileAvatarThumbnail(UserId, profile_update_date);
        }
    }
    
}
