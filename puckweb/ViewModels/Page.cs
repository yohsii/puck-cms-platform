using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Documents;
using puck.core.Attributes;
using puck.core.Base;
using puck.core.Models;
using puck.Models;

namespace puckweb.ViewModels
{
    public class Page:BaseModel
    {
        [Display(Name = "Image Gallery", GroupName = "Images")]
        [UIHint("PuckImagePicker")]
        public List<PuckPicker> ImageGallery { get; set; }

        [Display(GroupName = "Images")]
        [UIHint("PuckImage")]
        public PuckImage Image { get; set; }
        
        [Display(ShortName ="input",GroupName ="Content")]
        [UIHint("ListEditor")]
        public List<string> Names { get; set; }

        [Display(ShortName = "[name$='Name']",GroupName ="Content")]
        [UIHint("ListEditor")]
        public List<TestModel> Test { get; set; }

        [Required]
        [Display(Name = "Keywords",GroupName ="Content")]
        [IndexSettings(FieldIndexSetting = Field.Index.ANALYZED, Analyzer = typeof(SnowballAnalyzer))]
        public string MetaKeywords { get; set; }
        
        [Required]
        [Display(Name = "Description",GroupName ="Content")]
        [DataType(DataType.MultilineText)]
        [IndexSettings(FieldIndexSetting = Field.Index.ANALYZED, Analyzer = typeof(SnowballAnalyzer))]
        public string MetaDescription { get; set; }
        
        [Required]
        [IndexSettings(FieldIndexSetting = Field.Index.ANALYZED, Analyzer = typeof(SnowballAnalyzer))]
        [Display(Description="enter a description here",GroupName ="Content")]
        public string Title { get; set; }
        
        [Required]
        [UIHint("rte")]
        [Display(Name="Main Content",GroupName ="Content")]
        [IndexSettings(FieldIndexSetting = Field.Index.ANALYZED, Analyzer = typeof(SnowballAnalyzer))]
        public string MainContent { get; set; }

        
        [UIHint("PuckGoogleLongLat")]
        [Display(GroupName ="Content")]
        public GeoPosition Location { get; set; }                
    }

    /*THE FOLLOWING CLASSES CREATED TO TEST THE LISTEDITOR */
    public class TestModel3
    {
        public string Town { get; set; }
    }
    public class TestModel2
    {
        [Required]
        public string Name { get; set; }
        [Display(ShortName = "input")]
        [UIHint("ListEditor")]
        public List<TestModel3> Cities { get; set; }
    }
    public class TestModel
    {
        public int Age { get; set; }
        public string Name { get; set; }
        [Display(ShortName = "[name$='Name']")]
        [UIHint("ListEditor")]
        public List<TestModel2> Test2 { get; set; }

        [Display(ShortName = "input")]
        [UIHint("ListEditor")]
        public List<string> AddressLines { get; set; }
    }
}