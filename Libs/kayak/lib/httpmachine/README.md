# HttpMachine

HttpMachine is a C# HTTP request parser. It implements a state machine with [Adrian Thurston](http://www.complang.org/thurston/)'s excellent state machine compiler, [Ragel](http://www.complang.org/ragel/). Because Ragel supports C, D, Java, Ruby, it wouldn't be hard to port this library to those languages.

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the 
MIT License. See LICENSE.txt.

## Features

- HTTP/1.1 and 1.0
- Supports pipelined requests
- Tells your server if it should keep-alive
- Extracts the length of the entity body 

## Eminently-possible future features

- Support for decoding chunked transfers.
- Support for protocol upgrade.
- Support for parsing responses.

## Usage

HttpMachine provides HTTP data through callbacks. To receive these callbacks, implement the `IHttpParserHandler` interface.

    public interface IHttpParserHandler
    {
        void OnMessageBegin(HttpParser parser);
        void OnMethod(HttpParser parser, string method);
        void OnRequestUri(HttpParser parser, string requestUri);
        void OnFragment(HttpParser parser, string fragment);
        void OnQueryString(HttpParser parser, string queryString);
        void OnHeaderName(HttpParser parser, string name);
        void OnHeaderValue(HttpParser parser, string value);
        void OnHeadersEnd(HttpParser parser);
        void OnBody(HttpParser parser, ArraySegment<byte> data);
        void OnMessageEnd(HttpParser parser);
    }

Then, create an instance of `HttpParser`. Whenever you read data, execute the parser on the data. The `Execute` method returns the number of bytes successfully parsed. If value is not the same as the length of the buffer you provided, an error occurred while parsing. Make sure you provide a zero-length buffer at the end of the stream, as some callbacks may still be pending.

    var handler = new MyHttpParserHandler();
    var parser = new HttpParser(handler);
    
    var buffer = new byte[1024 /* or whatever you like */]
    
    int bytesRead;
    
    while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        if (bytesRead != parser.Execute(new ArraySegment<byte>(buffer, 0, bytesRead))
            goto error; /* or whatever you like */
    
    // ensure you get the last callbacks.
    parser.Execute(default(ArraySegment<byte>));
    
The parser has three public properties:

    // HTTP version provided in the request
    public int MajorVersion { get; }
    public int MinorVersion { get; }

    // inspects "Connection" header and HTTP version (if any) to recommend a connection behavior
    public bool ShouldKeepAlive { get; }

These properties are only guaranteed to be accurate in the `OnBody` and `OnMessageEnd` callbacks.
