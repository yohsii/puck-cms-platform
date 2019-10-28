using puck.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.tests.Models
{
    public class SiteTree
    {
        public SiteTree() {
            Variants = new List<PuckRevision>();
            Children = new List<SiteTree>();
        }
        public int Level { get; set; }
        public int Branch { get; set; }
        public SiteTree Parent { get; set; }
        public List<PuckRevision> Variants { get; set; }
        public List<SiteTree> Children { get; set; }
    }
}
