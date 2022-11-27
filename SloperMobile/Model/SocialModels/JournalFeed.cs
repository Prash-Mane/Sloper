using Prism.Mvvm;
using SloperMobile.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Globalization;

namespace SloperMobile.Model.SocialModels
{
    public class JournalFeed : BindableBase
    {
        //Key == ObjectKey. Should be changed to enum when we have them all specified
        static Dictionary<string, string> PatternsToHighlight = new Dictionary<string, string> { 
            { "Ascent", "ascent" },
            { "RouteIssue", "issue" },
            { "PersonalBest", "personal best" }
            //todo: support other types as 5StarRoute, King/QueenOfTheCrag. I don't find them in Postman atm, so don't know the pattern
        };

        public int JournalId { get; set; }
        public long JournalTypeId { get; set; }
        public long PortalId { get; set; }
        public int UserId { get; set; }
        public long ProfileId { get; set; }
        public long SocialGroupId { get; set; }
        string title { get; set; }
        public string Title
        {
            get => title;
            set => title = WebUtility.HtmlDecode(value);
        }
        public string Summary { get; set; }
        ParsedJournalGeneral summaryParsed;
        public ParsedJournalGeneral SummaryParsed
        {
            get => summaryParsed;
            set
            {
                SetProperty(ref summaryParsed, value);
                RaisePropertyChanged("ImgHeight");
            }
        }
        public object Body { get; set; }
        public JItemData ItemData { get; set; }
        public JXml JournalXml { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string ObjectKey { get; set; }
        public string AccessKey { get; set; }
        public object SecuritySet { get; set; }
        public long ContentItemId { get; set; }
        public Journal JournalAuthor { get; set; }
        public Journal JournalOwner { get; set; }
        public object TimeFrame { get; set; }
        [Obsolete("Use LikeByCurrentUser: 0 for false, 1 for true")]
        public bool CurrentUserLikes { get; set; }
        public string JournalType { get; set; }
        public bool IsDeleted { get; set; }
        public bool CommentsDisabled { get; set; }
        public bool CommentsHidden { get; set; }
        public long SimilarCount { get; set; }
        public long KeyId { get; set; }
        public long Cacheability { get; set; }
		public DateTime profile_update_date { get; set; }
		private int _likecounts;
        public int LikeCounts
        {
            get { return _likecounts; }
            set { _likecounts = value; RaisePropertyChanged("LikeCounts"); }
        }
        int commentCount;
        public int CommentCount
        {
            get => commentCount;
            set => SetProperty(ref commentCount, value);
        }

        private int _likebycurrentuser;
        public int LikeByCurrentUser
        {
            get { return _likebycurrentuser; }
            set { _likebycurrentuser = value; RaisePropertyChanged("LikeByCurrentUser"); RaisePropertyChanged("LikeImage"); }
        }

        public string LikeImage
        {
            get => LikeByCurrentUser == 1 ? "like_filled.png" : "like.png";
        }

        private long _commentbycurrentuser;
        public long CommentByCurrentUser
        {
            get { return _commentbycurrentuser; }
            set { _commentbycurrentuser = value; RaisePropertyChanged("CommentByCurrentUser"); }
        }

        public string ProfileImageUrl
        {
            get => ApiUrls.Url_M_ProfileAvatarThumbnail(UserId, profile_update_date);
		}

        public string AttachmentUrl { get => string.IsNullOrEmpty(SummaryParsed?.ImageURL) ? null : AppSetting.Root_Url + ChangeImgSizeUrl(SummaryParsed?.ImageURL); }

        public int ImgHeight { get => string.IsNullOrEmpty(AttachmentUrl) ? 0 : (int)Application.Current.MainPage.Width; }



        public class JXml
        {
            public Items Items { get; set; }
        }

        public class Items
        {
            public object Item { get; set; }
            public object Likes { get; set; }//{ set => LikesParsed = ParseLikes(value?.ToString()); } 
            //public LikeWrapper LikesParsed { get; set; } //cannot be parsed while prop names start with '@'
        }

        public class Journal
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public object Vanity { get; set; }
            public object Avatar { get; set; }
            public long Cacheability { get; set; }
        }

        public class JItemData
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public int Cacheability { get; set; }
        }

        public class LikeWrapper
        {
            public List<UserLike> u { get; set; }
        }

        public class UserLike
        {
            public int uid { get; set; }
            public string un { get; set; }
        }

        //temp workaround until xam-939 is fixed
        //public static LikeWrapper ParseLikes(string input)
        //{
        //    if (string.IsNullOrEmpty(input))
        //        return null;
        //    try
        //    {
        //        var trimmed = input.Replace("@", "");
        //        if (trimmed.StartsWith("{")) //for some reason if there's only 1 entry, array brackets are replaced with field ones
        //        {
        //            var builder = new StringBuilder(trimmed);
        //            builder[0] = '[';
        //            builder[builder.Length - 1] = ']';
        //            trimmed = builder.ToString();
        //        }
        //        return JsonConvert.DeserializeObject<LikeWrapper>(trimmed);
        //    }
        //    catch (Exception e) {
        //        Debug.WriteLine(e.Message);
        //        return null;
        //    }
        //}

        string ChangeImgSizeUrl(string input)
        {
            var rgx = new Regex("w=(\\d+)");
            return rgx.Replace(input, "w=600");
        }

        public FormattedString FormattedTitle
        {
            get
            {
                var formattedString = new FormattedString();

                if (PatternsToHighlight.TryGetValue(ObjectKey, out var textToHightlight) && Title.Contains(textToHightlight))
                {
                    var textBeforeHighlight = Title.Substring(0, Title.LastIndexOf(textToHightlight));
                    var spanBeforeHighlight = new Span
                    {
                        Text = textBeforeHighlight,
                        TextColor = Color.Black
                    };
                    formattedString.Spans.Add(spanBeforeHighlight);

                    var highLightSpan = new Span
                    {
                        //use this one if we don't need upper-case first letters
                        //Text = textToHightlight,
                        Text = Regex.Replace(textToHightlight, @"(^\w)|(\s\w)", m => m.Value.ToUpper()), //to upper-case
                        TextColor = Color.FromHex("#FF8E2D")
                    };
                    formattedString.Spans.Add(highLightSpan);

                    var textAfterHighlight = Title.Substring(textBeforeHighlight.Length + textToHightlight.Length);
                    var spanAfterHighlight = new Span
                    {
                        Text = textAfterHighlight,
                        TextColor = Color.Black
                    };
                    formattedString.Spans.Add(spanAfterHighlight);
                } else
                {
                    var normalTextSpan = new Span
                    {
                        Text = Title,
                        TextColor = Color.Black
                    };
                    formattedString.Spans.Add(normalTextSpan);
                }


                return formattedString;
            }
        }

    }

    public class ParsedJournalGeneral
    {
        public int ID { get; set; }
        public string ImageURL { get; set; }
    }

}