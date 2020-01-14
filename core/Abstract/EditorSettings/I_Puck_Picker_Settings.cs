using puck.core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Abstract.EditorSettings
{
    public interface I_Puck_Picker_Settings
    {
        string StartPathId { get; set; }
        int MaxPick { get; set; }
        //string SelectionType { get; set; }
        bool AllowUnpublished { get; set; }
        //bool AllowDuplicates { get; set; }
        List<PuckPicker> StartPath { get; set; }
        public string AllowedTypes { get; set; }
        public Type[] Types { get; set; }
    }
}
