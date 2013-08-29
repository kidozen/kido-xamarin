using System;
using System.Net;
using System.Net.Browser;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace KidoZen
{
    static class ExtensionsWebClient
    {
        static public Task<Response> Send_POST_GET_PUT(this Request request, Action<long[]> onProgress = null)
        {
            return Task.Factory.StartNew(() =>
            {
                var client = new KidoWebClient();
                var response = client.Send(request, onProgress);
                return response;
            });
        }

        class KidoWebClient : WebClient
        {
            Response response;
            Request request;
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpWebResponse;
            ManualResetEvent allDone = new ManualResetEvent(false);


            protected override WebRequest GetWebRequest(Uri address)
            {
                var webRequest = base.GetWebRequest(address);
                httpWebRequest = webRequest as HttpWebRequest;
                return webRequest;
            }

            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                var webResponse = base.GetWebResponse(request, result);
                httpWebResponse = webResponse as HttpWebResponse;
                return webResponse;
            }

            public Response Send(Request request, Action<long[]> onProgress = null)
            {
                try
                {
                    this.request = request;
                    response = null;
                    allDone.Reset();

                    foreach (var h in request.Headers)
                        this.Headers[h.Key] = h.Value;

                    this.UploadStringCompleted += downloadCompleted;
                    this.DownloadStringCompleted += downloadCompleted;
                    this.OpenWriteCompleted += openWriteCompleted;
                    this.WriteStreamClosed += writeStreamClosed;

                    var contentType = request.GetHeader("Content-Type");
                    if (contentType != null && contentType.Contains("application/octet-stream"))
                    {
                        this.OpenWriteAsync(request.Uri, request.Method);
                    }
                    else
                    {
                        if (request.Content.Length == 0 && request.Method=="GET")
                        {
                            this.DownloadStringAsync(request.Uri);
                        }
                        else
                        {
                            using (var reader = new StreamReader(request.Content))
                            {
                                var data = reader.ReadToEnd();
                                this.UploadStringAsync(request.Uri, request.Method, data);
                            }
                        }
                    }

                    allDone.WaitOne();
                    return response;
                }
                catch (Exception)
                {
                    throw;
                } 
                finally
                {
                    if (request != null) request.Dispose();
                    if (httpWebResponse != null) httpWebResponse.Dispose();
                }
            }

            private void writeStreamClosed(object sender, WriteStreamClosedEventArgs e)
            {
                try
                {
                    if (e.Error != null) throw e.Error;
                    
                    var headers = new Dictionary<string, string>();
                    if (this.httpWebResponse == null)
                    {
                        string content = @"{""success"":true}";
                        headers["Content-Type"] = "application/json";
                        headers["Content-Length"] = content.Length.ToString();
                        response = Response.Create(HttpStatusCode.OK, content, headers);
                    }
                    else
                    {
                        if (this.httpWebResponse.Headers != null)
                        {
                            foreach (var h in this.httpWebResponse.Headers.AllKeys)
                            {
                                headers.Add(h, this.httpWebResponse.Headers[h]);
                            }
                        }
                        response = Response.Create(this.httpWebResponse.StatusCode, "true", headers);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    allDone.Set();
                }
            }

            private void openWriteCompleted(object sender, OpenWriteCompletedEventArgs e)
            {
                request.Content.CopyTo(e.Result);
                e.Result.Close();
            }

            private void downloadCompleted(object sender, UploadStringCompletedEventArgs e)
            {
                downloadCompleted(e.Cancelled, e.Error, e.Result);
            }

            private void downloadCompleted(object sender, DownloadStringCompletedEventArgs e)
            {
                downloadCompleted(e.Cancelled, e.Error, e.Result);
            }

            private void downloadCompleted(bool cancelled, Exception error, string result)
            {
                try
                {
                    if (cancelled) return;
                    if (error != null) throw error;

                    var headers = new Dictionary<string, string>();
                    foreach (var h in this.ResponseHeaders.AllKeys)
                    {
                        headers.Add(h, this.ResponseHeaders[h]);
                    }

                    response = Response.Create(this.httpWebResponse.StatusCode, result, headers);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    allDone.Set();
                }
            }
        }
    }
}
