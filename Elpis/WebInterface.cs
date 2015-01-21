using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Kayak;
using Kayak.Http;

namespace Elpis
{
    class WebInterface
    {
        public void startInterface()
        {
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;
#endif

            var scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
            var server = KayakServer.Factory.CreateHttp(new RequestDelegate(), scheduler);

            using (server.Listen(new IPEndPoint(IPAddress.Any, 8080)))
            {
                // runs scheduler on calling thread. this method will block until
                // someone calls Stop() on the scheduler.
                scheduler.Start();
            }
        }
        class SchedulerDelegate : ISchedulerDelegate
        {
            public void OnException(IScheduler scheduler, Exception e)
            {
                Debug.WriteLine("Error on scheduler.");
                e.DebugStackTrace();
            }

            public void OnStop(IScheduler scheduler)
            {

            }
        }

        class RequestDelegate : IHttpRequestDelegate
        {
            public void OnRequest(HttpRequestHead request, IDataProducer requestBody,
                IHttpResponseDelegate response)
            {
                if (request.Method.ToUpperInvariant() == "POST" && request.Uri.StartsWith("/next"))
                {
                    // when you subecribe to the request body before calling OnResponse,
                    // the server will automatically send 100-continue if the client is 
                    // expecting it.
                    bool ret = MainWindow.Next();                   

                    var body = ret?"Successfully skipped.":"You have to wait for 20 seconds to skip again.";

                    var headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", body.Length.ToString() },
                    }
                    };
                    response.OnResponse(headers, new BufferedProducer(body));
                }
                else if (request.Method.ToUpperInvariant() == "POST" && request.Uri.StartsWith("/pause"))
                {
                    MainWindow.Pause();
                    var headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                        {
                            { "Content-Type", "text/plain" },
                            { "Connection", "close" }
                        }
                    };
                    if (request.Headers.ContainsKey("Content-Length"))
                        headers.Headers["Content-Length"] = request.Headers["Content-Length"];

                    // if you call OnResponse before subscribing to the request body,
                    // 100-continue will not be sent before the response is sent.
                    // per rfc2616 this response must have a 'final' status code,
                    // but the server does not enforce it.
                    response.OnResponse(headers, requestBody);
                }
                else if (request.Method.ToUpperInvariant() == "POST" && request.Uri.StartsWith("/play"))
                {
                    MainWindow.Play();
                    var headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                        {
                            { "Content-Type", "text/plain" },
                            { "Connection", "close" }
                        }
                    };
                    if (request.Headers.ContainsKey("Content-Length"))
                        headers.Headers["Content-Length"] = request.Headers["Content-Length"];

                    // if you call OnResponse before subscribing to the request body,
                    // 100-continue will not be sent before the response is sent.
                    // per rfc2616 this response must have a 'final' status code,
                    // but the server does not enforce it.
                    response.OnResponse(headers, requestBody);
                }
                else if (request.Method.ToUpperInvariant() == "POST" && request.Uri.StartsWith("/like"))
                {
                    MainWindow.Like();
                    var headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                        {
                            { "Content-Type", "text/plain" },
                            { "Connection", "close" }
                        }
                    };
                    if (request.Headers.ContainsKey("Content-Length"))
                        headers.Headers["Content-Length"] = request.Headers["Content-Length"];

                    // if you call OnResponse before subscribing to the request body,
                    // 100-continue will not be sent before the response is sent.
                    // per rfc2616 this response must have a 'final' status code,
                    // but the server does not enforce it.
                    response.OnResponse(headers, requestBody);
                }
                else if (request.Method.ToUpperInvariant() == "POST" && request.Uri.StartsWith("/dislike"))
                {
                    MainWindow.Dislike();
                    var headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                        {
                            { "Content-Type", "text/plain" },
                            { "Connection", "close" }
                        }
                    };
                    if (request.Headers.ContainsKey("Content-Length"))
                        headers.Headers["Content-Length"] = request.Headers["Content-Length"];

                    // if you call OnResponse before subscribing to the request body,
                    // 100-continue will not be sent before the response is sent.
                    // per rfc2616 this response must have a 'final' status code,
                    // but the server does not enforce it.
                    response.OnResponse(headers, requestBody);
                }
                else if (request.Uri.StartsWith("/"))
                {
                    var body = string.Format(
                        "Hello world.\r\nHello.\r\n\r\nUri: {0}\r\nPath: {1}\r\nQuery:{2}\r\nFragment: {3}\r\n",
                        request.Uri,
                        request.Path,
                        request.QueryString,
                        request.Fragment);

                    var headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", body.Length.ToString() },
                    }
                    };
                    response.OnResponse(headers, new BufferedProducer(body));
                }
                else
                {
                    var responseBody = "The resource you requested ('" + request.Uri + "') could not be found.";
                    var headers = new HttpResponseHead()
                    {
                        Status = "404 Not Found",
                        Headers = new Dictionary<string, string>()
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
                    };
                    var body = new BufferedProducer(responseBody);

                    response.OnResponse(headers, body);
                }
            }
        }

        class BufferedProducer : IDataProducer
        {
            ArraySegment<byte> data;

            public BufferedProducer(string data) : this(data, Encoding.UTF8) { }
            public BufferedProducer(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }
            public BufferedProducer(byte[] data) : this(new ArraySegment<byte>(data)) { }
            public BufferedProducer(ArraySegment<byte> data)
            {
                this.data = data;
            }

            public IDisposable Connect(IDataConsumer channel)
            {
                // null continuation, consumer must swallow the data immediately.
                channel.OnData(data, null);
                channel.OnEnd();
                return null;
            }
        }

        class BufferedConsumer : IDataConsumer
        {
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>();
            Action<string> resultCallback;
            Action<Exception> errorCallback;

            public BufferedConsumer(Action<string> resultCallback,
        Action<Exception> errorCallback)
            {
                this.resultCallback = resultCallback;
                this.errorCallback = errorCallback;
            }
            public bool OnData(ArraySegment<byte> data, Action continuation)
            {
                // since we're just buffering, ignore the continuation. 
                // TODO: place an upper limit on the size of the buffer. 
                // don't want a client to take up all the RAM on our server! 
                buffer.Add(data);
                return false;
            }
            public void OnError(Exception error)
            {
                errorCallback(error);
            }

            public void OnEnd()
            {
                // turn the buffer into a string. 
                // 
                // (if this isn't what you want, you could skip 
                // this step and make the result callback accept 
                // List<ArraySegment<byte>> or whatever) 
                // 
                var str = "";
                if (buffer.Count > 0)
                {
                    str = buffer
                    .Select(b => Encoding.UTF8.GetString(b.Array, b.Offset, b.Count))
                    .Aggregate((result, next) => result + next);
                }
                

                resultCallback(str);
            }
        } 
    }
}
