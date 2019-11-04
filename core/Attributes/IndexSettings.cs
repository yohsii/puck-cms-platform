using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using puck.core.Constants;
using Lucene.Net.Analysis;
namespace puck.core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexSettings:Attribute
    {
        private Field.Store _FieldStoreSetting = FieldSettings.FieldStoreSetting;
        public Field.Store FieldStoreSetting { 
            get{
                return _FieldStoreSetting;
            } set{_FieldStoreSetting=value;} }

        private Field.Index _FieldIndexSetting = FieldSettings.FieldIndexSetting;
        public Field.Index FieldIndexSetting {
            get {
                return _FieldIndexSetting;
            }
            set { _FieldIndexSetting = value; }
        }
        public Type Analyzer { get; set; }
        public bool Ignore { get; set; }
        //public bool KeepValueCasing { get; set; }
        public bool LowerCaseValue { get; set; }
        public bool Spatial { get; set; }
    }
}
