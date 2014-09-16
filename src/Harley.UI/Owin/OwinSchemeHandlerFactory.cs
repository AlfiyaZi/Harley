namespace Harley.UI.Owin
{
    using System;
    using System.Collections.Generic;
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
            { 200, "OK"},
            { 301, "Moved Permanently"},
            { 304, "Not Modified"},
            { 404, "Not Found"}
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
            Dictionary<string, string[]> requestHeaders = request
                .Headers
                .ToDictionary(header => header.Key, header => new[] { header.Value });
            Stream requestBody = Stream.Null;
            if (request.Body != null)
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
                {"owin.ResponseBody", stream},
                {"owin.ResponseHeaders", new Dictionary<string, string[]>()},
            };
            // Yucky continuation
            _appFunc.Invoke(environment).ContinueWith(task =>
            {
                string status = "200 OK";
                if (environment.ContainsKey("owin.ResponseStatusCode"))
                {
                    var statusCode = environment["owin.ResponseStatusCode"].ToString();
                    status = environment.ContainsKey("owin.ResponseReasonPhrase")
                        ? statusCode + " " + environment["owin.ResponseReasonPhrase"].ToString()
                        : statusCode + " " + ReasonPhrases[int.Parse(environment["owin.ResponseStatusCode"].ToString())];
                }
                //TODO CefSharp seems to be ignoring the status code and turning it to a 200OK :|
                response.ResponseHeaders = new Dictionary<string, string>
                {
                    {
                        "Status Code", status
                    }
                };
                var responseHeaders = (Dictionary<string, string[]>)environment["owin.ResponseHeaders"];
                foreach (KeyValuePair<string, string[]> responseHeader in responseHeaders)
                {
                    response.ResponseHeaders.Add(responseHeader.Key, string.Join(";", responseHeader.Value));
                }
                response.MimeType = !response.ResponseHeaders.ContainsKey("Content-Type") ? "text/plain" : response.ResponseHeaders["Content-Type"];
                response.ResponseStream = stream;
                stream.Position = 0;
                requestCompletedCallback();
            });
            return true;
        }
    }
}