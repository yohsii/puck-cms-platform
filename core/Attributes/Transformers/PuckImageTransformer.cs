using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Abstract;
using puck.core.Base;
using System.IO;
using puck.core.Helpers;
using SixLabors.ImageSharp;
using System.Threading.Tasks;
using puck.core.Models;

namespace puck.core.Attributes.Transformers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class PuckImageTransformer : Attribute, I_Property_Transformer<PuckImage, PuckImage>
    {
        I_Log logger;
        public void Configure(I_Log logger)
        {
            this.logger = logger;
        }
        public async Task<PuckImage> Transform(BaseModel m,string propertyName,string ukey,PuckImage p,Dictionary<string,object> dict)
        {
            try
            {
                if (p.File == null || string.IsNullOrEmpty(p.File.FileName))
                    return p;
            
                string filepath = string.Concat("~/wwwroot/Media/", m.Id, "/", m.Variant, "/", ukey, "_", p.File.FileName);
                string absfilepath =ApiHelper.MapPath(filepath);
                new FileInfo(absfilepath).Directory.Create();
                using (var stream = new FileStream(absfilepath, FileMode.Create)) {
                    p.File.CopyTo(stream);
                }
                p.Path = filepath.Replace("~/wwwroot","");
                p.Size = p.File.Length;
                p.Extension=Path.GetExtension(p.File.FileName);
                var img = Image.Load(absfilepath);
                p.Width = img.Width;
                p.Height = img.Height;
            }catch(Exception ex){
                logger.Log(ex);
            }
            finally {
                p.File = null;
            }
            return p;
        }
    }    
}