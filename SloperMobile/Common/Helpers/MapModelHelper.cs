using System;
using System.Threading.Tasks;
using SloperMobile.Model;
using SloperMobile.DataBase;
using SloperMobile.DataBase.DataTables;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using SloperMobile.Model.ResponseModels;
using System.Linq;
using SloperMobile.Common.Helpers;
using SloperMobile.Common.Constants;
using SloperMobile.Common.Enumerators;
using System.IO;
using SloperMobile.Common.Interfaces;

namespace SloperMobile
{
    public static class MapModelHelper
    {
        public static async Task<MapListModel> GetFromSectorIdAsync(int sectorId,
                                                                    IRepository<SectorTable> sectorRepository = null,
                                                                    IRepository<TopoTable> topoRepository = null,
                                                                    IRepository<CragImageTable> cragImageRepository = null) 
        {
            if (sectorRepository == null)
                sectorRepository = new Repository<SectorTable>();
            if (topoRepository == null)
                topoRepository = new Repository<TopoTable>();
            if (cragImageRepository == null)
                cragImageRepository = new Repository<CragImageTable>();

            var mapListModel = new MapListModel();
            mapListModel.SectorId = sectorId;
            var sectorItem = await sectorRepository.GetAsync(sectorId);
            mapListModel.SectorName = sectorItem.sector_name;
            mapListModel.SectorShortInfo = sectorItem.sector_info_short;
            if (!string.IsNullOrEmpty(sectorItem.angles_top_2) && sectorItem.angles_top_2.Contains(","))
            {
                string[] steeps = sectorItem.angles_top_2.Split(',');
                mapListModel.Steepness1 = ImageSource.FromFile(GetSteepnessResourceName(Convert.ToInt32(steeps[0])));
                mapListModel.Steepness2 = ImageSource.FromFile(GetSteepnessResourceName(Convert.ToInt32(steeps[1])));
            }
            else
            {
                mapListModel.Steepness1 = ImageSource.FromFile(GetSteepnessResourceName(2));
                mapListModel.Steepness2 = ImageSource.FromFile(GetSteepnessResourceName(4));
            }

            var topoDb = await topoRepository.FindAsync(t => t.sector_id == sectorId);
            if (topoDb == null || string.IsNullOrEmpty(topoDb.topo_json) || topoDb.topo_json == "[]")
                mapListModel.SectorImage = await GetCragImageAsync(cragImageRepository);
            else
            {
                var topoModel = JsonConvert.DeserializeObject<List<TopoImageResponseModel>>(topoDb.topo_json)?.FirstOrDefault();
                if (topoModel == null || topoModel.image == null || string.IsNullOrEmpty(topoModel.image.data))
                    mapListModel.SectorImage = await GetCragImageAsync(cragImageRepository);
                else
                    mapListModel.SectorImage = topoModel.image.data.GetImageSource();
            }

            return mapListModel;
        }

        public static async Task<ImageSource> GetCragImageAsync(IRepository<CragImageTable> cragImageRepository, int activeCrag = 0, bool defaultLandscape = true)
        {
            var activeCragId = activeCrag == 0 ? Settings.ActiveCrag : activeCrag;

            var cragImg = await cragImageRepository.FindAsync(ci => ci.crag_id == activeCragId);

            var base64Img = cragImg?.crag_image;
            return !string.IsNullOrEmpty(base64Img) ? base64Img.GetImageSource() : GetDefaultImage(defaultLandscape);
        }

        public static ImageSource GetDefaultImage(bool landscape)
        {
            string fileName;

            if (AppSetting.APP_TYPE == "indoor")
                fileName = landscape ? "default_sloper_indoor_landscape.jpg" : "default_sloper_indoor_portrait.jpg";
            else
                fileName = landscape ? "default_sloper_outdoor_landscape.jpg" : "default_sloper_outdoor_portrait.jpg";
                
            return ImageSource.FromFile(fileName);
        }

        static string GetSteepnessResourceName(int steep)
        {
            switch ((AppSteepness)steep)
            {
                case AppSteepness.Slab:
                    return "icon_steepness_1_slab_border_54x54.png";
                case AppSteepness.Vertical:
                    return "icon_steepness_2_vertical_border_54x54.png";
                case AppSteepness.Overhanging:
                    return "icon_steepness_4_overhanging_border_54x54.png";
                case AppSteepness.Roof:
                    return "icon_steepness_8_roof_border_54x54.png";
            }
            return "";
        }

        public static async Task<TopoImageResponseModel> GetDefaultTopo(IRepository<CragImageTable> cragImageRepository) 
        { 
            var activeCragId = Settings.ActiveCrag;
            var cragImg = await cragImageRepository.FindAsync(ci => ci.crag_id == activeCragId);
            if (cragImg != null && !string.IsNullOrEmpty(cragImg.crag_image))
            {
                return TopoFromBase64Image(cragImg?.crag_image);
            }

            var fileManager = DependencyService.Get<IFileManager>();
            var fileName = AppSetting.APP_TYPE == "indoor" ? "default_sloper_indoor_square" : "default_sloper_outdoor_square";
            var fileBytes = fileManager.ReadAllBytes(fileName);
            if (fileBytes == null)
                return null;
            var imgResizer = DependencyService.Get<IImageResizer>();
            var resizedImg = imgResizer.ResizeImage(fileBytes, 1024, 1024); //is needed, since it converts image to jpeg. And that's the only format that's working correctly with canvas
            var base64 = Convert.ToBase64String(resizedImg);
            var metadataImageString = $"data:image/Jpeg;base64,{base64}";
            return TopoFromBase64Image(metadataImageString);
        }

        static TopoImageResponseModel TopoFromBase64Image(string img) 
        { 
            return new TopoImageResponseModel
            {
                image = new Model.TopoModels.TopoImageModel
                {
                    height = "1024",
                    width = "1024",
                    data = img
                }
            };
        }

        public static async Task<byte[]> GetSectorImageBytes(int sectorId) 
        { 
            var topoRepository = new Repository<TopoTable>();
            var topoDb = await topoRepository.FindAsync(t => t.sector_id == sectorId);
            if (topoDb == null || string.IsNullOrEmpty(topoDb.topo_json) || topoDb.topo_json == "[]")
                return await GetCragImageBytesAsync();
            else
            {
                var topoModel = JsonConvert.DeserializeObject<List<TopoImageResponseModel>>(topoDb.topo_json)?.FirstOrDefault();
                if (topoModel == null || topoModel.image == null || string.IsNullOrEmpty(topoModel.image.data))
                    return await GetCragImageBytesAsync();
                else
                   return topoModel.image.data.GetImageBytes();
            }
        }

        static async Task<byte[]> GetCragImageBytesAsync(int activeCrag = 0)
        {
            var activeCragId = activeCrag == 0 ? Settings.ActiveCrag : activeCrag;

            var cragImg = await new Repository<CragImageTable>().FindAsync(ci => ci.crag_id == activeCragId);

            return !string.IsNullOrEmpty(cragImg.crag_image) ? cragImg.crag_image.GetImageBytes() : GetDefaultImageBytes();
        }

        static byte[] GetDefaultImageBytes()
        {
            var fileName = "default_sloper_outdoor_square.jpg";
            var fileManager = DependencyService.Get<IFileManager>();
            return fileManager.ReadAllBytes(fileName);
        }
    }
}
