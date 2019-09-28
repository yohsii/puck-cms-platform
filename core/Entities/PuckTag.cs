using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace puck.core.Entities
{
    public class PuckTag
    {
        [Key]
        public int Id { get; set; }
        public string Category { get; set; }
        public string Tag { get; set; }
        public int Count { get; set; }
    }
}
