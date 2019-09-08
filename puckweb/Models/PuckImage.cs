using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Constants;
using puck.Transformers;
using puck.core.State;
using Microsoft.AspNetCore.Http;

namespace puck.Models
{
    //[PuckAzureBlobImageTransformer()]
    [PuckImageTransformer()]
    public class PuckImage
    {
        [UIHint("SettingsDisplayImage")]
        public string Path { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [UIHint("SettingsReadOnly")]
        public string Size {get;set;}
        [UIHint("SettingsReadOnly")]
        public string Extension { get; set; }
        [UIHint("SettingsReadOnly")]
        public int? Width { get; set; }
        [UIHint("SettingsReadOnly")]
        public int? Height { get; set; }
        public List<CropModel> Crops { get; set; }
        public IFormFile File { get; set; }

        public string GetCropUrl(string cropAlias,string anchor="center") {
            var url = Path;
            if (PuckCache.CropSizes.ContainsKey(cropAlias)) {
                var cropInfo = PuckCache.CropSizes[cropAlias];

                if (url.ToLower().StartsWith("http")) {
                    var uri = new Uri(url);
                    url = $"{uri.AbsolutePath}";
                }
                
                var cropModel = (Crops ?? new List<CropModel> { }).FirstOrDefault(x=>x.Alias==cropAlias);
                /*check that left,top,right,bottom have values and that the cropmodel width and height match the cropinfo width and height.
                 if they don't match, it means that the crop settings have been changed since the crop was saved which should invalidate the crop.*/
                if (cropModel != null && cropModel.Left.HasValue && cropModel.Top.HasValue && cropModel.Right.HasValue && cropModel.Bottom.HasValue
                    && cropInfo.Width == cropModel.Width && cropInfo.Height == cropModel.Height)
                {
                    url += $"?crop={cropModel.Left},{cropModel.Top},{cropModel.Right},{cropModel.Bottom}&cropmode=percentage&width={cropInfo.Width}&height={cropInfo.Height}";
                }
                else {
                    url += $"?mode=crop&width={cropInfo.Width}&height={cropInfo.Height}&anchor={anchor}";
                }
            }
            return url;
        }
    }
}