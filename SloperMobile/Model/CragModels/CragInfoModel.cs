using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SloperMobile.Model.CragModels
{
    public class CragInfoModel : BindableBase
    {
        public enum CragStatus { Default, Downloaded, DownloadQueued, RemoveQueued, Downloading, Removing, Cancellation }   //

		public int CragID { get; set; }
        private string _cragname;
        public string CragName
        {
            get { return string.IsNullOrEmpty(_cragname) ? _cragname : _cragname.ToUpper(); }
            set { _cragname = value; RaisePropertyChanged("CragName"); }
        }


        public string ActionImage
        {
            get
            {
                switch (State)
                {
                    case CragStatus.Downloaded:
                    case CragStatus.Removing:
                    case CragStatus.Downloading:
                        return "icon_guidebook_remove.png";
                    case CragStatus.DownloadQueued:
                        return "icon_guidebook_queue.png";
                    case CragStatus.RemoveQueued:
                        return "icon_guidebook_queue.png";
                    case CragStatus.Default:
                    default:
                        return "icon_guidebook_download.png";
                }
            }
        }

        private decimal _progrssVal;
        public decimal ProgressValue
        {
            get => _progrssVal;
            set
            {
                _progrssVal = value * App.DeviceScreenWidth;
                RaisePropertyChanged(nameof(ProgressValue));
            }
        }

        public string ProgressStatus { get; set; }

        public bool Unlocked { get; set; }

        public bool IsFree { get; set; }

        CragStatus state;
        public CragStatus State
        {
            get => state;
            set
            {
                state = value;
                RaisePropertyChanged(nameof(State));
                RaisePropertyChanged(nameof(ActionImage));
                if (state == CragStatus.Downloaded)
                    ProgressValue = 1;
                if (state == CragStatus.Default)
                    ProgressValue = 0;
            }
        }
    }
}
