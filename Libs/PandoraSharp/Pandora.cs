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
using System.Linq;
using System.Threading;
using PandoraSharp.Exceptions;
using Util;

namespace PandoraSharp
{
    public class Pandora
    {
        #region Delegates

        public delegate void ConnectionEventHandler(object sender, bool state, string msg);

        public delegate void FeedbackUpdateEventHandler(object sender, Song song, bool success);

        public delegate void LoginStatusEventHandler(object sender, string status);

        public delegate void PandoraErrorEventHandler(object sender, string errorCode, string msg);

        public delegate void StationsUpdatedEventHandler(object sender);

        public delegate void StationsUpdatingEventHandler(object sender);

        public delegate void QuickMixSavedEventHandler(object sender);

        #endregion

        #region SortOrder enum

        public enum SortOrder
        {
            DateAsc,
            DateDesc,
            AlphaAsc,
            AlphaDesc
        }

        #endregion

        private readonly object _authTokenLock = new object();
        private readonly object _rpcCountLock = new object();

        protected internal List<string> QuickMixStationIDs = new List<string>();
        private string _audioFormat = PAudioFormat.MP3;

        private string _authToken;
        private bool _authorizing;
        private bool _connected;
        private bool _firstAuthComplete = false;
        private string _imageCachePath = "";
        private string _password = "";
        private string _rid;
        private int _rpcCount;
        private int _timeOffset;

        private string _user = "";
        private string listenerId;
        //private string webAuthToken;

        public Pandora()
        {
            QuickMixStationIDs = new List<string>();
            StationSortOrder = SortOrder.DateDesc;
            //this.set_proxy(null);
        }

        private string AuthToken
        {
            get
            {
                lock (_authTokenLock)
                {
                    return _authToken;
                }
            }
            set
            {
                lock (_authTokenLock)
                {
                    _authToken = value;
                }
            }
        }

        public List<Station> Stations { get; private set; }

        public string ImageCachePath
        {
            get { return _imageCachePath; }
            set { _imageCachePath = value; }
        }

        [DefaultValue(false)]
        public bool HasSubscription { get; private set; }

        public string AudioFormat
        {
            get { return _audioFormat; }
            set { SetAudioFormat(value); }
        }

        public SortOrder StationSortOrder { get; set; }
        public event ConnectionEventHandler ConnectionEvent;
        public event StationsUpdatedEventHandler StationUpdateEvent;
        public event StationsUpdatingEventHandler StationsUpdatingEvent;
        public event FeedbackUpdateEventHandler FeedbackUpdateEvent;
        public event LoginStatusEventHandler LoginStatusEvent;
        public event QuickMixSavedEventHandler QuickMixSavedEvent;

        protected internal string RPCRequest(string url, string data)
        {
            try
            {
                return PRequest.StringRequest(url, data);
            }
            catch (Exception e)
            {
                Log.O(e.ToString());
                throw new PandoraException("ERROR_RPC", e);
            }
        }

        //Checks for fault returns.  If it's an Auth fault (auth timed out)
        //return false, which signals that a re-auth and retry needs to be done
        //otherwise return true signalling all clear.
        //All other faults will be thrown
        protected internal bool HandleFaults(string response, bool secondTry)
        {
            string fault = XmlRPC.ResponseFaultCheck(response);
            if (fault != string.Empty)
            {
                if (fault == "AUTH_INVALID_TOKEN" || fault == "DAILY_SKIP_LIMIT_REACHED")
                    if (!secondTry)
                        return false; //auth fault, signal a re-auth

                Log.O("Fault: " + fault);
                throw new PandoraException(fault); //other, throw the exception
            }

            return true; //no fault
        }

        protected internal string CallRPC_Internal(string method, object[] args, bool b_url_args, bool isAuth,
                                                   bool useSSL = false, bool insertTime = true)
        {
            int callID = 0;
            lock (_rpcCountLock)
            {
                callID = _rpcCount++;
            }

            if (args == null)
                args = new object[] {};

            object[] url_args = args;

            var arg_list = new List<object>(args);
            if (insertTime)
                arg_list.Insert(0, Time.Unix() - _timeOffset);

            if (AuthToken != null)
                arg_list.Insert(1, AuthToken);
            args = arg_list.ToArray();

            string xml = XmlRPC.GenerateRPCXml(method, args);
            string data = PandoraCrypt.Encrypt(xml);

            var url_arg_strings = new List<string>();
            if (_rid != null)
                url_arg_strings.Add("rid=" + _rid);

            if (listenerId != null)
                url_arg_strings.Add("lid=" + listenerId);

            method = method.Split('.')[1];

            url_arg_strings.Add("method=" + method);

            if (b_url_args)
            {
                int count = 1;
                foreach (object a in url_args)
                {
                    url_arg_strings.Add("arg" + count + "=" + format_url_arg(a));
                    count++;
                }
            }

            string url = (useSSL ? "https://" : "http://") + Const.RPC_URL + string.Join("&", url_arg_strings);

            Log.O("[" + callID + ":url]: " + url);


            if (isAuth)
                Log.O("[" + callID + ":data]: " + xml.Replace(_password, "********").Replace(_user, "********"));
            else
                Log.O("[" + callID + ":data]: " + xml);

            //if reauthorizing, wait until it completes.
            if (!isAuth)
            {
                int waitCount = 30;
                while (_authorizing)
                {
                    waitCount--;
                    if (waitCount >= 0)
                        Thread.Sleep(1000);
                    else
                        break;
                }
            }

            string response = RPCRequest(url, data);
            Log.O("[" + callID + ":response]: " + response);
            return response;
        }

        protected internal object CallRPC(string method, object[] args = null, bool b_url_args = false,
                                          bool isAuth = false, bool useSSL = false, bool insertTime = true)
        {
            string response = CallRPC_Internal(method, args, b_url_args, isAuth, useSSL, insertTime);
            if (method == "listener.authenticateListener")
            {
                return XmlRPC.ParseXML(response);
            }
            else
            {
                if (!HandleFaults(response, false))
                {
                    Log.O("Reauth Required");
                    if (!AuthenticateUser()) //re-auth
                    {
                        HandleFaults(response, true);
                    }
                    else
                    {
                        response = CallRPC_Internal(method, args, b_url_args, isAuth, useSSL, insertTime);
                        HandleFaults(response, true);
                    }
                }
            }

            return XmlRPC.ParseXML(response);
        }

        public void RefreshStations()
        {
            Log.O("RefreshStations");
            if (StationsUpdatingEvent != null)
                StationsUpdatingEvent(this);

            var stationList = (object[]) CallRPC("station.getStations");
            QuickMixStationIDs.Clear();

            Stations = new List<Station>();
            foreach (PDict s in stationList)
                Stations.Add(new Station(this, s));

            if (QuickMixStationIDs.Count > 0)
            {
                foreach (Station s in Stations)
                {
                    if (QuickMixStationIDs.Contains(s.ID))
                        s.UseQuickMix = true;
                }
            }

            List<Station> quickMixes = Stations.FindAll(x => x.IsQuickMix);
            Stations = Stations.FindAll(x => !x.IsQuickMix);

            switch (StationSortOrder)
            {
                case SortOrder.DateDesc:
                    Stations = Stations.OrderByDescending(x => x.ID).ToList();
                    break;
                case SortOrder.DateAsc:
                    Stations = Stations.OrderBy(x => x.ID).ToList();
                    break;
                case SortOrder.AlphaDesc:
                    Stations = Stations.OrderByDescending(x => x.Name).ToList();
                    break;
                case SortOrder.AlphaAsc:
                    Stations = Stations.OrderBy(x => x.Name).ToList();
                    break;
            }

            Stations.InsertRange(0, quickMixes);

            if (StationUpdateEvent != null)
                StationUpdateEvent(this);
        }

        private string getSyncKey()
        {
            string result = string.Empty;

            try
            {
                var keyArray = new Util.Downloader().DownloadString(Const.SYNC_KEY_URL);

                var vals = keyArray.Split('|');
                if (vals.Length < 3) return result;
                var len = 48;
                if (!Int32.TryParse(vals[1], out len)) return result;
                if (vals[2].Length != len) return result;

                Log.O("Sync Key Age (sec): " + vals[0]);
                Log.O("Sync Key Length: " + vals[1]);
                Log.O("Sync Key: " + vals[2]);

                result = vals[2];
            }
            catch (Exception ex)
            {
                Log.O(ex.ToString());
            }

            return result;
        }

        private string getSyncTime()
        {
            string result = string.Empty;

            try
            {
                result = new Util.Downloader().DownloadString(Const.SYNC_TIME_URL);
            }
            catch (Exception ex)
            {
                Log.O(ex.ToString());
            }

            return result;
        }

        public void Logout()
        {
            _firstAuthComplete = false;
        }

        public bool AuthenticateUser()
        {
            _authorizing = true;

            Log.O("AuthUser");

            listenerId = null;
            //webAuthToken = null;
            AuthToken = null;

            //reset the unique tokens
            _rid = string.Format("{0:d7}P", (Time.Unix()%10000000));

            //var syncKey = getSyncKey();
            //var syncParams = syncKey != string.Empty ? new object[] { syncKey } : new object[] { };
            ////Sync with server
            //var crypt_time = (string)CallRPC("misc.sync", syncParams, false, true, true, false);
            //int realTime = Time.Unix();

            //var stx = new string((char) 2, 1);
            //string decrypt_time = PandoraCrypt.Decrypt(crypt_time);
            //decrypt_time = decrypt_time.SafeSubstring(4, decrypt_time.Length).Replace(stx, string.Empty);

            //if (decrypt_time != string.Empty)
            //    _timeOffset = realTime - int.Parse(decrypt_time);

            string sync_time = getSyncTime();
            int realTime = Time.Unix();
            if (sync_time != string.Empty)
                _timeOffset = realTime - int.Parse(sync_time);

            object userData = CallRPC("listener.authenticateListener", new object[] {_user, _password}, false, true, true);
            if (userData == null) return false;

            try
            {
                var userDict = (PDict) userData;
                string fault = string.Empty;
                if ((fault = XmlRPC.GetFaultString(userDict)) != string.Empty)
                {
                    throw new PandoraException(fault);
                }

                //webAuthToken = (string) userDict["webAuthToken"];
                listenerId = (string) userDict["listenerId"];
                AuthToken = (string) userDict["authToken"];

                if (!_firstAuthComplete)
                {
                    HasSubscription = ((int) userDict["subscriptionDaysLeft"]) > 0;

                    if (!HasSubscription && _audioFormat == PAudioFormat.MP3_HIFI)
                        _audioFormat = PAudioFormat.MP3;
                }

                _firstAuthComplete = true;

                _authorizing = false;
                return true;
            }
            catch (PandoraException pex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                _authorizing = false;
            }
        }

        private void SendLoginStatus(string status)
        {
            if (LoginStatusEvent != null)
                LoginStatusEvent(this, status);
        }

        public void Connect(string user, string password)
        {
            Log.O("Connect");
            string statusMsg = "Connected";
            _connected = false;

            _user = user;
            _password = password;

            try
            {
                SendLoginStatus("Authenticating user...");
                _connected = AuthenticateUser();

                if (_connected)
                {
                    SendLoginStatus("Loading station list...");
                    RefreshStations();
                }
            }
            catch (Exception ex)
            {
                statusMsg = ex.Message;
                _connected = false;
            }

            if (ConnectionEvent != null)
                ConnectionEvent(this, _connected, statusMsg);
        }

        //public void SetProxy()
        //{

        //}

        public void SetAudioFormat(string fmt)
        {
            if ((fmt != PAudioFormat.AACPlus &&
                 fmt != PAudioFormat.MP3 &&
                 fmt != PAudioFormat.MP3_HIFI) ||
                (!HasSubscription && fmt == PAudioFormat.MP3_HIFI))
            {
                fmt = PAudioFormat.MP3;
            }

            _audioFormat = fmt;
        }

        public void SaveQuickMix()
        {
            var ids = new List<string>();
            foreach (Station s in Stations)
            {
                if (s.UseQuickMix)
                    ids.Add(s.ID);
            }

            CallRPC("station.setQuickMix", new object[] {"RANDOM", (object[])ids.ToArray()});

            if (QuickMixSavedEvent != null)
                QuickMixSavedEvent(this);
        }

        public List<SearchResult> Search(string query)
        {
            Log.O("Search: " + query);
            var results = (PDict) CallRPC("music.search", new object[] {query});

            var list = new List<SearchResult>();
            var artists = ((object[]) (results)["artists"]);
            var songs = ((object[]) (results)["songs"]);
            foreach (PDict a in artists)
                list.Add(new SearchResult(SearchResultType.Artist, a));

            foreach (PDict s in songs)
                list.Add(new SearchResult(SearchResultType.Song, s));

            list = list.OrderByDescending((i) => i.Score).ToList();

            return list;
        }

        private Station CreateStation(string reqType, string id)
        {
            if (reqType != "mi" && reqType != "sh")
                throw new PandoraException("CreateStation: reqType must be mi or sh.");

            var result = (PDict) CallRPC("station.createStation",
                                         new object[] {reqType + id, ""});

            var station = new Station(this, result);
            Stations.Add(station);

            return station;
        }

        public Station CreateStationFromMusic(string id)
        {
            return CreateStation("mi", id);
        }

        public Station CreateStationFromShared(string id)
        {
            return CreateStation("sh", id);
        }

        public Station CreateStationFromSong(Song song)
        {
            return CreateStationFromMusic(song.MusicID);
        }

        public Station CreateStationFromArtist(Song song)
        {
            return CreateStationFromMusic(song.ArtistMusicID);
        }

        public void AddFeedback(string stationID, string trackToken, SongRating rating)
        {
            Log.O("AddFeedback");

            bool rate = (rating == SongRating.love) ? true : false;

            CallRPC("station.addFeedback", new object[]
                                               {
                                                   stationID,
                                                   trackToken,
                                                   rate
                                               });
        }

        public void DeleteFeedback(string feedbackID)
        {
            Log.O("DeleteFeedback");
            object result = CallRPC("station.deleteFeedback", new object[] {feedbackID});
        }

        public void CallFeedbackUpdateEvent(Song song, bool success)
        {
            if (FeedbackUpdateEvent != null)
                FeedbackUpdateEvent(this, song, success);
        }

        public Station GetStationByID(string id)
        {
            foreach (Station s in Stations)
            {
                if (s.ID == id)
                    return s;
            }

            return null;
        }

        public string GetFeedbackID(string stationID, string musicID)
        {
            var station = (PDict) CallRPC("station.getStation", new object[] {stationID});
            var feedback = ((object[]) station["feedback"]);

            foreach (PDict d in feedback)
            {
                if (musicID == (string) d["musicId"])
                    return (string) d["feedbackId"];
            }

            return string.Empty;
        }

        private string format_url_arg(object v)
        {
            string result = string.Empty;
            Type t = v.GetType();
            if (t == typeof (bool))
            {
                if ((bool) v)
                    result = "true";
                else
                    result = "false";
            }
            else if (t == typeof (string[]))
                result = string.Join("%2C", (string[]) v);
            else if (t == typeof (int))
            {
                result = ((int) v).ToString();
            }
            else
            {
                result = Uri.EscapeUriString((string) v);
            }

            return result;
        }
    }
}