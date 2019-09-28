using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Lucene.Net.Store.Azure
{
    public class SyncDirectory : Directory
    {
        private Directory _cacheDirectory;
        private Directory _primaryDirectory;
        private LockFactory _lockFactory = new NativeFSLockFactory();
        public override LockFactory LockFactory => _lockFactory;
        public string CacheDirectoryPath { get; set; }
        public string CatalogPath { get; set; }
        public string PrimaryDirectoryPath { get; set; }
        /// <summary>
        /// Create an SyncDirectory
        /// </summary>
        /// <param name="storageAccount">storage account to use</param>
        /// <param name="containerName">name of container (folder in blob storage)</param>
        /// <param name="cacheDirectory">local Directory object to use for local cache</param>
        /// <param name="rootFolder">path of the root folder inside the container</param>
        public SyncDirectory(
            string primaryDirectoryPath,
            string cacheDirectoryPath
            )
        {
            CacheDirectoryPath = cacheDirectoryPath;
            PrimaryDirectoryPath = primaryDirectoryPath;
            
            _initDirectories();
        }

        
        public void ClearCache()
        {
            foreach (string file in _cacheDirectory.ListAll())
            {
                _cacheDirectory.DeleteFile(file);
            }
        }
        public Directory PrimaryDirectory
        {
            get
            {
                return _primaryDirectory;
            }
            set
            {
                _primaryDirectory = value;
            }
        }
        public Directory CacheDirectory
        {
            get
            {
                return _cacheDirectory;
            }
            set
            {
                _cacheDirectory = value;
            }
        }

        private void _initDirectories()
        {
            var cacheDir = new DirectoryInfo(CacheDirectoryPath);
            if (!cacheDir.Exists)
                cacheDir.Create();

            var primaryDir = new DirectoryInfo(PrimaryDirectoryPath);
            if (!primaryDir.Exists)
                primaryDir.Create();
            
            _cacheDirectory = FSDirectory.Open(CacheDirectoryPath);
            _primaryDirectory = FSDirectory.Open(PrimaryDirectoryPath);
            
           
        }

        /// <summary>Returns an array of strings, one for each file in the directory. </summary>
        public override String[] ListAll()
        {
            return _primaryDirectory.ListAll();
        }

        /// <summary>Returns true if a file with the given name exists. </summary>
        public override bool FileExists(String name)
        {
            // this always comes from the server
            try
            {
                return _primaryDirectory.FileExists(name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Removes an existing file in the directory. </summary>
        public override void DeleteFile(System.String name)
        {
            // We're going to try to remove this from the cache directory first,
            // because the IndexFileDeleter will call this file to remove files 
            // but since some files will be in use still, it will retry when a reader/searcher
            // is refreshed until the file is no longer locked. So we need to try to remove 
            // from local storage first and if it fails, let it keep throwing the IOExpception
            // since that is what Lucene is expecting in order for it to retry.
            // If we remove the main storage file first, then this will never retry to clean out
            // local storage because the FileExist method will always return false.
            try
            {
                if (_cacheDirectory.FileExists(name))
                {
                    _cacheDirectory.DeleteFile(name);
                }
                
                //if we've made it this far then the cache directly file has been successfully removed so now we'll do the master
                if (_primaryDirectory.FileExists(name))
                {
                    _primaryDirectory.DeleteFile(name);
                }
            }
            catch (IOException ex)
            {
                // This will occur because this file is locked, when this is the case, we don't really want to delete it from the master either because
                // if we do that then this file will never get removed from the cache folder either! This is based on the Deletion Policy which the
                // IndexFileDeleter uses. We could implement our own one of those to deal with this scenario too but it seems the easiest way it to just 
                // let this throw so Lucene will retry when it can and when that is successful we'll also clear it from the master
                throw;
            }
        }


        /// <summary>Returns the length of a file in the directory. </summary>
        public override long FileLength(String name)
        {
            return _primaryDirectory.FileLength(name);
        }

        /// <summary>Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file. 
        /// </summary>
        public override IndexOutput CreateOutput(System.String name,IOContext context)
        {
            return new CompositeIndexOutput(this, name);
        }

        /// <summary>Returns a stream reading an existing file. </summary>
        public override IndexInput OpenInput(System.String name,IOContext context)
        {
            try
            {
                return new CompositeIndexInput(this, name, "syncDirectory");
            }
            catch (Exception err)
            {
                throw new FileNotFoundException(name, err);
            }
        }

        /// <summary>Construct a {@link Lock}.</summary>
        /// <param name="name">the name of the lock file
        /// </param>
        public override Lock MakeLock(System.String name)
        {
            return _primaryDirectory.MakeLock(name);
        }

        public override void ClearLock(string name)
        {
            _primaryDirectory.ClearLock(name);
            _cacheDirectory.ClearLock(name);
        }

        /// <summary>Closes the store. </summary>
        protected override void Dispose(bool disposing)
        {
            
        }

        public StreamInput OpenCachedInputAsStream(string name)
        {
            return new StreamInput(CacheDirectory.OpenInput(name,IOContext.DEFAULT));
        }

        public StreamOutput CreateCachedOutputAsStream(string name)
        {
            return new StreamOutput(CacheDirectory.CreateOutput(name,IOContext.DEFAULT));
        }

        public StreamInput OpenPrimaryInputAsStream(string name)
        {
            return new StreamInput(PrimaryDirectory.OpenInput(name, IOContext.DEFAULT));
        }

        public StreamOutput CreatePrimaryOutputAsStream(string name)
        {
            return new StreamOutput(PrimaryDirectory.CreateOutput(name, IOContext.DEFAULT));
        }

        public override void Sync(ICollection<string> names)
        {
            
        }

        public override void SetLockFactory(LockFactory lockFactory)
        {
            _lockFactory = lockFactory;
        }
    }

}
