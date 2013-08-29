using KidoZen.authentication;
using Newtonsoft.Json;
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
    static class ExtensionsNet
    {
        static public Task<Response> Send_POST_GET_PUT(this Request request, Action<long[]> onProgress = null)
        {
            return request.Send_OTHERS(onProgress);
        }

        static public async Task<Response> Send_OTHERS(this Request request, Action<long[]> onProgress = null)
        {
            Response response = null;
            try
            {
#if NET45
				var req= (HttpWebRequest)WebRequest.Create(request.Uri);
#else
				var req= new HttpWebRequest(request.Uri);
#endif
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, ssl) => {
                    return true;
                };

                req.Method = request.Method;
                req.AddHeaders(request.Headers);
                if (request.Content != null && request.Content.Length > 0)
                {
                    var reqStream = await Task.Factory.FromAsync<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream, null);
                    await request.Content.CopyToAsync(reqStream);
                    await reqStream.FlushAsync();
                }

                var res = (HttpWebResponse)await Task.Factory.FromAsync<WebResponse>(req.BeginGetResponse, req.EndGetResponse, null);
                var resStream = default(Stream);
                if (res.ContentLength > 0)
                {
                    resStream = res.GetResponseStream();
                }
                
                response = await Response.Create(res.StatusCode, resStream);
                foreach (var name in res.Headers.AllKeys)
                {
                    response.Headers.Add(name, res.Headers[name]);
                }
                return response;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        static private void AddHeader(this HttpWebRequest req, string name, string value)
        {
            switch (name.ToLower())
            {
                case "accept": req.Accept = value; break;
                case "content-type": req.ContentType = value; break;
#if WINDOWS_PHONE
                case "content-length": req.ContentLength = long.Parse(value); break;
                case "user-agent": req.UserAgent = value; break;
#elif NET45
                case "expect": req.Expect = value; break;
                case "host": req.Host = value; break;
                case "referer": req.Referer = value; break;
                case "transfer-encoding": req.TransferEncoding = value; break;
                case "user-agent": req.UserAgent = value; break;
                case "date": req.Date = DateTime.Parse(value); break;
                case "connection": req.Connection = value; break;
#endif
                default:
                    req.Headers[name] = value;
                    break;
            }
        }

        static private void AddHeaders(this HttpWebRequest req, Dictionary<string, string> headers)
        {
            foreach (var h in headers)
            {
                req.AddHeader(h.Key, h.Value);
            }
        }
    }
}
