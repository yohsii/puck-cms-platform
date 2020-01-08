using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Entities
{
    public class PuckInstruction
    {
        public PuckInstruction() {
            Timestamp = DateTime.Now;
        }
        [Key]
        public int Id { get; set; }
        
        [MaxLength(256)]
        public string ServerName { get; set; }
        public int Count { get; set; }
        public string InstructionKey { get; set; }
        public string InstructionDetail { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
