using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Abstract;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using puck.core.Base;
using System.Web;

namespace puck.core.Helpers
{
    public class PreviewHelper<TModel> where TModel : BaseModel
    {
        static string namePattern = @"(?:[A-Za-z0-9]*\()?[A-Za-z0-9]\.([A-Za-z0-9.]*)";
        static string nameArrayPattern = @"\.get_Item\(\d\)";
        static string paramPattern = @"((?:[a-zA-Z0-9]+\.?)+)\)";
        static string queryPattern = @"^\(*""(.*)""\s";
        static string fieldPattern = @"@";
        static string dateFormat = "yyyyMMddHHmmss";

        //regexes compiled on startup and reused since they will be used frequently
        static Regex nameRegex = new Regex(namePattern, RegexOptions.Compiled);
        static Regex nameArrayRegex = new Regex(nameArrayPattern, RegexOptions.Compiled);
        static Regex paramRegex = new Regex(paramPattern, RegexOptions.Compiled);
        static Regex queryRegex = new Regex(queryPattern, RegexOptions.Compiled);
        static Regex fieldRegex = new Regex(fieldPattern, RegexOptions.Compiled);

        private static string getName(string str)
        {
            //((exp.Body as PropertyExpression)).Member.Name
            str = nameArrayRegex.Replace(str, "");
            var match = nameRegex.Match(str);
            string result = match.Groups[1].Value;
            //result = result.ToLower();
            return result;
        }

        public string Field(params Expression<Func<TModel, object>>[] exps)
        {
            var fieldNames = string.Empty;
            foreach (var exp in exps) {
                fieldNames += getName(exp.Body.ToString())+",";
            }
            return $"data-puck-field={fieldNames.TrimEnd(',')} ";
        }

        public bool IsPreviewPage() {
            return HttpContext.Current?.Request?.GetUri()?.AbsolutePath?.TrimEnd('/')?.ToLower()?.EndsWith("/puck/preview/previewguid")??false;
        }

    }        
}
