using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Entities
{
    public class PuckAudit
    {
        public PuckAudit() {
            this.Timestamp = DateTime.Now;
        }
        [Key]
        public int Id { get; set; }
        public Guid ContentId { get; set; }
        [MaxLength(10)]
        public string Variant { get; set; }
        [MaxLength(256)]
        public string Action { get; set; }
        [Display(Name ="User Name")]
        [MaxLength(256)]
        public string UserName { get; set; }
        public string Notes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
