using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Models
{
    public class ConfigContainer
    {
        public string EnvironmentName { get; set; }
        public string Name { get; set; }
        public IConfiguration Config { get; set; }
    }
}
