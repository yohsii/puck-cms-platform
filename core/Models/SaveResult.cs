using puck.core.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Models
{
    public class SaveResult
    {
        public List<BaseModel> ItemsToIndex { get; set; }
        public string Message { get; set; }
    }
}
