using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using puck.core.Constants;
using puck.core.State;
using Microsoft.AspNetCore.Http;
using puck.core.Attributes;
using Lucene.Net.Analysis.Core;
using puck.core.Models.EditorSettings;

namespace puck.core.Models
{
    public class PuckImage
    {
        public PuckImage() {

        }
        
        [UIHint("PuckReadOnly")]
        [Display(Name = "Brightness")]
        public double? Brightness { get; set; } = 0.0;
        
        [UIHint("PuckReadOnly")]
        [Display(Name = "Red")]
        public double? Red { get; set; } = 0.0;
        
        [UIHint("PuckReadOnly")]
        [Display(Name = "Blue")]
        public double? Blue { get; set; } = 0.0;
        
        [UIHint("PuckReadOnly")]
        [Display(Name = "Green")]
        public double? Green { get; set; } = 0.0;
        
        [IndexSettings(LowerCaseValue = false, FieldIndexSetting = Lucene.Net.Documents.Field.Index.NOT_ANALYZED, Analyzer = typeof(KeywordAnalyzer))]
        public string Path { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [UIHint("PuckReadOnly")]
        public long? Size {get;set;}
        [UIHint("PuckReadOnly")]
        public string Extension { get; set; }
        [UIHint("PuckReadOnly")]
        public int? Width { get; set; }
        [UIHint("PuckReadOnly")]
        public int? Height { get; set; }
        public List<CropModel> Crops { get; set; }
        public Dictionary<string, string> CropUrls { get; set; }
        
        [UIHint(EditorTemplates.Tags)]
        [TagsEditorSettings(Category = "Images")]
        [IndexSettings(Analyzer = typeof(KeywordAnalyzer))]
        public List<string> Tags { get; set; }

        public IFormFile File { get; set; }

        public string GetCropUrl(string cropAlias=null,string anchor="center") {
            var url = Path;
            if (string.IsNullOrEmpty(cropAlias))
                return url;
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
                    if (cropModel.Zoom > 1)
                        url += $"?mode=crop&width={Width*cropModel.Zoom}&height={Height*cropModel.Zoom}&crop={cropModel.CropLeft},{cropModel.CropTop},{cropModel.Right},{cropModel.Bottom}";// &cropmode=percentage&width={cropInfo.Width}&height={cropInfo.Height}";
                    else
                        url += $"?crop={cropModel.Left},{cropModel.Top},{cropModel.Right},{cropModel.Bottom}";// &cropmode=percentage&width={cropInfo.Width}&height={cropInfo.Height}";
                }
                else {
                    url += $"?mode=crop&width={cropInfo.Width}&height={cropInfo.Height}&anchor={anchor}";
                }
            }
            return url;
        }
    }
}