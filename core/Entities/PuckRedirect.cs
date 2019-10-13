using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace puck.core.Entities
{
    public class PuckRedirect
    {
        [Key]
        public int Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Type { get; set; }
    }
}
