using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using puck.core.Abstract;
using puck.core.Base;
using System.IO;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using puck.Models;
using System.Configuration;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using puck.core.State;
using Microsoft.Extensions.Configuration;

namespace puck.Transformers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class PuckAzureBlobImageTransformer : Attribute, I_Property_Transformer<PuckImage, PuckImage>
    {
        string accountName; 
        string accessKey;
        string containerName;
        public void Configure(IConfiguration config) {
            this.accountName = config.GetValue<string>("AzureImageTransformer_AccountName");
            this.accessKey = config.GetValue<string>("AzureImageTransformer_AccessKey");
            this.containerName = config.GetValue<string>("AzureImageTransformer_ContainerName");
        }
        public async Task<PuckImage> Transform(BaseModel m,string propertyName,string ukey,PuckImage p)
        {
            try
            {
                if (p.File == null || string.IsNullOrEmpty(p.File.FileName))
                    return null;

                
                StorageCredentials creden = new StorageCredentials(accountName, accessKey);

                CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: false);

                CloudBlobClient client = acc.CreateCloudBlobClient();

                CloudBlobContainer cont = client.GetContainerReference(containerName);

                if(await cont.CreateIfNotExistsAsync())
                    await cont.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                string filepath = string.Concat(m.Id, "/", m.Variant, "/", ukey, "_", p.File.FileName);

                CloudBlockBlob cblob = cont.GetBlockBlobReference(filepath);
                //var stream = new MemoryStream();
                //p.File.CopyTo(stream);
                p.Size = p.File.Length.ToString();
                p.Extension = Path.GetExtension(p.File.FileName);

                using (var stream = p.File.OpenReadStream()) {
                    await cblob.UploadFromStreamAsync(stream);
                    p.Path = $"https://{accountName}.blob.core.windows.net/{containerName}/{filepath}";
                    stream.Position = 0;
                    var img = Image.Load(stream);
                    p.Width = img.Width;
                    p.Height = img.Height;
                }
                
            }catch(Exception ex){
                puck.core.State.PuckCache.PuckLog.Log(ex);
            }finally {
                p.File = null;
            }
            return p;
        }
    }    
}