using Prism.Mvvm;
using SloperMobile.Common.NotifyProperties;
using System;
using System.Collections.ObjectModel;
using SloperMobile.DataBase.DataTables;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SloperMobile.Model.PurchaseModels
{
    public class PurchaseCrag
    { 
        public CragExtended Crag { get; set; }
        public string RootTitle { get; set; }
        public string Country { get; set; }
        public string Area { get; set; }
        public string Guide { get; set; }
        public string Image { get => Crag.is_downloaded ? (Crag.Unlocked ? "sloper_premium_50w" : "sloper_basic_50w") : (Crag.Unlocked ? "sloper_premium_faded_50w" : ""); }
        public Color TextColor { get => Crag.Unlocked ? (Crag.is_downloaded ? Color.FromHex("#FF8E2D") : Color.FromHex("#A6FF8E2D")) : (Crag.is_downloaded ? Color.White : Color.FromHex("#B3FFFFFF")); }
        public bool ShowGuidebookDownload { get; set; }
    }
}
