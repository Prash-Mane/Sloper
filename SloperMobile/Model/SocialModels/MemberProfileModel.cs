using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Converters;

namespace SloperMobile.Model.SocialModels
{
    public class MemberProfile
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public byte FollowerStatus { get; set; }
        public byte FollowingStatus { get; set; }
        public int FriendId { get; set; }
        [JsonConverter(typeof(BoolJsonConverter))]
        public bool FriendStatus { get; set; }
        public string LastName { get; set; }
        public int MemberId { get; set; }
        public string Phone { get; set; }
        public string PhotoURL { get; set; }
        public ProfileProperties ProfileProperties { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string Website { get; set; }
        public int FriendsCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
    }

    public class ProfileProperties
    {
        [JsonConverter(typeof(DateJsonConverter))]
        public DateTime? DOB { get; set; }
        public string Email { get; set; }
        [JsonConverter(typeof(IntJsonConverter))]
        public int FirstYearClimb { get; set; }
        public string Gender { get; set; }
        public decimal? Height { get; set; }
        public string Height_Uom { get; set; }
        public bool? PrivacyClimbingCommunity { get; set; }
        public string UnitOfMeasure { get; set; }
        public int? User_Profile_Id { get; set; }
        public decimal? Weight { get; set; }
        public string Weight_Uom { get; set; }
		public DateTime? profile_update_date { get; set; }
		public string flash { get; set; }
        public double flashpercentage { get; set; }
        public string onsight { get; set; }
        public double onsightpercentage { get; set; }
        public string redpoint { get; set; }
        public double redpointpercentage { get; set; }
        public int ranking { get; set; }
        public double rankingpercent { get; set; }
        public int daysclimbed { get; set; }
        public double climbpercent { get; set; }
        public int sends { get; set; }
        public double sendspercent { get; set; }
    }
}
