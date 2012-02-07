using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace Util
{
    public class Downloader
    {
        private bool _downloadComplete;
        private string _downloadString = string.Empty;

        public string DownloadString(string url, int timeoutSec = 10)
        {
            using (var wc = new WebClient())
            {
                wc.DownloadStringCompleted += wc_DownloadStringCompleted;

                _downloadComplete = false;
                _downloadString = string.Empty;
                Log.O("Downloading: " + url);
                wc.DownloadStringAsync(new Uri(url));

                DateTime start = DateTime.Now;
                while (!_downloadComplete && ((DateTime.Now - start).TotalMilliseconds < (timeoutSec * 1000)))
                    Thread.Sleep(25);

                if (_downloadComplete)
                    return _downloadString;

                wc.CancelAsync();

                throw new Exception("Timeout waiting for " + url + " to download.");
            }
        }

        private void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try { _downloadString = e.Result; }
            catch { _downloadString = string.Empty; }

            _downloadComplete = true;
        }
    }
}
