using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Abstract;
using puck.core.Base;
using System.IO;
using puck.Models;
using puck.core.Helpers;
using SixLabors.ImageSharp;

namespace puck.Transformers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class PuckImageTransformer : Attribute, I_Property_Transformer<PuckImage, PuckImage>
    {
        public PuckImage Transform(BaseModel m,string propertyName,string ukey,PuckImage p)
        {
            try
            {
                if (p.File == null || string.IsNullOrEmpty(p.File.FileName))
                    return null;
            
                string filepath = string.Concat("~/wwwroot/Media/", m.Id, "/", m.Variant, "/", ukey, "_", p.File.FileName);
                string absfilepath =ApiHelper.MapPath(filepath);
                new FileInfo(absfilepath).Directory.Create();
                using (var stream = new FileStream(absfilepath, FileMode.Create)) {
                    p.File.CopyTo(stream);
                }
                p.Path = filepath.Replace("~/wwwroot","");
                p.Size = p.File.Length.ToString();
                p.Extension=Path.GetExtension(p.File.FileName);
                var img = Image.Load(absfilepath);
                p.Width = img.Width;
                p.Height = img.Height;
            }catch(Exception ex){
                
            }finally {
                p.File = null;
            }
            return p;
        }
    }    
}