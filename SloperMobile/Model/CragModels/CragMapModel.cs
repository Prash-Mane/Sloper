using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SloperMobile.DataBase.DataTables;

namespace SloperMobile.Model.CragModels
{
    public class CragMapModel
    { 
        public MapTable Map { get; set; }
        public List<CragSectorMapTable> MapRegions { get; set; }
        public string CragName { get; set; }
    }
}
