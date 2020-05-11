using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Abstract
{
    public interface I_BaseModel
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public String NodeName { get; set; }
        public string LastEditedBy { get; set; }
        public string CreatedBy { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int Revision { get; set; }
        public string Variant { get; set; }
        public bool Published { get; set; }
        public int SortOrder { get; set; }
        public string TemplatePath { get; set; }
        public string TypeChain { get; set; }
        public string Type { get; set; }
        public List<string> References { get; set; }
    }
}
