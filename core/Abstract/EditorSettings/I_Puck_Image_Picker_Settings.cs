using puck.core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace puck.core.Abstract.EditorSettings
{
    public interface I_Puck_Image_Picker_Settings
    {
        string StartPathId { get; set; }
        int MaxPick { get; set; }
        List<PuckPicker> StartPath { get; set; }
    }
}
