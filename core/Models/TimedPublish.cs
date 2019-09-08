using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Models
{
    public class TimedPublish
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Variant { get; set; }
        public List<Variant> Variants { get; set; }
        public DateTime? PublishAt { get; set; }
        public DateTime? UnpublishAt { get; set; }
        public List<string> PublishDescendantVariants { get; set; }
        public List<string> UnpublishDescendantVariants { get; set; }
    }
}
