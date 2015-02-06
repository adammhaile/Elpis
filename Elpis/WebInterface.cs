/*
 * Copyright 2015 - Alexey Seliverstov
 * email: alexey.seliverstov.dev@gmail.com
 *
 * This file is part of Elpis.
 * Elpis is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * Elpis is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with Elpis. If not, see http://www.gnu.org/licenses/.
*/
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
using PandoraSharp;
using System.Web.Script.Serialization;

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

            using (server.Listen(new IPEndPoint(IPAddress.Any, 35747)))
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
                if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/next"))
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
                else if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/pause"))
                {
                    MainWindow.Pause();
                    var body = "Paused.";

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
                else if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/play"))
                {
                    MainWindow.Play();
                    var body = "Playing.";

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
                else if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/like"))
                {
                    MainWindow.Like();
                    var body = "Like";
                    if (MainWindow.GetCurrentSong().Loved)
                        body = "Liked";

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
                else if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/dislike"))
                {
                    MainWindow.Dislike();
                    var body = "Disliked.";

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
                else if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/currentsong"))
                {
                    Song s = MainWindow.GetCurrentSong();
                    var body = new JavaScriptSerializer().Serialize(s);

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
                else if (request.Method.ToUpperInvariant() == "GET" && request.Uri.StartsWith("/connect"))
                {
                    var body = "true";

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
