using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Model
{
    public class UserDetails
    {
        public string SocialMediaId { get; set; }
        public string UserId { get; set; }
        public string TwitterId { get; set; }
        public string APIkey { get; set; }
        public string APISecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string AuthType { get; set; }
        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Token);
            }
        }
    }
}
