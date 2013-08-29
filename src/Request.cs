using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{
    internal class Request : IDisposable
    {
        public Uri Uri { get; private set; }
        public string Method { get; private set; }
        public TimeSpan? Timeout { get; private set; }
        public MemoryStream Content { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }

        private Request()
        {
        }

        static public async Task<Request> Create(Uri uri, string method, Stream content, IDictionary<string, string> headers, TimeSpan? timeout)
        {
            if (string.IsNullOrWhiteSpace(method)) method = "GET";

            var req = new Request
            {
                Uri = uri,
                Headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase), //InvariantCultureIgnoreCase
                Timeout = timeout,
                Method = method,
                Content = new MemoryStream()
            };

            if (headers != null)
            {
                foreach (var h in headers)
                    req.Headers.Add(h.Key, h.Value);
            }
            
            if (content != null)
            {
                await content.CopyToAsync(req.Content);
                await req.Content.FlushAsync();
                req.Content.Seek(0, SeekOrigin.Begin);
            }

            var length = req.Content.Length;
            if (length == 0)
            {
                if (headers.ContainsKey("Content-Type")) headers.Remove("Content-Type");
                if (headers.ContainsKey("Content-Length")) headers.Remove("Content-Length");
            }
            else
            {
                if (!headers.ContainsKey("Content-Type")) throw new Exception("'Content-Type' HTTP header is missing.");
                headers["Content-Length"] = length.ToString();
            }

            if (!headers.ContainsKey("Accept")) headers["Accept"] = "*/*";
            return req;
        }

        public void Dispose()
        {
            if (Content != null) Content.Dispose();
        }

        public string GetHeader(string name)
        {
            string header = null;
            Headers.TryGetValue(name, out header);
            return header;
        }
    }
}