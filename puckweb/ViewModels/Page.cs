using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Documents;
using puck.core.Attributes;
using puck.core.Attributes.Transformers;
using puck.core.Base;
using puck.core.Constants;
using puck.core.Models;
using puck.core.Models.EditorSettings;
using puck.core.Models.EditorSettings.Attributes;

namespace puckweb.ViewModels
{
    /*You don't need this viewmodel (or any of them, although i recommend keeping ImageVM), feel free to modify/delete and make your own*/
    public class Page:BaseModel
    {
        [Display(Name = "Image Gallery", GroupName = "Images")]
        [UIHint(EditorTemplates.PuckImagePicker)]
        /*this is an editor template setting, the editor template reads this attribute to modify its behaviour. settings can be set
         using attributes or in the back office on the "settings->editor parameters" page*/
        [PuckImagePickerEditorSettings(MaxPick = 2)]
        public List<PuckPicker> ImageGallery { get; set; }

        [Display(GroupName = "Images")]
        [PuckImageTransformer()]
        public PuckImage Image { get; set; }

        [Display(GroupName ="Content")]
        [UIHint(EditorTemplates.Tags)]
        [PuckTagsEditorSettings(Category ="")]
        [IndexSettings(Analyzer=typeof(KeywordAnalyzer))]
        public List<string> Names { get; set; }

        /*NOTE, the group name sets the tab. the short name specifies a jquery selector to get the row title. 
         * if a short name isn't provided, your rows will be titled - item1, item2 etc*/
        [Display(ShortName = "[name$='Name']",GroupName ="Content")]
        [UIHint(EditorTemplates.ListEditor)]
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
        [UIHint(EditorTemplates.RichTextEditor)]
        [Display(Name="Main Content",GroupName ="Content")]
        [IndexSettings(FieldIndexSetting = Field.Index.ANALYZED, Analyzer = typeof(SnowballAnalyzer))]
        public string MainContent { get; set; }

        
        [UIHint(EditorTemplates.GoogleLongLat)]
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
        [UIHint(EditorTemplates.ListEditor)]
        public List<TestModel3> Cities { get; set; }
    }
    public class TestModel
    {
        public int Age { get; set; }
        public string Name { get; set; }
        [Display(ShortName = "[name$='Name']")]
        [UIHint(EditorTemplates.ListEditor)]
        public List<TestModel2> Test2 { get; set; }

        [Display(ShortName = "input")]
        [UIHint(EditorTemplates.ListEditor)]
        public List<string> AddressLines { get; set; }
        
    }
}