using System;
using Xamarin.Forms;

namespace SloperMobile.Model
{
    public class MasterPageItemModel
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
        public Type TargetType { get; set; }
        public string Contents { get; set; }
        public int ItemId { get; set; }
        public Color ActiveTextColor { get; set; }
        public bool IsContentVisible { get; set; }
        public double ContentHeight { get; set; }
    }
}
