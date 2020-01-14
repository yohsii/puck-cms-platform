using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Models;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property)]
    public class StringReferencesTransformer : Attribute, I_Property_Transformer<object, object>
    {
        private static Regex hrefRegex = new Regex("<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string[] Properties { get; set; }
        public async Task<object> Transform(BaseModel m, string propertyName, string ukey, object obj,Dictionary<string,object> dict)
        {
            if (m.References == null)
                m.References = new List<string>();

            if (dict == null)
                dict = new Dictionary<string, object>();

            if (Properties==null || !Properties.Any()) return obj;

            var properties = obj.GetType().GetProperties().Where(x => Properties.Contains(x.Name) && x.PropertyType == typeof(string)).ToList();

            foreach (var prop in properties)
            {
                var str = prop.GetValue(obj) as string;

                if (str == null) continue;

                var hrefs = hrefRegex.Matches(str).OfType<Match>().Select(x => x.Groups["href"].Value).Where(x => x.StartsWith("/")).ToList();

                foreach (var reference in hrefs)
                {
                    var key = $"href_references_{reference}";
                    if (!dict.ContainsKey(key))
                    {
                        m.References.Add($"{reference}");
                        dict.Add(key, true);
                    }
                }
            }
            return obj;
        }
    }
}
