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
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace Elpis.UpdateSystem
{
    public class UpdateCheck
    {
        #region Delegates

        public delegate void UpdateDataLoadedEventHandler(bool foundUpdate);

        #endregion

        private bool _downloadComplete;
        private string _downloadString = string.Empty;

        public Version CurrentVersion
        {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        public Version NewVersion { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseNotesPath { get; set; }
        public string ReleaseNotes { get; set; }
        public bool UpdateNeeded { get; set; }
        public event UpdateDataLoadedEventHandler UpdateDataLoadedEvent;

        private void SendUpdateEvent(bool foundUpdate)
        {
            if (UpdateDataLoadedEvent != null)
                UpdateDataLoadedEvent(foundUpdate);
        }

        private string DownloadString(string url, int timeoutSec = 10)
        {
            using (var wc = new WebClient())
            {
                wc.DownloadStringCompleted += wc_DownloadStringCompleted;

                _downloadComplete = false;
                _downloadString = string.Empty;

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

        private bool CheckForUpdateInternal()
        {
            try
            {
                Log.O("Checking for updates...");
                string updateUrl = "";

#if APP_RELEASE
                updateUrl = ReleaseData.UpdateBaseUrl + ReleaseData.UpdateConfigFile +
                            "?r=" + DateTime.UtcNow.ToEpochTime().ToString();
                    //Because WebClient won't let you disable caching :(
#endif
                Log.O("Downloading update file: " + updateUrl);

                string data = DownloadString(updateUrl);

                var mc = new MapConfig();
                mc.LoadConfig(data);

                string verStr = mc.GetValue("CurrentVersion", string.Empty);
                DownloadUrl = mc.GetValue("DownloadUrl", string.Empty);
                ReleaseNotesPath = mc.GetValue("ReleaseNotes", string.Empty);

                ReleaseNotes = string.Empty;
                if(ReleaseNotesPath != string.Empty)
                {
                    ReleaseNotes = DownloadString(ReleaseNotesPath);
                }

                Version ver = null;

                if (!Version.TryParse(verStr, out ver))
                {
                    SendUpdateEvent(false);
                    return false;
                }

                NewVersion = ver;

                bool result = false;
                if (NewVersion > CurrentVersion)
                    result = true;

                UpdateNeeded = result;
                SendUpdateEvent(result);
                return result;
            }
            catch (Exception e)
            {
                Log.O("Error checking for updates: " + e);
                UpdateNeeded = false;
                SendUpdateEvent(false);
                return false;
            }
        }

        public bool CheckForUpdate()
        {
            return CheckForUpdateInternal();
        }

        public void CheckForUpdateAsync()
        {
            Task.Factory.StartNew(() => CheckForUpdateInternal());
        }
    }
}