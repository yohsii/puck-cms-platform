using Lucene.Net.Support;
using Microsoft.Azure.Storage.Blob;
//using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;


namespace Lucene.Net.Store.Azure
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append only file
    /// </summary>
    public class CompositeIndexOutput : IndexOutput
    {
        private SyncDirectory _syncDirectory;
        private string _name;
        private IndexOutput _indexOutput;
        private Mutex _fileMutex;
        public Lucene.Net.Store.Directory CacheDirectory { get { return _syncDirectory.CacheDirectory; } }
        public Lucene.Net.Store.Directory PrimaryDirectory { get { return _syncDirectory.PrimaryDirectory; } }
        public CompositeIndexOutput(SyncDirectory syncDirectory, string name)
        {

            _name = name;

            _fileMutex = SyncMutexManager.GrabMutex(_name);
            _fileMutex.WaitOne();
            try
            {
                _syncDirectory = syncDirectory;
                
                // create the local cache one we will operate against...
                _indexOutput = CacheDirectory.CreateOutput(_name,IOContext.DEFAULT);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override void Flush()
        {
            _indexOutput.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _fileMutex.WaitOne();
            try
            {
                string fileName = _name;

                // make sure it's all written out
                _indexOutput.Flush();

                long originalLength = _indexOutput.Length;
                _indexOutput.Dispose();

                Stream blobStream;
                //blobStream = new StreamInput(CacheDirectory.OpenInput(fileName,IOContext.DEFAULT));

                try
                {
                    var cacheInput = CacheDirectory.OpenInput(fileName, IOContext.DEFAULT);
                    byte[] cacheInputBytes = new byte[cacheInput.Length];
                    cacheInput.ReadBytes(cacheInputBytes, 0, (int)cacheInputBytes.Length);

                    using (var primaryOutput = PrimaryDirectory.CreateOutput(fileName, IOContext.DEFAULT))
                    {
                        primaryOutput.WriteBytes(cacheInputBytes, (int)cacheInputBytes.Length);
                        primaryOutput.Flush();
                    }
                    cacheInput.Dispose();
                    // push the blobStream up to the cloud
                    //_blob.UploadFromStream(blobStream);

                    // set the metadata with the original index file properties
                    //_blob.Metadata["CachedLength"] = originalLength.ToString();

                    var cachedFilePath = Path.Combine(_syncDirectory.CacheDirectoryPath, fileName);
                    var lastModified = File.GetLastWriteTimeUtc(cachedFilePath);
                    
                    var primaryFilePath = Path.Combine(_syncDirectory.PrimaryDirectoryPath, fileName);
                    File.SetLastWriteTime(primaryFilePath,lastModified);
                    
                    Debug.WriteLine(string.Format("PUT {1} bytes to {0} in primary directory", _name, cacheInputBytes.Length));
                }
                finally
                {
                    //blobStream.Dispose();
                }

#if FULLDEBUG
                Debug.WriteLine(string.Format("CLOSED WRITESTREAM {0}", _name));
#endif
                // clean up
                _indexOutput = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override long Length
        {
            get
            {
                return _indexOutput.Length;
            }
        }

        public override void WriteByte(byte b)
        {
            _indexOutput.WriteByte(b);
        }

        public override void WriteBytes(byte[] b, int length)
        {
            _indexOutput.WriteBytes(b, length);
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            _indexOutput.WriteBytes(b, offset, length);
        }

        public /*override*/ long FilePointer
        {
            get
            {
                return _indexOutput.GetFilePointer();
            }
        }

        public override long Checksum => _indexOutput.Checksum;

        public override void Seek(long pos)
        {
            _indexOutput.Seek(pos);
        }

        public override long GetFilePointer()
        {
            return _indexOutput.GetFilePointer();
        }
    }
}
