using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Entities;
using puck.core.Base;

namespace puck.core.Models
{
    public class RevisionCompare
    {
        public BaseModel Current { get; set; }
        public BaseModel Revision { get; set; }
        public int RevisionID { get; set; }
    }
}
