using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Model
{
    public class GooglePlusUser
    {
        public GooglePlusUser(string id, string firstName, string lastName, string email, string imageUrl, string gender, string displayName)
        {           
            Id = id;
            DisplayName = displayName;
            Gender = gender;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Picture = imageUrl;
        }

        public string Id { get; set; }

        public string Token { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Picture { get; set; }
        public string DisplayName { get; set; }
        public string Gender { get; set; }
    }
}
