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
using System.ComponentModel;
using System.IO;
using System.Net;
using Util;
using PandoraSharp.Exceptions;
using Newtonsoft.Json.Linq;

namespace PandoraSharp
{
    public class Station : INotifyPropertyChanged
    {
        private readonly object _artLock = new object();
        private readonly Pandora _pandora;
        private byte[] _artImage;

        public Station(Pandora p, JToken d)
        {
            SkipLimitReached = false;
            SkipLimitTime = DateTime.MinValue;

            _pandora = p;
            
            ID = d["stationId"].ToString();
            IdToken = d["stationToken"].ToString();
            IsCreator = !d["isShared"].ToObject<bool>();
            IsQuickMix = d["isQuickMix"].ToObject<bool>();
            Name = d["stationName"].ToString();
            InfoUrl = (string)d["stationDetailUrl"];

            if (IsQuickMix)
            {
                Name = "Quick Mix";
                _pandora.QuickMixStationIDs.Clear();
                var qmIDs = d["quickMixStationIds"].ToObject<string[]>();
                foreach(var qmid in qmIDs)
                    _pandora.QuickMixStationIDs.Add((string) qmid);
            }

            bool downloadArt = true;
            if (!_pandora.ImageCachePath.Equals("") && File.Exists(ArtCacheFile))
            {
                try
                {
                    ArtImage = File.ReadAllBytes(ArtCacheFile);
                }
                catch (Exception)
                {
                    Log.O("Error retrieving image cache file: " + ArtCacheFile);
                    downloadArt = true;
                }

                downloadArt = false;
            }

            if (downloadArt)
            {
                var value = d.SelectToken("artUrl");
                if (value != null)
                {
                    ArtUrl = value.ToString();

                    if (ArtUrl != string.Empty)
                    {
                        try
                        {
                            ArtImage = PRequest.ByteRequest(ArtUrl);
                            if (ArtImage.Length > 0)
                                File.WriteAllBytes(ArtCacheFile, ArtImage);
                        }
                        catch (Exception)
                        {
                            Log.O("Error saving image cache file: " + ArtCacheFile);
                        }
                    }
                }
                //}
                
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string ID { get; private set; }
        public string IdToken { get; private set; }
        public bool IsCreator { get; private set; }

        [DefaultValue(false)]
        public bool IsQuickMix { get; private set; }

        public string Name { get; private set; }

        private bool _useQuickMix = false;
        public bool UseQuickMix
        {
            get { return _useQuickMix; }
            set
            {
                if (value != _useQuickMix)
                {
                    _useQuickMix = value;
                    Notify("UseQuickMix");
                }
            }
        }

        public string ArtUrl { get; private set; }

        public byte[] ArtImage
        {
            get
            {
                lock (_artLock)
                {
                    return _artImage;
                }
            }
            private set
            {
                lock (_artLock)
                {
                    _artImage = value;
                }
            }
        }

        public string InfoUrl
        {
            get;
            set;
        }

        public string ArtCacheFile
        {
            get { return Path.Combine(_pandora.ImageCachePath, "Station_" + IdToken); }
        }

        public bool SkipLimitReached { get; set; }
        public DateTime SkipLimitTime { get; set; }

        private void StationArtDownloadHandler(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Result.Length == 0)
                return;

            ArtImage = e.Result;
        }

        public void TransformIfShared()
        {
            if (!IsCreator)
            {
                Log.O("Pandora: Transforming Station");
                _pandora.CallRPC("station.transformSharedStation", "stationToken", IdToken);
                IsCreator = true;
            }
        }

        private bool _gettingPlaylist = false;
        public List<Song> GetPlaylist()
        { 
            var results = new List<Song>();
            if (_gettingPlaylist) return results;
            Log.O("GetPlaylist");
            try
            {
                _gettingPlaylist = true;
                JObject req = new JObject();
                req["stationToken"] = IdToken;
                if(_pandora.AudioFormat != PAudioFormat.AACPlus)
                    req["additionalAudioUrl"] = "HTTP_128_MP3,HTTP_192_MP3";

                var playlist = _pandora.CallRPC("station.getPlaylist", req, false, true); // MUST use SSL

                foreach (var song in playlist.Result["items"])
                {
                    if (song["songName"] == null) continue;
                    try
                    {
                        results.Add(new Song(_pandora, song));
                    }
                    catch (PandoraException ex)
                    {
                        Log.O("Song Add Error: " + ex.FaultMessage);
                    }
                }

                _gettingPlaylist = false;
                return results;
            }
            catch (PandoraException ex) 
            {
                _gettingPlaylist = false;
                if (ex.Message == "PLAYLIST_END" || ex.Message == "DAILY_SKIP_LIMIT_REACHED")
                {
                    if (ex.Message == "PLAYLIST_END")
                    {
                        SkipLimitReached = true;
                        SkipLimitTime = DateTime.Now;
                    }
                    else
                        throw;
                }

                Log.O("Error getting playlist, will try again next time: " + Errors.GetErrorMessage(ex.Fault));
                return results;
            }
        }

        public void AddVariety(SearchResult item)
        {
            Log.O("Pandora: Adding {0} to {1}", item.DisplayName, this.Name);

            try
            {
                _pandora.CallRPC("station.addMusic", "stationToken", IdToken, "musicToken", item.MusicToken);
            }
            catch{} // eventually do something with this
        }

        public void Rename(string newName)
        {
            if (newName == Name)
                return;
            Log.O("Pandora: Renaming Station");
            _pandora.CallRPC("station.renameStation", "stationToken", IdToken, "stationName", newName);

            Name = newName;
        }

        public void Delete()
        {
            Log.O("Pandora: Deleting Station");
            _pandora.CallRPC("station.deleteStation", "stationToken", IdToken);
            if (File.Exists(ArtCacheFile))
            {
                try
                {
                    File.Delete(ArtCacheFile);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}