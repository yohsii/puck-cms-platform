using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace puck.core.Models.Admin
{
    public class ForgottenPassword
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
