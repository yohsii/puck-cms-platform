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
        [Key]
        public int Id { get; set; }
        public string ServerName { get; set; }
        public int Count { get; set; }
        public string InstructionKey { get; set; }
        public string InstructionDetail { get; set; }
    }
}
