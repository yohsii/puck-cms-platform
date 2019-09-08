using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Entities
{
    public class GeneratedModel
    {
        public GeneratedModel() {
            Properties = new List<GeneratedProperty>();
        }
        [Key]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [DataType("SettingsGeneratedInherits")]
        public string Inherits { get; set; }
        public string IFullName { get; set; }
        public string IFullPath { get; set; }
        public string CName { get; set; }
        public string CFullPath { get; set; }

        public virtual ICollection<GeneratedProperty> Properties { get; set; }
    }
    public class GeneratedProperty
    {
        public GeneratedProperty() {
            Attributes = new List<GeneratedAttribute>();
        }        
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        [DataType("SettingsPropertyType")]
        public string Type { get; set; }
        
        public int ModelID { get; set; }
        [ForeignKey("ModelID")]
        public virtual GeneratedModel Model { get; set; }
        
        public virtual ICollection<GeneratedAttribute> Attributes { get; set; }
    }
    public class GeneratedAttribute
    {
        public GeneratedAttribute() {
            
        }
        [Key]
        public int ID { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public int PropertyID { get; set; }
        [ForeignKey("PropertyID")]
        public virtual GeneratedProperty Property { get; set; }
                
    }    
}
