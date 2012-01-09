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

namespace PandoraSharp
{
    public class Station : INotifyPropertyChanged
    {
        private readonly object _artLock = new object();
        private readonly Pandora _pandora;
        private byte[] _artImage;

        public Station(Pandora p, PDict d)
        {
            _pandora = p;
            
            ID = (string) d["stationId"];
            IdToken = (string) d["stationIdToken"];
            IsCreator = (bool) d["isCreator"];
            IsQuickMix = (bool) d["isQuickMix"];
            Name = (string) d["stationName"];

            if (IsQuickMix)
            {
                Name = "Quick Mix";
                _pandora.QuickMixStationIDs.Clear();
                var qmIDs = (object[]) d["quickMixStationIds"];
                foreach(var qmid in qmIDs)
                    _pandora.QuickMixStationIDs.Add((string) qmid);
            }

            try
            {
                if (d.ContainsKey("initialSeed"))
                {
                    var seed = (PDict) d["initialSeed"];

                    bool getArt = true;
                    PDict entry = null;
                    if (seed.ContainsKey("artist"))
                        entry = (PDict) seed["artist"];
                    else if (seed.ContainsKey("song"))
                        entry = (PDict) seed["song"];
                    else
                        getArt = false;

                    if (getArt)
                    {
                        ArtUrl = (string) entry["artUrl"];

                        if (ArtUrl != string.Empty)
                        {
                            bool download = true;
                            if (!_pandora.ImageCachePath.Equals("") && File.Exists(ArtCacheFile))
                            {
                                try
                                {
                                    ArtImage = File.ReadAllBytes(ArtCacheFile);
                                }
                                catch (Exception)
                                {
                                    Log.O("Error retrieving image cache file: " + ArtCacheFile);
                                    download = true;
                                }

                                download = false;
                            }

                            if (download)
                            {  
                                try
                                {
                                    ArtImage = PRequest.ByteRequest(ArtUrl);
                                    if(ArtImage.Length > 0)
                                        File.WriteAllBytes(ArtCacheFile, ArtImage);
                                }
                                catch (Exception)
                                {
                                    Log.O("Error saving image cache file: " + ArtCacheFile);
                                }
                                //PRequest.ByteRequestAsync(ArtUrl, StationArtDownloadHandler);
                            }
                        }
                    }
                }
            }
            catch
            {
                Log.O("Error getting station art.");
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
            get { return "http://www.pandora.com/stations/" + IdToken; }
        }

        public string ArtCacheFile
        {
            get { return Path.Combine(_pandora.ImageCachePath, "Station_" + IdToken); }
        }

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
                _pandora.CallRPC("station.transformShared", new object[] {ID});
                IsCreator = true;
            }
        }

        private bool _gettingPlaylist = false;
        public List<Song> GetPlaylist()
        { 
            Log.O("GetPlaylist");
            var results = new List<Song>();
            if (_gettingPlaylist) return results;
            try
            {
                _gettingPlaylist = true;
                var playlist =
                    (object[])_pandora.CallRPC("playlist.getFragment",
                                                new object[]
                                                {
                                                    ID,
                                                    "0", "",
                                                    "", _pandora.AudioFormat,
                                                    "", ""
                                                });

                foreach (PDict s in playlist)
                    results.Add(new Song(_pandora, s));

                _gettingPlaylist = false;
                return results;
            }
            catch (PandoraException ex) 
            {
                _gettingPlaylist = false;
                Log.O("Error getting playlist, will try again next time: " + ex.FaultCode);
                return results;
            }
        }

        public void AddVariety(SearchResult item)
        {
            Log.O("Pandora: Adding {0} to {1}", item.DisplayName, this.Name);

            try
            {
                _pandora.CallRPC("station.addSeed",
                                 new object[] { this.ID, item.MusicID });
            }
            catch{} // eventually do something with this
        }

        public void Rename(string newName)
        {
            if (newName == Name)
                return;
            Log.O("Pandora: Renaming Station");
            _pandora.CallRPC("station.setStationName",
                             new object[] {ID, newName});

            Name = newName;
        }

        public void Delete()
        {
            Log.O("Pandora: Deleting Station");
            _pandora.CallRPC("station.removeStation", new object[] {ID});
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