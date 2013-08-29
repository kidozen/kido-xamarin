using KidoZen.authentication;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{
    public class Files
    {
        public Uri Url { get; private set; }

        public delegate void OnDownloadProgressEventHandler(object sender, DownloadProgressArgs e);
        public event OnDownloadProgressEventHandler OnDownloadProgress;

        KZApplication app;

        internal Files(KZApplication app, Uri endpoint)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            this.app = app;
            Url = endpoint;
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="stream">File's stream</param>
        /// <param name="path">Target full path. It must contains the destination folder and the file name. (ie: /folder/subfolder/subfolder/filename)</param>
        /// <param name="timeout">Optional timeout.</param>
        /// <returns>Returns true if the upload was successful</returns>
        public async Task<ServiceEvent<bool>> Upload(Stream stream, string path, TimeSpan? timeout = null)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (path == null) throw new ArgumentNullException("path");

            string fileName;
            var targetUrl = buildUploadUrl(path, out fileName);
            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/octet-stream");
            headers.Add("x-file-name", fileName);
            var result = await targetUrl.ExecuteAsync<JToken>(app, stream, "POST", false, timeout, headers);
            return result.Clone<bool>(result.Succeed && result.Data != null && result.Data.Value<bool>("success"));
        }

        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <param name="path">Source file's URL</param>
        /// <param name="timeout">Optional timeout.</param>
        /// <returns>File's stream</returns>
        public async Task<ServiceEvent<Stream>> Download(string path, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

            Action<long[]> progress = null;
            if (OnDownloadProgress != null) progress = p => { OnDownloadProgress.Invoke(this, new DownloadProgressArgs(path, p[0], p[1])); };
            return await buildDownloadUrl(path).ExecuteAsync<Stream>(app, timeout: timeout, onProgress: progress);
        }

        /// <summary>
        /// Browse afolder
        /// </summary>
        /// <param name="path">Folder's URL. The URL must ends with character '/'</param>
        /// <returns>Folder information</returns>
        public async Task<ServiceEvent<FilesBrowseResult>> Browse(string path = "/")
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

            var targetUrl = buildBrowseUrl(path);
            return await targetUrl.ExecuteAsync<FilesBrowseResult>(app);
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="path">File's URL.</param>
        /// <returns>Returns true if the upload was successful</returns>
        public async Task<ServiceEvent<bool>> Delete(string path = "/")
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

            var targetUrl = buildDeleteUrl(path);
            var result = await targetUrl.ExecuteAsync<JToken>(app, method: "DELETE");
            return result.Clone<bool>(result.Succeed);
        }

        private Uri buildUploadUrl(string path, out string fileName)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");
            path = path.Trim().Replace(@"\", "/");
            if (!path.StartsWith("/")) path = "/" + path;

            fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("The path does not contain the file's name.", "path");

            path = Path.GetDirectoryName(path);
            
            return Url.Concat(path==null ? "/" : path.Replace(@"\","/"));
        }

        private Uri buildDownloadUrl(string path)
        {
            path = path==null ? "/" : path.Trim().Replace(@"\", "/");
            if (!path.StartsWith("/")) path = "/" + path;
            if (string.IsNullOrEmpty(Path.GetFileName(path))) throw new ArgumentException("A file name is required.", "path");

            return Url.Concat(path);
        }

        private Uri buildBrowseUrl(string path)
        {
            path = path == null ? "/" : path.Trim().Replace(@"\", "/");
            if (!path.StartsWith("/")) path = "/" + path;
            if (!path.EndsWith("/")) path += "/";

            return Url.Concat(path);
        }

        private Uri buildDeleteUrl(string path)
        {
            path = path == null ? "/" : path.Trim().Replace(@"\", "/");
            if (!path.StartsWith("/")) path = "/" + path;

            return Url.Concat(path);
        }
    }
}
