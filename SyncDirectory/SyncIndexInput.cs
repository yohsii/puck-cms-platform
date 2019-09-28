//using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Lucene.Net.Store.Azure
{
    /// <summary>
    /// Implements IndexInput semantics for a read only blob
    /// </summary>
    public class CompositeIndexInput : IndexInput
    {
        private SyncDirectory _syncDirectory;
        private string _name;

        private IndexInput _indexInput;
        private Mutex _fileMutex;

        public Lucene.Net.Store.Directory CacheDirectory { get { return _syncDirectory.CacheDirectory; } }
        public Lucene.Net.Store.Directory PrimaryDirectory { get { return _syncDirectory.PrimaryDirectory; } }
        public CompositeIndexInput(SyncDirectory azuredirectory, string name,string resourceDescription):base(resourceDescription)
        {
            _name = name;

#if FULLDEBUG
            Debug.WriteLine(String.Format("opening {0} ", _name));
#endif
            _fileMutex = SyncMutexManager.GrabMutex(_name);
            _fileMutex.WaitOne();
            try
            {
                _syncDirectory = azuredirectory;
                
                var fileName = _name;

                var fFileNeeded = false;
                if (!CacheDirectory.FileExists(fileName))
                {
                    fFileNeeded = true;
                }
                else
                {
                    long cachedLength = CacheDirectory.FileLength(fileName);
                    long primaryLength = PrimaryDirectory.FileLength(fileName);
                    if (cachedLength != primaryLength)
                        fFileNeeded = true;
                    else
                    {

                        // cachedLastModifiedUTC was not ouputting with a date (just time) and the time was always off
                        var cachedFilePath = Path.Combine(_syncDirectory.CacheDirectoryPath,fileName);
                        var primaryFilePath = Path.Combine(_syncDirectory.PrimaryDirectoryPath, fileName);
                        var cachedLastModified = File.GetLastWriteTimeUtc(cachedFilePath);
                        var primaryLastModified = File.GetLastWriteTimeUtc(primaryFilePath);
                        
                        if (cachedLastModified != primaryLastModified)
                        {
                            var timeSpan = primaryLastModified.Subtract(cachedLastModified);
                            if (timeSpan.TotalSeconds > 1)
                                fFileNeeded = true;
                            else
                            {
#if FULLDEBUG
                                Debug.WriteLine(timeSpan.TotalSeconds);
#endif
                                // file not needed
                            }
                        }
                    }
                }

                // if the file does not exist
                // or if it exists and it is older then the lastmodified time in the blobproperties (which always comes from the blob storage)
                if (fFileNeeded)
                {
                    var primaryInput = PrimaryDirectory.OpenInput(fileName, IOContext.DEFAULT);
                    byte[] primaryInputBytes = new byte[primaryInput.Length];
                    primaryInput.ReadBytes(primaryInputBytes, 0, (int)primaryInputBytes.Length);

                    using (var cachedOutput = CacheDirectory.CreateOutput(fileName, IOContext.DEFAULT))
                    {
                        cachedOutput.WriteBytes(primaryInputBytes, (int)primaryInputBytes.Length);
                        cachedOutput.Flush();
                    }
                    primaryInput.Dispose();

                    var cachedFilePath = Path.Combine(_syncDirectory.CacheDirectoryPath, fileName);
                    var primaryFilePath = Path.Combine(_syncDirectory.PrimaryDirectoryPath, fileName);
                    var primaryLastModified = File.GetLastWriteTimeUtc(primaryFilePath);
                    File.SetLastWriteTimeUtc(cachedFilePath,primaryLastModified);

                    //using (var fileStream = _azureDirectory.CreateCachedOutputAsStream(fileName))
                    //{

                    //    // get the blob
                    //    _blob.DownloadToStream(fileStream);

                    //    fileStream.Flush();
                    //    Debug.WriteLine(string.Format("GET {0} RETREIVED {1} bytes", _name, fileStream.Length));
                    //}


                    // and open it as an input 
                    _indexInput = CacheDirectory.OpenInput(fileName,IOContext.DEFAULT);
                }
                else
                {
#if FULLDEBUG
                    Debug.WriteLine(String.Format("Using cached file for {0}", _name));
#endif

                    // open the file in read only mode
                    _indexInput = CacheDirectory.OpenInput(fileName,IOContext.DEFAULT);
                }
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public CompositeIndexInput(CompositeIndexInput cloneInput,string resourceDescription):base(resourceDescription)
        {
            _fileMutex = SyncMutexManager.GrabMutex(cloneInput._name);
            _fileMutex.WaitOne();

            try
            {
#if FULLDEBUG
                Debug.WriteLine(String.Format("Creating clone for {0}", cloneInput._name));
#endif
                _syncDirectory = cloneInput._syncDirectory;
                _indexInput = cloneInput._indexInput.Clone() as IndexInput;
            }
            catch (Exception)
            {
                // sometimes we get access denied on the 2nd stream...but not always. I haven't tracked it down yet
                // but this covers our tail until I do
                Debug.WriteLine(String.Format("Dagnabbit, falling back to memory clone for {0}", cloneInput._name));
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override byte ReadByte()
        {
            return _indexInput.ReadByte();
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            _indexInput.ReadBytes(b, offset, len);
        }

        public override void Seek(long pos)
        {
            _indexInput.Seek(pos);
        }

        protected override void Dispose(bool disposing)
        {
            _fileMutex.WaitOne();
            try
            {
#if FULLDEBUG
                Debug.WriteLine(String.Format("CLOSED READSTREAM local {0}", _name));
#endif
                _indexInput.Dispose();
                _indexInput = null;
                _syncDirectory = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override long Length => _indexInput.Length;

        public override System.Object Clone()
        {
            IndexInput clone = null;
            try
            {
                _fileMutex.WaitOne();
                CompositeIndexInput input = new CompositeIndexInput(this,"clone");
                clone = (IndexInput)input;
            }
            catch (System.Exception err)
            {
                Debug.WriteLine(err.ToString());
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
            Debug.Assert(clone != null);
            return clone;
        }

        public override long GetFilePointer()
        {
            return _indexInput.GetFilePointer();
        }
    }
}
