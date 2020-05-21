using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Entities
{
    public class PuckWorkflowItem
    {
        public PuckWorkflowItem() {
            this.Timestamp = DateTime.Now;
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid ContentId { get; set; }
        [MaxLength(10)]
        [Required]
        public string Variant { get; set; }
        [MaxLength(256)]
        [Required]
        public string Status { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Group { get; set; }
        public string Assignees { get; set; }
        [Display(Name ="Locked By")]
        [MaxLength(256)]
        public string LockedBy { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool Complete { get; set; }
        public DateTime? CompleteDate { get; set; }
        public DateTime Timestamp { get; set; }
        public string ViewedBy { get; set; }
    }
}
