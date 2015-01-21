using System;
using System.Text;

namespace HttpMachine
{
    public class HttpParser
    {
        public object UserContext { get; set; }
        public int MajorVersion { get { return versionMajor; } }
        public int MinorVersion { get { return versionMinor; } }

        public bool ShouldKeepAlive
        {
            get
            {
                if (versionMajor > 0 && versionMinor > 0)
                    // HTTP/1.1
                    return !gotConnectionClose;
                else
                    // < HTTP/1.1
                    return gotConnectionKeepAlive;
            }
        }

        IHttpParserDelegate del;

		// necessary evil?
		StringBuilder sb;
		StringBuilder sb2;
		// Uri uri;

		int versionMajor;
		int versionMinor;
		
        int contentLength;

		// TODO make flags or something, dang
		bool inContentLengthHeader;
		bool inConnectionHeader;
		bool inTransferEncodingHeader;
		bool inUpgradeHeader;
		bool gotConnectionClose;
		bool gotConnectionKeepAlive;
		bool gotTransferEncodingChunked;
		bool gotUpgradeValue;

        int cs;
        // int mark;

        %%{

        # define actions
        machine http_parser;

		action buf {
			sb.Append((char)fc);
		}

		action clear {
			sb.Length = 0;
		}

		action buf2 {
			sb2.Append((char)fc);
		}

		action clear2 {
			if (sb2 == null)
				sb2 = new StringBuilder();
			sb2.Length = 0;
		}

		action message_begin {
			//Console.WriteLine("message_begin");
			versionMajor = 0;
			versionMinor = 9;
			contentLength = -1;

			inContentLengthHeader = false;
			inConnectionHeader = false;
			inTransferEncodingHeader = false;
			inUpgradeHeader = false;

			gotConnectionClose = false;
			gotConnectionKeepAlive = false;
			gotTransferEncodingChunked = false;
			gotUpgradeValue = false;
			del.OnMessageBegin(this);
		}
        
        action matched_absolute_uri {
            //Console.WriteLine("matched absolute_uri");
        }
        action matched_abs_path {
            //Console.WriteLine("matched abs_path");
        }
        action matched_authority {
            //Console.WriteLine("matched authority");
        }
        action matched_first_space {
            //Console.WriteLine("matched first space");
        }
        action leave_first_space {
            //Console.WriteLine("leave_first_space");
        }
        action eof_leave_first_space {
            //Console.WriteLine("eof_leave_first_space");
        }
		action matched_header { 
			//Console.WriteLine("matched header");
		}
		action matched_leading_crlf {
			//Console.WriteLine("matched_leading_crlf");
		}
		action matched_last_crlf_before_body {
			//Console.WriteLine("matched_last_crlf_before_body");
		}
		action matched_header_crlf {
			//Console.WriteLine("matched_header_crlf");
		}

		action on_method {
			del.OnMethod(this, sb.ToString());
		}
        
		action on_request_uri {
			del.OnRequestUri(this, sb.ToString());
		}

		action on_abs_path
		{
			del.OnPath(this, sb2.ToString());
		}
        
		action on_query_string
		{
			del.OnQueryString(this, sb2.ToString());
		}

        action enter_query_string {
            //Console.WriteLine("enter_query_string fpc " + fpc);
            qsMark = fpc;
        }

        action leave_query_string {
            //Console.WriteLine("leave_query_string fpc " + fpc + " qsMark " + qsMark);
            del.OnQueryString(this, new ArraySegment<byte>(data, qsMark, fpc - qsMark));
        }

		action on_fragment
		{
			del.OnFragment(this, sb2.ToString());
		}

        action enter_fragment {
            //Console.WriteLine("enter_fragment fpc " + fpc);
            fragMark = fpc;
        }

        action leave_fragment {
            //Console.WriteLine("leave_fragment fpc " + fpc + " fragMark " + fragMark);
            del.OnFragment(this, new ArraySegment<byte>(data, fragMark, fpc - fragMark));
        }

        action version_major {
			versionMajor = (char)fc - '0';
		}

		action version_minor {
			versionMinor = (char)fc - '0';
		}
		
        action header_content_length {
            if (contentLength != -1) throw new Exception("Already got Content-Length. Possible attack?");
			//Console.WriteLine("Saw content length");
			contentLength = 0;
			inContentLengthHeader = true;
        }

		action header_connection {
			//Console.WriteLine("header_connection");
			inConnectionHeader = true;
		}

		action header_connection_close {
			//Console.WriteLine("header_connection_close");
			if (inConnectionHeader)
				gotConnectionClose = true;
		}

		action header_connection_keepalive {
			//Console.WriteLine("header_connection_keepalive");
			if (inConnectionHeader)
				gotConnectionKeepAlive = true;
		}
		
		action header_transfer_encoding {
			//Console.WriteLine("Saw transfer encoding");
			inTransferEncodingHeader = true;
		}

		action header_transfer_encoding_chunked {
			if (inTransferEncodingHeader)
				gotTransferEncodingChunked = true;
		}

		action header_upgrade {
			inUpgradeHeader = true;
		}

		action on_header_name {
			del.OnHeaderName(this, sb.ToString());
		}

		action on_header_value {
			var str = sb.ToString();
			//Console.WriteLine("on_header_value '" + str + "'");
			//Console.WriteLine("inContentLengthHeader " + inContentLengthHeader);
			if (inContentLengthHeader)
				contentLength = int.Parse(str);

			inConnectionHeader = inTransferEncodingHeader = inContentLengthHeader = false;
			
			del.OnHeaderValue(this, str);
		}

        action last_crlf {
			
			if (fc == 10)
			{
				//Console.WriteLine("leave_headers contentLength = " + contentLength);
				del.OnHeadersEnd(this);

				// if chunked transfer, ignore content length and parse chunked (but we can't yet so bail)
				// if content length given but zero, read next request
				// if content length is given and non-zero, we should read that many bytes
				// if content length is not given
				//   if should keep alive, assume next request is coming and read it
				//   else read body until EOF

				if (contentLength == 0)
				{
					del.OnMessageEnd(this);
					//fhold;
					fgoto main;
				}
				else if (contentLength > 0)
				{
					//fhold;
					fgoto body_identity;
				}
				else
				{
					//Console.WriteLine("Request had no content length.");
					if (ShouldKeepAlive)
					{
						del.OnMessageEnd(this);
						//Console.WriteLine("Should keep alive, will read next message.");
						//fhold;
						fgoto main;
					}
					else
					{
						//Console.WriteLine("Not keeping alive, will read until eof. Will hold, but currently fpc = " + fpc);
						//fhold;
						fgoto body_identity_eof;
					}
				}
			}
        }

		action body_identity {
			var toRead = Math.Min(pe - p, contentLength);
			//Console.WriteLine("body_identity: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				del.OnBody(this, new ArraySegment<byte>(data, p, toRead));
				p += toRead - 1;
				contentLength -= toRead;
				//Console.WriteLine("content length is now " + contentLength);

				if (contentLength == 0)
				{
					del.OnMessageEnd(this);

					if (ShouldKeepAlive)
					{
						//Console.WriteLine("Transitioning from identity body to next message.");
						//fhold;
						fgoto main;
					}
					else
					{
						//fhold;
						fgoto dead;
					}
				}
				else
				{
					fbreak;
				}
			}
		}
		
		action body_identity_eof {
			var toRead = pe - p;
			//Console.WriteLine("body_identity_eof: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				del.OnBody(this, new ArraySegment<byte>(data, p, toRead));
				p += toRead - 1;
				fbreak;
			}
			else
			{
				del.OnMessageEnd(this);
				
				if (ShouldKeepAlive)
					fgoto main;
				else
				{
					//Console.WriteLine("body_identity_eof: going to dead");
					fhold;
					fgoto dead;
				}
			}
		}

		action enter_dead {
			throw new Exception("Parser is dead; there shouldn't be more data. Client is bogus? fpc =" + fpc);
		}

        include http "http.rl";
        
        }%%
        
        %% write data;
        
        public HttpParser(IHttpParserDelegate del)
        {
            this.del = del;
			sb = new StringBuilder();
            %% write init;
        }

        public int Execute(ArraySegment<byte> buf)
        {
            byte[] data = buf.Array;
            int p = buf.Offset;
            int pe = buf.Offset + buf.Count;
            int eof = buf.Count == 0 ? buf.Offset : -1;
            //int eof = pe;
            // mark = 0;
            
			//if (p == pe)
			//	Console.WriteLine("Parser executing on p == pe (EOF)");

            %% write exec;
            
            var result = p - buf.Offset;

			if (result != buf.Count)
			{
				Console.WriteLine("error on character " + p);
				Console.WriteLine("('" + buf.Array[p] + "')");
				Console.WriteLine("('" + (char)buf.Array[p] + "')");
			}

			return p - buf.Offset;
        }
    }
}