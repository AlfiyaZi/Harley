namespace Harley.UI.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CefSharp;

    public class OwinSchemeHandlerFactory : ISchemeHandlerFactory
    {
        private readonly Func<IDictionary<string, object>, Task> _appFunc;

        public OwinSchemeHandlerFactory(Func<IDictionary<string, object>, Task> appFunc)
        {
            _appFunc = appFunc;
        }

        public ISchemeHandler Create()
        {
            return new OwinSchemeHandler(_appFunc);
        }
    }

    internal class OwinSchemeHandler : ISchemeHandler
    {
        private readonly Func<IDictionary<string, object>, Task> _appFunc;

        private static readonly Dictionary<int, string> ReasonPhrases = new Dictionary<int, string>
        {
            {200, "OK"},
            {301, "Moved Permanently"},
            {304, "Not Modified"},
            {404, "Not Found"}
        };

        public OwinSchemeHandler(Func<IDictionary<string, object>, Task> appFunc)
        {
            _appFunc = appFunc;
        }

        public bool ProcessRequestAsync(
            IRequest request,
            ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback)
        {
            IDictionary<string, string[]> requestHeaders = request.Headers.ToDictionary();
            Stream requestBody = Stream.Null;
            if(request.Body != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(request.Body);
                requestBody = new MemoryStream(bytes, 0, bytes.Length);
            }
            var stream = new MemoryStream();
            var uri = new Uri(request.Url);
            var environment = new Dictionary<string, object>
            {
                {"owin.RequestBody", requestBody},
                {"owin.RequestHeaders", requestHeaders},
                {"owin.RequestMethod", request.Method},
                {"owin.RequestPath", uri.AbsolutePath},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestProtocol", "HTTP/1.1"},
                {"owin.RequestQueryString", uri.Query},
                {"owin.RequestScheme", "HTTP/1.1"},
                {"owin.ResponseBody", new UnclosableStream(stream)},
                {"owin.ResponseHeaders", new Dictionary<string, string[]>()},
            };
            // Yucky continuation
            _appFunc.Invoke(environment).ContinueWith(task =>
            {
                string status = "200 OK";
                if(environment.ContainsKey("owin.ResponseStatusCode"))
                {
                    var statusCode = environment["owin.ResponseStatusCode"].ToString();
                    status = environment.ContainsKey("owin.ResponseReasonPhrase")
                        ? statusCode + " " + environment["owin.ResponseReasonPhrase"].ToString()
                        : statusCode + " " + ReasonPhrases[int.Parse(environment["owin.ResponseStatusCode"].ToString())];
                }
                //TODO CefSharp seems to be ignoring the status code and turning it to a 200OK :|
                response.ResponseHeaders = new NameValueCollection
                {
                    {
                        "Status Code", status
                    }
                };
                var responseHeaders = (Dictionary<string, string[]>)environment["owin.ResponseHeaders"];
                foreach(KeyValuePair<string, string[]> responseHeader in responseHeaders)
                {
                    response.ResponseHeaders.Add(responseHeader.Key, string.Join(";", responseHeader.Value));
                }
                response.MimeType = !response.ResponseHeaders.AllKeys.Contains("Content-Type") ? "text/plain" : response.ResponseHeaders["Content-Type"];
                response.ResponseStream = stream;
                stream.Position = 0;
                requestCompletedCallback();
            });
            return true;
        }

        private class UnclosableStream : Stream
        {
            private readonly Stream _inner;

            internal UnclosableStream(Stream inner)
            {
                _inner = inner;
            }

            public override bool CanRead
            {
                get { return _inner.CanRead; }
            }

            public override bool CanSeek
            {
                get { return _inner.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return _inner.CanWrite; }
            }

            public override long Length
            {
                get { return _inner.Length; }
            }

            public override long Position
            {
                get { return _inner.Position; }

                set { _inner.Position = value; }
            }

            public override bool CanTimeout
            {
                get { return _inner.CanTimeout; }
            }

            public override int ReadTimeout
            {
                get { return _inner.ReadTimeout; }
                set { _inner.ReadTimeout = value; }
            }

            public override int WriteTimeout
            {
                get { return _inner.WriteTimeout; }
                set { _inner.WriteTimeout = value; }
            }

            public override void Close()
            {}

            public new void Dispose()
            {}

            public override void Flush()
            {
                _inner.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _inner.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _inner.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _inner.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _inner.Write(buffer, offset, count);
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _inner.BeginRead(buffer, offset, count, callback, state);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _inner.BeginWrite(buffer, offset, count, callback, state);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                return _inner.EndRead(asyncResult);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                _inner.EndWrite(asyncResult);
            }

            public override int ReadByte()
            {
                return _inner.ReadByte();
            }

            public override void WriteByte(byte value)
            {
                _inner.WriteByte(value);
            }

            protected override void Dispose(bool disposing)
            {}
        }
    }

    internal static class NameValueCollectionExtensions
    {
        public static IDictionary<string, string[]> ToDictionary(this NameValueCollection nameValueCollection)
        {
            var dict = new Dictionary<string, string[]>();
            foreach (var key in nameValueCollection.AllKeys)
            {
                if(!dict.ContainsKey(key))
                {
                    dict.Add(key, new string[0]);
                }
                var strings = nameValueCollection.GetValues(key);
                if(strings == null)
                {
                    continue;
                }
                foreach(string value in  strings)
                {
                    var values = dict[key].ToList();
                    values.Add(value);
                    dict[key] = values.ToArray();
                }
            }
            return dict;
        }
    }
}