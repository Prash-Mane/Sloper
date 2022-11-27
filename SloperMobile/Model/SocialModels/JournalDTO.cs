using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Model.SocialModels
{
    public class CommentDTO
    {
        public long JournalId { get; set; }
        public string Comment { get; set; }
        public string Mentions { get; set; }
    }
}
