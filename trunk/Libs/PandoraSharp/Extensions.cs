/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of PandoraSharp.
 * PandoraSharp is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * PandoraSharp is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with PandoraSharp. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;

namespace PandoraSharp
{
    public static class DateTimeExtensions
    {
        public static int ToEpochTime(this DateTime time)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int) time.Subtract(epoch).TotalSeconds;
        }
    }

    public static class StringExtensions
    {
        private static readonly Dictionary<char, string> encChars =
            new Dictionary<char, string>
                {
                    {'&', "&amp;"},
                    {'\'', "&apos;"},
                    {'\"', "&quot;"},
                    {'<', "&lt;"},
                    {'>', "&gt;"},
                };

        private static readonly Dictionary<string, char> decChars =
            new Dictionary<string, char>
                {
                    {"&amp;", '&'},
                    {"&apos;", '\''},
                    {"&quot;", '\"'},
                    {"&lt;", '<'},
                    {"&gt;", '>'},
                };

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string ToHex(this string str)
        {
            string hex = "";
            foreach (char c in str)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        public static string FromHex(this string hex)
        {
            if (hex.Length%2 != 0)
                throw new ArgumentException("Input must be hex values and have an even number of characters.");

            string result = string.Empty;
            for (int i = 0; i < hex.Length; i += 2)
            {
                result += (char) Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return result;
        }

        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            if ((startIndex + length) > (str.Length - 1))
                length = (str.Length - 1) - startIndex;

            return str.Substring(startIndex, length);
        }

        public static string XmlEncode(this string data)
        {
            string result = string.Empty;
            foreach (char c in data)
            {
                if (encChars.ContainsKey(c))
                    result += encChars[c];
                else
                    result += c;
            }

            return result;
        }

        public static string XmlDecode(this string data)
        {
            foreach (string s in decChars.Keys)
                data = data.Replace(s, decChars[s].ToString());

            return data;
        }
    }
}