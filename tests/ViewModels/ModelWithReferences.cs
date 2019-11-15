using puck.core.Base;
using puck.core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.tests.ViewModels
{
    public class ModelWithReferences:BaseModel
    {
        public List<PuckPicker> NewsItems { get; set; }
        public List<PuckPicker> Images { get; set; }
    }
}
