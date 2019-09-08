using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Models
{
    public class GeneratedViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string BaseClass { get; set; }

        public List<GeneratedViewProperty> Properties { get; set; }
    }

    public class GeneratedViewProperty
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool Analyze { get; set; }

        public string Analyzer { get; set; }

        public bool Store { get; set; }

        public bool KeepCasing { get; set; }

        public bool Ignore { get; set; }

        public Dictionary<string, List<GeneratedViewAttribute>> Attributes { get; set; }

    }

    public class GeneratedViewAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }

}
