/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
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
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading;

namespace Util
{
    public class PostSubmitter
    {
        private readonly NameValueCollection _params;
        private readonly string _url = string.Empty;

        private bool _uploadComplete = false;
        private string _uploadResult = string.Empty;

        public PostSubmitter(string url)
        {
            _url = url;
            _url = _url + "?sync=" + DateTime.UtcNow.ToEpochTime().ToString(); //randomize URL to prevent caching
            _params = new NameValueCollection();
        }

        public void Add(string key, string value)
        {
            _params.Add(key, value);
        }

        public string Send(int timeoutSec = 10)
        {
            var wc = new WebClient();
            wc.UploadValuesCompleted += wc_UploadValuesCompleted;

            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            wc.Headers.Add("Origin", "http://elpis.adamhaile.net");

            if (PRequest.Proxy != null)
                wc.Proxy = PRequest.Proxy;

            wc.UploadValuesAsync(new Uri(_url), "POST", _params);

            DateTime start = DateTime.Now;
            while (!_uploadComplete && ((DateTime.Now - start).TotalMilliseconds < (timeoutSec * 1000)))
                Thread.Sleep(25);

            if (_uploadComplete)
                return _uploadResult;

            wc.CancelAsync();

            throw new Exception("Timeout waiting for POST to " + _url);
        }

        void wc_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            _uploadResult = Encoding.ASCII.GetString(e.Result);
            _uploadComplete = true;
        }
    }
}