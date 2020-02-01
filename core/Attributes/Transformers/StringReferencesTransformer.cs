using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Models;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property,Inherited =true,AllowMultiple =true)]
    public class StringReferencesTransformer : Attribute, I_Property_Transformer<object, object>
    {
        private static Regex aHrefRegex = new Regex("<a\\s+(?:[^>]*?\\s+)?(?:href='(?<href>.*?)')|(?:href=\"(?<href>.*?)\")[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex imgSrcRegex = new Regex("<img\\s+(?:[^>]*?\\s+)?(?:src='(?<src>.*?)')|(?:src=\"(?<src>.*?)\")[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string[] Properties { get; set; }
        private IConfiguration Config { get; set; }
        private string AzureAccountName { get; set; }
        private string AzureContainerName { get; set; }
        private const string AzureBlobDomainPart = "blob.core.windows.net";

        public void Configure(IConfiguration config)
        {
            this.Config = config;
            this.AzureAccountName = config.GetValue<string>("AzureImageTransformer_AccountName");
            this.AzureContainerName = config.GetValue<string>("AzureImageTransformer_ContainerName");
        }

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

                var hrefs = aHrefRegex
                    .Matches(str)
                    .OfType<Match>()
                    .Select(x => x.Groups["href"].Value)
                    .Where(x=> x.StartsWith("/"))
                    .ToList();

                var srcs = imgSrcRegex
                    .Matches(str)
                    .OfType<Match>()
                    .Select(x => x.Groups["src"].Value)
                    .Where(x =>
                        x.StartsWith("/")
                        || (!string.IsNullOrEmpty(AzureAccountName) &&
                            !string.IsNullOrEmpty(AzureContainerName)
                            && (x.StartsWith($"http://{AzureAccountName}.{AzureBlobDomainPart}/{AzureContainerName}/")
                                || x.StartsWith($"https://{AzureAccountName}.{AzureBlobDomainPart}/{AzureContainerName}/")
                                )
                            )
                    )
                    .ToList();

                var references = new List<string>();
                references.AddRange(hrefs);
                references.AddRange(srcs);

                foreach (var reference in references)
                {
                    var referenceWithoutQuery = reference.IndexOf("?") > -1 ? reference.Substring(0, reference.IndexOf("?")) : reference;

                    if (referenceWithoutQuery.StartsWith("http://") || referenceWithoutQuery.StartsWith("https://")) {
                        referenceWithoutQuery = new Uri(referenceWithoutQuery).AbsolutePath;
                    }

                    if (string.IsNullOrEmpty(referenceWithoutQuery))
                        continue;

                    var key = $"href_references_{referenceWithoutQuery}";
                    if (!dict.ContainsKey(key))
                    {
                        m.References.Add($"{referenceWithoutQuery}");
                        dict.Add(key, true);
                    }
                }
            }
            return obj;
        }
    }
}
