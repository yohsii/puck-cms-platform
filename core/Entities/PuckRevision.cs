using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using puck.core.Base;
using puck.core.Attributes;
using Newtonsoft.Json;
using puck.core.Helpers;

namespace puck.core.Entities
{
    public class PuckRevision:BaseModel
    {
        [Key]
        [IndexSettings(Ignore=false)]
        public int RevisionID { get; set; }
        [IndexSettings(Ignore = false)]
        public bool Current { get; set; }
        [IndexSettings(Ignore = false)]
        public string Value { get; set; }
        public bool HasNoPublishedRevision { get; set; }
        public bool IsPublishedRevision { get; set; }
        public string IdPath { get; set; }
        public bool HasChildren { get; set; }
        public BaseModel ToBaseModel(bool cast=false)
        {
            try
            {
                var _t = cast ? typeof(BaseModel) : ApiHelper.ConcreteType(ApiHelper.GetTypeFromName(this.Type));
                var model = JsonConvert.DeserializeObject(this.Value, _t);
                var mod = model as BaseModel;
                mod.Id = this.Id;
                mod.ParentId = this.ParentId;
                mod.Path = this.Path;
                mod.SortOrder = this.SortOrder;
                mod.NodeName = this.NodeName;
                mod.Published = this.Published;
                mod.Type = this.Type;
                mod.TypeChain = this.TypeChain;
                mod.Variant = this.Variant;
                return mod;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
