using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace puck.core.Entities
{
    public class PuckMeta
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(2048)]
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime? Dt { get; set; }
        public string UserName { get; set; }
    }
}
