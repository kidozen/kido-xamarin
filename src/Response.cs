using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{

    internal class Response : IDisposable
    {
        public MemoryStream Body { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }

        private Response()
        {
        }

        public static Response Create(HttpStatusCode statusCode, string data, IDictionary<string, string> headers = null)
        {
            var res = new Response
            {
                StatusCode = statusCode,
                Body = new MemoryStream(),
                Headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
            };

            if (!string.IsNullOrEmpty(data))
            {
                var bytes = UTF8Encoding.UTF8.GetBytes(data);
                res.Body.Write(bytes, 0, bytes.Length);
                res.Body.Flush();
                res.Body.Seek(0, SeekOrigin.Begin);
            }

            if (headers != null)
            {
                foreach (var h in headers)
                    res.Headers.Add(h.Key, h.Value);
            }

            return res;
        }

        public static async Task<Response> Create(HttpStatusCode statusCode, Stream body = null, IDictionary<string, string> headers = null)
        {
            var res = new Response {
                StatusCode = statusCode,
                Body = new MemoryStream(),
                Headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
            };

            if (body != null)
            {
                await body.CopyToAsync(res.Body);
                await res.Body.FlushAsync();
                res.Body.Seek(0, SeekOrigin.Begin);
            }

            if (headers != null)
            {
                foreach (var h in headers)
                    res.Headers.Add(h.Key, h.Value);
            }

            return res;
        }

        public void Dispose()
        {
            if (Body != null) Body.Dispose();
        }

        public string GetHeader(string name)
        {
            string header = null;
            Headers.TryGetValue(name, out header);
            return header;
        }
    }
}