using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KidoZen.authentication;

namespace KidoZen
{
    static class Extensions
    {
        #region JSON serialization

        static private JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        });

        public static string SerializeJson<T>(this T data)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, data);
            }
            return sb.ToString();
        }

        public static void SerializeJson<T>(this T data, TextWriter writer)
        {
            serializer.Serialize(writer, data);
        }

        public static JToken ToJToken<T>(this T data)
        {
            var jtoken = data as JToken;
            if (jtoken == null)
                jtoken = JToken.FromObject(data, serializer);
            return jtoken;
        }

        public static T Convert<T>(this JToken jtoken)
        {
            T t = default(T);
            if (jtoken != null)
                t = jtoken.ToObject<T>(serializer);
            return t;
        }

        #endregion

        internal static Uri ValueUri(this JObject source, string prop)
        {
            Uri uri = null;
            if (source == null) 
            {
                throw new ArgumentNullException("source");
            }

            var val = source.Value<string>(prop);
            if (string.IsNullOrWhiteSpace(val) || !Uri.TryCreate(val, UriKind.Absolute, out uri))
            {
                throw new InvalidCastException("Invalid URI string");
            }

            return uri;
        }

        internal static Uri Concat(this Uri source, string segments)
        {
            if (string.IsNullOrWhiteSpace(segments)) return new Uri(source.AbsoluteUri);

            var path = source.AbsolutePath;
            var pathWithSep = path.Length > 0 && path.EndsWith("/");
            var segmenstWithSep = segments.Substring(0, 1) == "/";

            if (pathWithSep)
            {
                if (segmenstWithSep)
                    path += segments.Substring(1);
                else
                    path += segments;
            }
            else
            {
                if (segmenstWithSep)
                    path += segments;
                else
                    path += "/" + segments;
            }

            return new Uri(source, path + source.Query);
        }

        internal static async Task<ServiceEvent<T>> ExecuteAsync<T>(this Uri uri, KZApplication app, string content, string contentType, string method = "GET", bool cache = false, TimeSpan? timeout = null, Dictionary<string, string> headers = null, UseToken useToken = UseToken.Application, Action<long[]> onProgress = null, bool cors = false)
        {
            MemoryStream    stream = null;
            TextWriter      writer = null;

            if (!string.IsNullOrWhiteSpace(content))
            {
                stream = new MemoryStream();
                writer = new StreamWriter(stream);

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    if (headers == null) headers = new Dictionary<string, string>();
                    headers["Content-Type"] = contentType;
                }
                await writer.WriteAsync(content);
                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);
            }

            var result = await uri.ExecuteAsync<T>(app, stream, method, cache, timeout, headers, useToken, onProgress, cors);

            if (writer==null) writer.Dispose();
            if (stream==null) stream.Dispose();
            
            return result;
        }

        internal static async Task<ServiceEvent<T>> ExecuteAsync<T>(this Uri uri, KZApplication app, JToken content, string method = "GET", bool cache = false, TimeSpan? timeout = null, Dictionary<string, string> headers = null, UseToken useToken = UseToken.Application, Action<long[]> onProgress = null, bool cors = false)
        {
            MemoryStream    stream = null;
            TextWriter      writer = null;

            if (content!=null)
            {
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                
                content.SerializeJson(writer);
                await writer.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);

                if (headers == null) headers = new Dictionary<string, string>();
                headers["Content-Type"] = "application/json";
            }

            var result = await uri.ExecuteAsync<T>(app, stream, method, cache, timeout, headers, useToken, onProgress, cors);

            if (writer != null) writer.Dispose();
            if (stream != null) writer.Dispose();

            return result;
        }

        internal static async Task<ServiceEvent<T>> ExecuteAsync<T>(this Uri uri, KZApplication app, Stream content = null, string method = "GET", bool cache = false, TimeSpan? timeout = null, Dictionary<string, string> headers = null, UseToken useToken = UseToken.Application, Action<long[]> onProgress = null, bool cors = false)
        {
            Request request = null;
            Response response = null;
            if (headers==null) headers = new Dictionary<string,string>();

            try
            {
                // Does the URL require a no cache?
                if (!cache)
                {
                    uri = addNoCache(uri);
                    headers.Add("Cache-Control", "no-cache");
                    headers.Add("Pragma", "no-cache");
                }

				//**** Passive Auth HotFix ****
				if (app.PassiveAuthenticationInformation!=null) {
					headers["Authorization"] = "WRAP access_token=\"" + app.PassiveAuthenticationInformation["access_token"] + "\"";
				}
				else {
					// Adds authentication's header
					if (useToken != UseToken.None && app != null && app.Authentication != null && app.Authentication.User != null)
					{
						addAuthenticationHeader(app, useToken, headers);
					}				
                }

                request = await Request.Create(uri, method.ToUpper(), content, headers, timeout);
                response = (method=="POST" || method=="GET" || method=="PUT")?
                    await request.Send_POST_GET_PUT(onProgress) :
                    await request.Send_OTHERS(onProgress);

                // Is token expired?
                if (response.StatusCode == HttpStatusCode.Unauthorized && app.User != null)
                {
                    // Refresh token if it is expired
                    var authHeader = response.Headers["WWW-Authenticate"];
                    if (!string.IsNullOrWhiteSpace(authHeader))
                    {
                        var realm = authHeader
                            .Split(',')
                            .Where(r => r.StartsWith("error="))
                            .FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(realm))
                        {
                            var message = realm.Split('=')[1].Trim();
                            if (string.Compare(message, "\"Token is expired\"", StringComparison.CurrentCultureIgnoreCase) == 0)
                            {
								//**** Passive Auth HotFix ****
								if (app.PassiveAuthenticationInformation!=null) {

								}
								else {
	                                // Do refresh tokens
	                                app.Authentication.RemoveFromCache(app.User.Credential.UserName, app.User.Credential.Password, app.User.Provider);
	                                await app.Authentication.Authenticate(app.User.Credential.UserName, app.User.Credential.Password, app.User.Provider);

	                                // Set new auth header
	                                addAuthenticationHeader(app, useToken, request.Headers);
	                                request.Content.Seek(0, SeekOrigin.Begin);
	                                
	                                // Send request
	                                response = (method == "POST" || method == "GET" || method == "PUT") ?
	                                    await request.Send_POST_GET_PUT(onProgress) :
	                                    await request.Send_OTHERS(onProgress);
								}
							}
                        }
                    }
                }

                // Process response
                var evt = new ServiceEvent<T>();
                evt.StatusCode = response.StatusCode;
                evt.Headers = response.Headers;

                var read = 0L;
                var total = response.Body == null ? 0 : response.Body.Length;

                if (typeof(T) == typeof(Stream))
                {
                    // Download the body as stream an send progress information

                    // Sends initial progress notification
                    if (onProgress != null) onProgress(new[] { read, total });

                    // Creates the stream that will be returned to the client
                    dynamic result = new MemoryStream();
                    if (total > 0)
                    {
                        // Copies the response body's stream
                        var buffer = new byte[4096];
                        var bytesRead = await response.Body.ReadAsync(buffer, 0, 4096);
                        while (bytesRead > 0)
                        {
                            result.WriteAsync(buffer, 0, bytesRead);
                            if (onProgress != null)
                            {
                                read += bytesRead;
                                onProgress(new[] { read, total });
                            }
                            bytesRead = await response.Body.ReadAsync(buffer, 0, 4096);
                        }
                        // Rewinds the stream
                        result.Seek(0, SeekOrigin.Begin);
                    }
                    evt.Data = result;
                }
                else if (typeof(T) == typeof(object))
                {
                    using (var stream = response.Body)
                    {
                        using (var reader = new StreamReader(stream, UTF8Encoding.UTF8))
                        {
                            dynamic data = await reader.ReadToEndAsync();
                            evt.Data = data;
                        }
                    }
                }
                else if (response.Headers.ContainsKey("Content-Type") && response.Headers["Content-Type"].Contains("application/json"))
                {
                    using (var reader = new StreamReader(response.Body, Encoding.UTF8))
                    {
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            evt.Data = serializer.Deserialize<T>(jsonReader);
                        }
                    }
                }
                return evt;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (request != null) request.Dispose();
                if (response != null) response.Dispose();
            }
        }

        static private void addAuthenticationHeader(KZApplication app, UseToken useToken, Dictionary<string, string> headers)
        {
            Token token = null;
            switch (useToken)
            {
                case UseToken.Application: token = app.Authentication.User.TokenApplication; break;
                case UseToken.Marketplace: token = app.Authentication.User.TokenMarketplace; break;
                case UseToken.ServiceBus: token = app.Authentication.User.TokenServiceBus; break;
            }

            if (token != null)
            {
                headers["Authorization"] = "WRAP access_token=\"" + token.Value + "\"";
            }
        }

        static private Uri addNoCache(Uri uri)
        {
            var noCache = (string.IsNullOrWhiteSpace(uri.Query) ? "?" : "&") + "kidoNoCache=" + DateTime.UtcNow.Ticks.ToString();
            return new Uri(uri.AbsoluteUri + noCache);
        }
    }
}
