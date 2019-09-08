using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace puck.core.Models
{
    public class LogIn
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name="Persistant Cookie")]
        public bool PersistentCookie { get; set; }        
    }
}
