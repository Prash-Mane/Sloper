using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Model
{    
    public class SocialUserData
    {                
        public virtual string name { get; set; }        
        public virtual string email { get; set; }                
        public virtual string first_name { get; set; }        
        public string gender { get; set; }        
        public string id { get; set; }
        public virtual string last_name { get; set; }        
        public virtual string locale { get; set; }        
        public virtual string Name { get; set; }
        public string PreferredEmail { get; set; }
        public virtual string ProfileImage { get; set; }        
        public string timezone { get; set; }        
        public string TimeZoneInfo { get; set; }        
        public virtual string UserName { get; set; }        
        public virtual string Website { get; set; }        
        public string screen_name { get; set; }
        public string accessToken { get; set; }        
        public string givenName { get; set; }
        public string surname { get; set; }
        public string displayName { get; set; }
        public string userPrincipalName { get; set; }
        public string siteUrl { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string profile_image_url { get; set; }
        public string picture { get; set; }
        public string birthday { get; set; }
        public string accountName { get; set; }
    }
    public class FbProfileData
    {
        public picture picture { get; set; }
    }
    public class picture
    {
        public data data { get; set; }
    }
    public class data
    {
        public string height { get; set; }
        public string url { get; set; }
        public string width { get; set; }
        public bool is_silhouette { get; set; }
    }

    public class RegUserData
    {
        public string email { get; set; }
        public string account { get; set; }
    }
}
