using System;

namespace KidoZen
{
    public class DownloadProgressArgs : EventArgs
    {
        internal DownloadProgressArgs(string path, long bytesDownloaded, long bytesTotal) 
        { 
            BytesDownloaded = bytesDownloaded;
            BytesTotal = bytesTotal;
            Path = path;
        }

        public long BytesDownloaded { get; private set; }
        public long BytesTotal { get; private set; }
        public string Path { get; private set; }
    }
}
