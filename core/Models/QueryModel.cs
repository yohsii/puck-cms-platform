using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace puck.core.Models
{
    public class QueryModel
    {
        public string Type { get; set; }
        public string Query { get; set; }
        public List<List<string>> Include { get; set; }
        public string Sorts { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Implements { get; set; }
    }

    public class QueryResult { 
        public List<ExpandoObject> Results { get; set; }
        public int Total { get; set; }
    }
}
