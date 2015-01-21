
#line 1 "rl/HttpParser.cs.rl"
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

        
#line 335 "rl/HttpParser.cs.rl"

        
        
#line 58 "HttpParser.cs"
static readonly sbyte[] _http_parser_actions =  new sbyte [] {
	0, 1, 0, 1, 10, 1, 11, 1, 
	13, 1, 17, 1, 18, 1, 26, 1, 
	27, 1, 28, 1, 29, 1, 30, 1, 
	31, 2, 1, 0, 2, 2, 0, 2, 
	4, 11, 2, 12, 8, 2, 14, 0, 
	2, 14, 13, 2, 15, 0, 2, 15, 
	13, 2, 16, 13, 2, 19, 26, 2, 
	20, 26, 2, 21, 27, 2, 22, 27, 
	2, 23, 26, 2, 24, 27, 2, 25, 
	26, 3, 3, 2, 0, 3, 3, 15, 
	0, 3, 3, 15, 13, 3, 3, 16, 
	13, 3, 4, 1, 0, 4, 9, 1, 
	7, 0, 4, 9, 1, 7, 13, 5, 
	9, 1, 5, 7, 0, 6, 9, 1, 
	6, 3, 2, 0
};

static readonly short[] _http_parser_key_offsets =  new short [] {
	0, 0, 5, 6, 10, 15, 30, 31, 
	53, 54, 70, 78, 80, 86, 90, 94, 
	98, 102, 106, 108, 112, 116, 120, 122, 
	126, 130, 134, 137, 141, 145, 149, 153, 
	157, 159, 177, 195, 215, 233, 251, 269, 
	287, 305, 323, 339, 357, 375, 393, 409, 
	427, 445, 463, 481, 499, 517, 533, 551, 
	569, 587, 605, 623, 641, 659, 675, 693, 
	711, 729, 747, 765, 783, 801, 819, 835, 
	853, 871, 889, 907, 925, 943, 959, 960, 
	961, 962, 963, 964, 966, 967, 969, 970, 
	985, 991, 997, 1012, 1025, 1038, 1044, 1050, 
	1056, 1062, 1076, 1090, 1096, 1102, 1123, 1144, 
	1157, 1163, 1169, 1174, 1179, 1184, 1189, 1194, 
	1199, 1204, 1209, 1214, 1219, 1224, 1229, 1234, 
	1239, 1244, 1249, 1254, 1259, 1264, 1269, 1274, 
	1279, 1280, 1280, 1280, 1280, 1280, 1280
};

static readonly char[] _http_parser_trans_keys =  new char [] {
	'\u000d', '\u0041', '\u005a', '\u0061', '\u007a', '\u000a', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u000d', 
	'\u0020', '\u0021', '\u0025', '\u002f', '\u003d', '\u0040', '\u005f', '\u007e', 
	'\u0024', '\u003b', '\u0041', '\u005a', '\u0061', '\u007a', '\u000a', '\u000d', 
	'\u0021', '\u0043', '\u0054', '\u0055', '\u0063', '\u0074', '\u0075', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u000a', '\u0021', '\u003a', 
	'\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', 
	'\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0009', '\u000a', 
	'\u000d', '\u0020', '\u0043', '\u004b', '\u0063', '\u006b', '\u000a', '\u000d', 
	'\u000a', '\u000d', '\u0048', '\u004c', '\u0068', '\u006c', '\u000a', '\u000d', 
	'\u0055', '\u0075', '\u000a', '\u000d', '\u004e', '\u006e', '\u000a', '\u000d', 
	'\u004b', '\u006b', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', 
	'\u0044', '\u0064', '\u000a', '\u000d', '\u000a', '\u000d', '\u004f', '\u006f', 
	'\u000a', '\u000d', '\u0053', '\u0073', '\u000a', '\u000d', '\u0045', '\u0065', 
	'\u000a', '\u000d', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', 
	'\u0045', '\u0065', '\u000a', '\u000d', '\u0050', '\u0070', '\u000a', '\u000d', 
	'\u002d', '\u000a', '\u000d', '\u0041', '\u0061', '\u000a', '\u000d', '\u004c', 
	'\u006c', '\u000a', '\u000d', '\u0049', '\u0069', '\u000a', '\u000d', '\u0056', 
	'\u0076', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', '\u0021', 
	'\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', '\u0054', '\u006e', 
	'\u0074', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0043', '\u0063', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0054', '\u0074', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0049', 
	'\u0069', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0054', '\u0074', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u002d', '\u002e', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004c', '\u006c', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0047', '\u0067', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0054', '\u0074', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0048', '\u0068', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0052', 
	'\u0072', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0041', '\u0061', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0042', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0053', '\u0073', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0046', 
	'\u0066', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0052', '\u0072', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u002d', '\u002e', '\u003a', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0043', '\u0063', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0044', '\u0064', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0049', 
	'\u0069', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0047', '\u0067', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0050', '\u0070', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0047', 
	'\u0067', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0052', '\u0072', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0041', '\u0061', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0042', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0044', '\u0064', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', 
	'\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0048', 
	'\u0054', '\u0054', '\u0050', '\u002f', '\u0030', '\u0039', '\u002e', '\u0030', 
	'\u0039', '\u000d', '\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', 
	'\u007e', '\u0024', '\u002e', '\u0030', '\u003b', '\u0040', '\u005a', '\u0061', 
	'\u007a', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0030', 
	'\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', '\u0021', 
	'\u0023', '\u0025', '\u003d', '\u003f', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u0040', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0025', 
	'\u003d', '\u005f', '\u007e', '\u0024', '\u003b', '\u003f', '\u005a', '\u0061', 
	'\u007a', '\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', '\u007e', 
	'\u0024', '\u003b', '\u003f', '\u005a', '\u0061', '\u007a', '\u0030', '\u0039', 
	'\u0041', '\u0046', '\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', 
	'\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', 
	'\u0021', '\u0023', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u003f', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0023', 
	'\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', '\u003f', '\u005a', 
	'\u0061', '\u007a', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', 
	'\u0021', '\u0025', '\u002b', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', 
	'\u002c', '\u002d', '\u002e', '\u0030', '\u0039', '\u003a', '\u003b', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0025', '\u002b', 
	'\u003a', '\u003b', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', '\u002c', 
	'\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', 
	'\u003b', '\u003f', '\u005a', '\u0061', '\u007a', '\u0030', '\u0039', '\u0041', 
	'\u0046', '\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', 
	'\u0066', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', 
	'\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', 
	'\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', 
	(char) 0
};

static readonly sbyte[] _http_parser_single_lengths =  new sbyte [] {
	0, 1, 1, 0, 1, 9, 1, 10, 
	1, 4, 8, 2, 6, 4, 4, 4, 
	4, 4, 2, 4, 4, 4, 2, 4, 
	4, 4, 3, 4, 4, 4, 4, 4, 
	2, 6, 6, 8, 6, 6, 6, 6, 
	6, 6, 4, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 4, 6, 6, 
	6, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 6, 4, 6, 
	6, 6, 6, 6, 6, 4, 1, 1, 
	1, 1, 1, 0, 1, 0, 1, 7, 
	0, 0, 9, 7, 7, 0, 0, 0, 
	0, 8, 8, 0, 0, 9, 11, 7, 
	0, 0, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 0, 0, 0, 0, 0, 0
};

static readonly sbyte[] _http_parser_range_lengths =  new sbyte [] {
	0, 2, 0, 2, 2, 3, 0, 6, 
	0, 6, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 6, 5, 6, 
	6, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 5, 6, 6, 
	6, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 6, 0, 0, 
	0, 0, 0, 1, 0, 1, 0, 4, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 6, 5, 3, 
	3, 3, 2, 2, 2, 2, 2, 2, 
	2, 2, 2, 2, 2, 2, 2, 2, 
	2, 2, 2, 2, 2, 2, 2, 2, 
	0, 0, 0, 0, 0, 0, 0
};

static readonly short[] _http_parser_index_offsets =  new short [] {
	0, 0, 4, 6, 9, 13, 26, 28, 
	45, 47, 58, 67, 70, 77, 82, 87, 
	92, 97, 102, 105, 110, 115, 120, 123, 
	128, 133, 138, 142, 147, 152, 157, 162, 
	167, 170, 183, 196, 211, 224, 237, 250, 
	263, 276, 289, 300, 313, 326, 339, 351, 
	364, 377, 390, 403, 416, 429, 440, 453, 
	466, 479, 492, 505, 518, 531, 543, 556, 
	569, 582, 595, 608, 621, 634, 647, 658, 
	671, 684, 697, 710, 723, 736, 747, 749, 
	751, 753, 755, 757, 759, 761, 763, 765, 
	777, 781, 785, 798, 809, 820, 824, 828, 
	832, 836, 848, 860, 864, 868, 884, 901, 
	912, 916, 920, 924, 928, 932, 936, 940, 
	944, 948, 952, 956, 960, 964, 968, 972, 
	976, 980, 984, 988, 992, 996, 1000, 1004, 
	1008, 1010, 1011, 1012, 1013, 1014, 1015
};

static readonly byte[] _http_parser_indicies =  new byte [] {
	0, 2, 2, 1, 3, 1, 4, 4, 
	1, 5, 6, 6, 1, 7, 8, 9, 
	10, 11, 9, 9, 9, 9, 9, 12, 
	12, 1, 13, 1, 14, 15, 16, 17, 
	18, 16, 17, 18, 15, 15, 15, 15, 
	15, 15, 15, 15, 1, 19, 1, 20, 
	21, 20, 20, 20, 20, 20, 20, 20, 
	20, 1, 23, 1, 1, 23, 24, 25, 
	24, 25, 22, 1, 27, 26, 1, 27, 
	28, 29, 28, 29, 26, 1, 27, 30, 
	30, 26, 1, 27, 31, 31, 26, 1, 
	27, 32, 32, 26, 1, 27, 33, 33, 
	26, 1, 27, 34, 34, 26, 1, 35, 
	26, 1, 27, 36, 36, 26, 1, 27, 
	37, 37, 26, 1, 27, 38, 38, 26, 
	1, 39, 26, 1, 27, 40, 40, 26, 
	1, 27, 41, 41, 26, 1, 27, 42, 
	42, 26, 1, 27, 43, 26, 1, 27, 
	44, 44, 26, 1, 27, 45, 45, 26, 
	1, 27, 46, 46, 26, 1, 27, 47, 
	47, 26, 1, 27, 48, 48, 26, 1, 
	49, 26, 20, 21, 50, 50, 20, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	21, 51, 51, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 52, 53, 
	52, 53, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 20, 21, 54, 54, 20, 
	20, 20, 20, 20, 20, 20, 20, 1, 
	20, 21, 55, 55, 20, 20, 20, 20, 
	20, 20, 20, 20, 1, 20, 21, 56, 
	56, 20, 20, 20, 20, 20, 20, 20, 
	20, 1, 20, 21, 57, 57, 20, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	21, 58, 58, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 59, 59, 
	20, 20, 20, 20, 20, 20, 20, 20, 
	1, 20, 60, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 61, 61, 
	20, 20, 20, 20, 20, 20, 20, 20, 
	1, 20, 21, 62, 62, 20, 20, 20, 
	20, 20, 20, 20, 20, 1, 20, 21, 
	63, 63, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 20, 64, 20, 21, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	21, 65, 65, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 66, 66, 
	20, 20, 20, 20, 20, 20, 20, 20, 
	1, 20, 21, 67, 67, 20, 20, 20, 
	20, 20, 20, 20, 20, 1, 20, 21, 
	68, 68, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 20, 21, 69, 69, 20, 
	20, 20, 20, 20, 20, 20, 20, 1, 
	20, 21, 70, 70, 20, 20, 20, 20, 
	20, 20, 20, 20, 1, 20, 71, 20, 
	20, 20, 20, 20, 20, 20, 20, 1, 
	20, 21, 72, 72, 20, 20, 20, 20, 
	20, 20, 20, 20, 1, 20, 21, 73, 
	73, 20, 20, 20, 20, 20, 20, 20, 
	20, 1, 20, 21, 74, 74, 20, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	21, 75, 75, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 76, 76, 
	20, 20, 20, 20, 20, 20, 20, 20, 
	1, 20, 21, 77, 77, 20, 20, 20, 
	20, 20, 20, 20, 20, 1, 20, 21, 
	78, 78, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 20, 79, 20, 21, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	21, 80, 80, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 81, 81, 
	20, 20, 20, 20, 20, 20, 20, 20, 
	1, 20, 21, 82, 82, 20, 20, 20, 
	20, 20, 20, 20, 20, 1, 20, 21, 
	83, 83, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 20, 21, 84, 84, 20, 
	20, 20, 20, 20, 20, 20, 20, 1, 
	20, 21, 85, 85, 20, 20, 20, 20, 
	20, 20, 20, 20, 1, 20, 21, 86, 
	86, 20, 20, 20, 20, 20, 20, 20, 
	20, 1, 20, 21, 87, 87, 20, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	88, 20, 20, 20, 20, 20, 20, 20, 
	20, 1, 20, 21, 89, 89, 20, 20, 
	20, 20, 20, 20, 20, 20, 1, 20, 
	21, 90, 90, 20, 20, 20, 20, 20, 
	20, 20, 20, 1, 20, 21, 91, 91, 
	20, 20, 20, 20, 20, 20, 20, 20, 
	1, 20, 21, 92, 92, 20, 20, 20, 
	20, 20, 20, 20, 20, 1, 20, 21, 
	93, 93, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 20, 21, 94, 94, 20, 
	20, 20, 20, 20, 20, 20, 20, 1, 
	20, 95, 20, 20, 20, 20, 20, 20, 
	20, 20, 1, 96, 1, 97, 1, 98, 
	1, 99, 1, 100, 1, 101, 1, 102, 
	1, 103, 1, 104, 1, 105, 106, 107, 
	108, 107, 107, 107, 107, 107, 107, 107, 
	1, 109, 109, 109, 1, 107, 107, 107, 
	1, 110, 111, 112, 113, 114, 112, 115, 
	112, 112, 112, 112, 112, 1, 116, 117, 
	118, 119, 118, 118, 118, 118, 118, 118, 
	1, 120, 121, 122, 123, 122, 122, 122, 
	122, 122, 122, 1, 124, 124, 124, 1, 
	122, 122, 122, 1, 125, 125, 125, 1, 
	112, 112, 112, 1, 126, 127, 128, 129, 
	130, 128, 128, 128, 128, 128, 128, 1, 
	131, 132, 133, 134, 135, 133, 133, 133, 
	133, 133, 133, 1, 136, 136, 136, 1, 
	133, 133, 133, 1, 105, 106, 107, 108, 
	137, 107, 107, 107, 107, 107, 137, 137, 
	107, 137, 137, 1, 105, 106, 107, 108, 
	137, 138, 107, 107, 107, 107, 107, 107, 
	137, 137, 137, 137, 1, 105, 106, 138, 
	139, 138, 138, 138, 138, 138, 138, 1, 
	140, 140, 140, 1, 138, 138, 138, 1, 
	5, 141, 141, 1, 5, 142, 142, 1, 
	5, 143, 143, 1, 5, 144, 144, 1, 
	5, 145, 145, 1, 5, 146, 146, 1, 
	5, 147, 147, 1, 5, 148, 148, 1, 
	5, 149, 149, 1, 5, 150, 150, 1, 
	5, 151, 151, 1, 5, 152, 152, 1, 
	5, 153, 153, 1, 5, 154, 154, 1, 
	5, 155, 155, 1, 5, 156, 156, 1, 
	5, 157, 157, 1, 5, 158, 158, 1, 
	5, 159, 159, 1, 5, 160, 160, 1, 
	5, 161, 161, 1, 5, 162, 162, 1, 
	5, 1, 163, 164, 1, 163, 165, 1, 
	0
};

static readonly byte[] _http_parser_trans_targs =  new byte [] {
	2, 0, 4, 3, 4, 5, 106, 6, 
	78, 87, 88, 90, 101, 7, 8, 9, 
	33, 54, 71, 131, 9, 10, 11, 10, 
	12, 23, 11, 6, 13, 19, 14, 15, 
	16, 17, 18, 6, 20, 21, 22, 6, 
	24, 25, 26, 27, 28, 29, 30, 31, 
	32, 6, 34, 35, 36, 43, 37, 38, 
	39, 40, 41, 42, 10, 44, 45, 46, 
	47, 48, 49, 50, 51, 52, 53, 10, 
	55, 56, 57, 58, 59, 60, 61, 62, 
	63, 64, 65, 66, 67, 68, 69, 70, 
	10, 72, 73, 74, 75, 76, 77, 10, 
	79, 80, 81, 82, 83, 84, 85, 86, 
	6, 6, 78, 87, 88, 89, 6, 78, 
	90, 91, 95, 97, 6, 78, 92, 93, 
	6, 78, 92, 93, 94, 96, 6, 78, 
	98, 91, 99, 6, 78, 98, 91, 99, 
	100, 102, 103, 104, 105, 107, 108, 109, 
	110, 111, 112, 113, 114, 115, 116, 117, 
	118, 119, 120, 121, 122, 123, 124, 125, 
	126, 127, 128, 132, 134, 133
};

static readonly sbyte[] _http_parser_trans_actions =  new sbyte [] {
	31, 0, 89, 5, 25, 34, 1, 98, 
	98, 93, 93, 109, 103, 0, 0, 25, 
	25, 25, 25, 17, 1, 13, 25, 0, 
	25, 25, 1, 15, 1, 1, 1, 1, 
	1, 1, 1, 67, 1, 1, 1, 58, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 61, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 55, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 52, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	64, 1, 1, 1, 1, 1, 1, 70, 
	0, 0, 0, 0, 0, 9, 0, 11, 
	0, 7, 7, 1, 1, 1, 40, 40, 
	28, 37, 28, 37, 85, 85, 73, 73, 
	49, 49, 28, 28, 28, 28, 81, 81, 
	73, 77, 73, 46, 46, 28, 43, 28, 
	28, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 19, 0, 21
};

static readonly sbyte[] _http_parser_from_state_actions =  new sbyte [] {
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 23
};

static readonly sbyte[] _http_parser_eof_actions =  new sbyte [] {
	0, 0, 0, 0, 0, 3, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 21, 0
};

const int http_parser_start = 1;
const int http_parser_first_final = 131;
const int http_parser_error = 0;

const int http_parser_en_main = 1;
const int http_parser_en_body_identity = 129;
const int http_parser_en_body_identity_eof = 133;
const int http_parser_en_dead = 130;


#line 338 "rl/HttpParser.cs.rl"
        
        public HttpParser(IHttpParserDelegate del)
        {
            this.del = del;
			sb = new StringBuilder();
            
#line 557 "HttpParser.cs"
	{
	cs = http_parser_start;
	}

#line 344 "rl/HttpParser.cs.rl"
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

            
#line 578 "HttpParser.cs"
	{
	sbyte _klen;
	short _trans;
	int _acts;
	int _nacts;
	short _keys;

	if ( p == pe )
		goto _test_eof;
	if ( cs == 0 )
		goto _out;
_resume:
	_acts = _http_parser_from_state_actions[cs];
	_nacts = _http_parser_actions[_acts++];
	while ( _nacts-- > 0 ) {
		switch ( _http_parser_actions[_acts++] ) {
	case 31:
#line 329 "rl/HttpParser.cs.rl"
	{
			throw new Exception("Parser is dead; there shouldn't be more data. Client is bogus? fpc =" + p);
		}
	break;
#line 601 "HttpParser.cs"
		default: break;
		}
	}

	_keys = _http_parser_key_offsets[cs];
	_trans = (short)_http_parser_index_offsets[cs];

	_klen = _http_parser_single_lengths[cs];
	if ( _klen > 0 ) {
		short _lower = _keys;
		short _mid;
		short _upper = (short) (_keys + _klen - 1);
		while (true) {
			if ( _upper < _lower )
				break;

			_mid = (short) (_lower + ((_upper-_lower) >> 1));
			if ( data[p] < _http_parser_trans_keys[_mid] )
				_upper = (short) (_mid - 1);
			else if ( data[p] > _http_parser_trans_keys[_mid] )
				_lower = (short) (_mid + 1);
			else {
				_trans += (short) (_mid - _keys);
				goto _match;
			}
		}
		_keys += (short) _klen;
		_trans += (short) _klen;
	}

	_klen = _http_parser_range_lengths[cs];
	if ( _klen > 0 ) {
		short _lower = _keys;
		short _mid;
		short _upper = (short) (_keys + (_klen<<1) - 2);
		while (true) {
			if ( _upper < _lower )
				break;

			_mid = (short) (_lower + (((_upper-_lower) >> 1) & ~1));
			if ( data[p] < _http_parser_trans_keys[_mid] )
				_upper = (short) (_mid - 2);
			else if ( data[p] > _http_parser_trans_keys[_mid+1] )
				_lower = (short) (_mid + 2);
			else {
				_trans += (short)((_mid - _keys)>>1);
				goto _match;
			}
		}
		_trans += (short) _klen;
	}

_match:
	_trans = (short)_http_parser_indicies[_trans];
	cs = _http_parser_trans_targs[_trans];

	if ( _http_parser_trans_actions[_trans] == 0 )
		goto _again;

	_acts = _http_parser_trans_actions[_trans];
	_nacts = _http_parser_actions[_acts++];
	while ( _nacts-- > 0 )
	{
		switch ( _http_parser_actions[_acts++] )
		{
	case 0:
#line 55 "rl/HttpParser.cs.rl"
	{
			sb.Append((char)data[p]);
		}
	break;
	case 1:
#line 59 "rl/HttpParser.cs.rl"
	{
			sb.Length = 0;
		}
	break;
	case 2:
#line 63 "rl/HttpParser.cs.rl"
	{
			sb2.Append((char)data[p]);
		}
	break;
	case 3:
#line 67 "rl/HttpParser.cs.rl"
	{
			if (sb2 == null)
				sb2 = new StringBuilder();
			sb2.Length = 0;
		}
	break;
	case 4:
#line 73 "rl/HttpParser.cs.rl"
	{
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
	break;
	case 5:
#line 91 "rl/HttpParser.cs.rl"
	{
            //Console.WriteLine("matched absolute_uri");
        }
	break;
	case 6:
#line 94 "rl/HttpParser.cs.rl"
	{
            //Console.WriteLine("matched abs_path");
        }
	break;
	case 7:
#line 97 "rl/HttpParser.cs.rl"
	{
            //Console.WriteLine("matched authority");
        }
	break;
	case 8:
#line 100 "rl/HttpParser.cs.rl"
	{
            //Console.WriteLine("matched first space");
        }
	break;
	case 9:
#line 103 "rl/HttpParser.cs.rl"
	{
            //Console.WriteLine("leave_first_space");
        }
	break;
	case 11:
#line 112 "rl/HttpParser.cs.rl"
	{
			//Console.WriteLine("matched_leading_crlf");
		}
	break;
	case 12:
#line 122 "rl/HttpParser.cs.rl"
	{
			del.OnMethod(this, sb.ToString());
		}
	break;
	case 13:
#line 126 "rl/HttpParser.cs.rl"
	{
			del.OnRequestUri(this, sb.ToString());
		}
	break;
	case 14:
#line 131 "rl/HttpParser.cs.rl"
	{
			del.OnPath(this, sb2.ToString());
		}
	break;
	case 15:
#line 136 "rl/HttpParser.cs.rl"
	{
			del.OnQueryString(this, sb2.ToString());
		}
	break;
	case 16:
#line 151 "rl/HttpParser.cs.rl"
	{
			del.OnFragment(this, sb2.ToString());
		}
	break;
	case 17:
#line 165 "rl/HttpParser.cs.rl"
	{
			versionMajor = (char)data[p] - '0';
		}
	break;
	case 18:
#line 169 "rl/HttpParser.cs.rl"
	{
			versionMinor = (char)data[p] - '0';
		}
	break;
	case 19:
#line 173 "rl/HttpParser.cs.rl"
	{
            if (contentLength != -1) throw new Exception("Already got Content-Length. Possible attack?");
			//Console.WriteLine("Saw content length");
			contentLength = 0;
			inContentLengthHeader = true;
        }
	break;
	case 20:
#line 180 "rl/HttpParser.cs.rl"
	{
			//Console.WriteLine("header_connection");
			inConnectionHeader = true;
		}
	break;
	case 21:
#line 185 "rl/HttpParser.cs.rl"
	{
			//Console.WriteLine("header_connection_close");
			if (inConnectionHeader)
				gotConnectionClose = true;
		}
	break;
	case 22:
#line 191 "rl/HttpParser.cs.rl"
	{
			//Console.WriteLine("header_connection_keepalive");
			if (inConnectionHeader)
				gotConnectionKeepAlive = true;
		}
	break;
	case 23:
#line 197 "rl/HttpParser.cs.rl"
	{
			//Console.WriteLine("Saw transfer encoding");
			inTransferEncodingHeader = true;
		}
	break;
	case 24:
#line 202 "rl/HttpParser.cs.rl"
	{
			if (inTransferEncodingHeader)
				gotTransferEncodingChunked = true;
		}
	break;
	case 25:
#line 207 "rl/HttpParser.cs.rl"
	{
			inUpgradeHeader = true;
		}
	break;
	case 26:
#line 211 "rl/HttpParser.cs.rl"
	{
			del.OnHeaderName(this, sb.ToString());
		}
	break;
	case 27:
#line 215 "rl/HttpParser.cs.rl"
	{
			var str = sb.ToString();
			//Console.WriteLine("on_header_value '" + str + "'");
			//Console.WriteLine("inContentLengthHeader " + inContentLengthHeader);
			if (inContentLengthHeader)
				contentLength = int.Parse(str);

			inConnectionHeader = inTransferEncodingHeader = inContentLengthHeader = false;
			
			del.OnHeaderValue(this, str);
		}
	break;
	case 28:
#line 227 "rl/HttpParser.cs.rl"
	{
			
			if (data[p] == 10)
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
					{cs = 1; if (true) goto _again;}
				}
				else if (contentLength > 0)
				{
					//fhold;
					{cs = 129; if (true) goto _again;}
				}
				else
				{
					//Console.WriteLine("Request had no content length.");
					if (ShouldKeepAlive)
					{
						del.OnMessageEnd(this);
						//Console.WriteLine("Should keep alive, will read next message.");
						//fhold;
						{cs = 1; if (true) goto _again;}
					}
					else
					{
						//Console.WriteLine("Not keeping alive, will read until eof. Will hold, but currently fpc = " + fpc);
						//fhold;
						{cs = 133; if (true) goto _again;}
					}
				}
			}
        }
	break;
	case 29:
#line 272 "rl/HttpParser.cs.rl"
	{
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
						{cs = 1; if (true) goto _again;}
					}
					else
					{
						//fhold;
						{cs = 130; if (true) goto _again;}
					}
				}
				else
				{
					{p++; if (true) goto _out; }
				}
			}
		}
	break;
	case 30:
#line 305 "rl/HttpParser.cs.rl"
	{
			var toRead = pe - p;
			//Console.WriteLine("body_identity_eof: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				del.OnBody(this, new ArraySegment<byte>(data, p, toRead));
				p += toRead - 1;
				{p++; if (true) goto _out; }
			}
			else
			{
				del.OnMessageEnd(this);
				
				if (ShouldKeepAlive)
					{cs = 1; if (true) goto _again;}
				else
				{
					//Console.WriteLine("body_identity_eof: going to dead");
					p--;
					{cs = 130; if (true) goto _again;}
				}
			}
		}
	break;
#line 971 "HttpParser.cs"
		default: break;
		}
	}

_again:
	if ( cs == 0 )
		goto _out;
	if ( ++p != pe )
		goto _resume;
	_test_eof: {}
	if ( p == eof )
	{
	int __acts = _http_parser_eof_actions[cs];
	int __nacts = _http_parser_actions[__acts++];
	while ( __nacts-- > 0 ) {
		switch ( _http_parser_actions[__acts++] ) {
	case 10:
#line 106 "rl/HttpParser.cs.rl"
	{
            //Console.WriteLine("eof_leave_first_space");
        }
	break;
	case 30:
#line 305 "rl/HttpParser.cs.rl"
	{
			var toRead = pe - p;
			//Console.WriteLine("body_identity_eof: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				del.OnBody(this, new ArraySegment<byte>(data, p, toRead));
				p += toRead - 1;
				{p++; if (true) goto _out; }
			}
			else
			{
				del.OnMessageEnd(this);
				
				if (ShouldKeepAlive)
					{cs = 1; if (true) goto _again;}
				else
				{
					//Console.WriteLine("body_identity_eof: going to dead");
					p--;
					{cs = 130; if (true) goto _again;}
				}
			}
		}
	break;
#line 1020 "HttpParser.cs"
		default: break;
		}
	}
	}

	_out: {}
	}

#line 359 "rl/HttpParser.cs.rl"
            
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