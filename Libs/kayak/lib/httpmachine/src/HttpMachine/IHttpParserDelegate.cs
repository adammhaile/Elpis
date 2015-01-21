using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMachine
{
    public interface IHttpParserDelegate
    {
        void OnMessageBegin(HttpParser parser);
        void OnMethod(HttpParser parser, string method);
        void OnRequestUri(HttpParser parser, string requestUri);
        void OnPath(HttpParser parser, string path);
        void OnFragment(HttpParser parser, string fragment);
        void OnQueryString(HttpParser parser, string queryString);
        void OnHeaderName(HttpParser parser, string name);
        void OnHeaderValue(HttpParser parser, string value);
        void OnHeadersEnd(HttpParser parser);
        void OnBody(HttpParser parser, ArraySegment<byte> data);
        void OnMessageEnd(HttpParser parser);
    }
}
