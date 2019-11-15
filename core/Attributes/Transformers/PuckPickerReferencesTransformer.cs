using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Models;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property)]
    public class PuckPickerReferencesTransformer : Attribute, I_Property_Transformer<List<PuckPicker>, List<PuckPicker>>
    {
        public async Task<List<PuckPicker>> Transform(BaseModel m, string propertyName, string ukey, List<PuckPicker> references,Dictionary<string,object> dict)
        {
            if (references == null) return references;
            
            if (m.References == null)
                m.References = new List<string>();

            if (dict == null)
                dict = new Dictionary<string, object>();

            foreach (var reference in references) {
                var key = $"{reference.Id}{reference.Variant ?? ""}";
                if (!dict.ContainsKey(key))
                {
                    m.References.Add($"{reference.Id}_{reference.Variant.ToLower()}");
                    dict.Add(key,true);
                }
            }

            return references;
        }
    }
}
