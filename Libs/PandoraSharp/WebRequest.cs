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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Util;

namespace PandoraSharp
{
    public class PRequest
    {
        private static WebProxy _proxy;

        public static void SetProxy(string address, string user = "", string password = "")
        {
            var p = new WebProxy(new Uri(address));

            if (user != "")
                p.Credentials = new NetworkCredential(user, password);

            _proxy = p;
        }

        public static void SetProxy(string address, int port, string user = "", string password = "")
        {
            var p = new WebProxy(address, port);

            if (user != "")
                p.Credentials = new NetworkCredential(user, password);

            _proxy = p;
        }

        public static string StringRequest(string url, string data)
        {
            var wc = new WebClient();
            if (_proxy != null)
                wc.Proxy = _proxy;

            wc.Headers.Add("Content-Type", "text/plain");
            wc.Headers.Add("User-Agent", Const.USER_AGENT);

            string response = string.Empty;
            try
            {
                response = wc.UploadString(new Uri(url), "POST", data);
            }
            catch (WebException wex)
            {
                Log.O("StringRequest Error: " + wex.ToString());
                //Wait and Try again, just in case
                Thread.Sleep(500);
                response = wc.UploadString(new Uri(url), "POST", data);
            }

            //Log.O(response);
            return response;
        }

        public static void ByteRequestAsync(string url, DownloadDataCompletedEventHandler dataHandler)
        {
            Log.O("Downloading Async: " + url);
            var wc = new WebClient();
            if (_proxy != null)
                wc.Proxy = _proxy;

            wc.DownloadDataCompleted += dataHandler;
            wc.DownloadDataAsync(new Uri(url));
        }

        public static byte[] ByteRequest(string url)
        {
            Log.O("Downloading: " + url);
            var wc = new WebClient();
            if (_proxy != null)
                wc.Proxy = _proxy;

            return wc.DownloadData(new Uri(url));
        }

        public static void FileRequest(string url, string outputFile)
        {
            var wc = new WebClient();
            if (_proxy != null)
                wc.Proxy = _proxy;
            wc.DownloadFile(url, outputFile);
        }
    }
}