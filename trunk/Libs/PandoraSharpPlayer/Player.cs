﻿/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of PandoraSharpPlayer.
 * PandoraSharpPlayer is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * PandoraSharpPlayer is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with PandoraSharpPlayer. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using BassPlayer;
using PandoraSharp;
using PandoraSharp.Exceptions;
using Log = Util.Log;
using Util;

namespace PandoraSharpPlayer
{
    public class Player : INotifyPropertyChanged
    {
        private BassAudioEngine _bass;
        private Pandora _pandora;

        private bool _playNext;
        private Playlist _playlist;

        #region Events

        #region Delegates

        public delegate void ConnectionEventHandler(object sender, bool state, ErrorCodes code);

        public delegate void LogoutEventHandler(object sender);
        
        public delegate void ExceptionEventHandler(object sender, ErrorCodes code, Exception ex);

        public delegate void FeedbackUpdateEventHandler(object sender, Song song, bool success);

        public delegate void LoadingNextSongHandler(object sender);

        public delegate void LoginStatusEventHandler(object sender, string status);

        public delegate void PlaybackProgressHandler(object sender, BassAudioEngine.Progress prog);

        public delegate void PlaybackStartHandler(object sender, double duration);

        public delegate void PlaybackStateChangedHandler(
            object sender, BassAudioEngine.PlayState oldState, BassAudioEngine.PlayState newState);

        public delegate void PlaybackStopHandler(object sender);

        public delegate void PlaylistSongHandler(object sender, Song song);

        public delegate void SearchResultHandler(object sender, List<SearchResult> result);

        public delegate void StationCreatedHandler(object sender, Station station);

        public delegate void StationLoadedHandler(object sender, Station station);

        public delegate void StationLoadingHandler(object sender, Station station);

        public delegate void StationsRefreshedHandler(object sender);

        public delegate void StationsRefreshingHandler(object sender);

        public delegate void QuickMixSavedEventHandler(object sender);

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public event PlaybackProgressHandler PlaybackProgress;
        public event PlaybackStateChangedHandler PlaybackStateChanged;
        public event PlaybackStartHandler PlaybackStart;
        public event PlaybackStopHandler PlaybackStop;

        public event PlaylistSongHandler SongStarted;
        public event PlaylistSongHandler PlayedSongAdded;
        public event PlaylistSongHandler PlayedSongRemoved;

        public event StationLoadingHandler StationLoading;

        public event StationLoadedHandler StationLoaded;

        public event ConnectionEventHandler ConnectionEvent;

        public event LogoutEventHandler LogoutEvent;

        public event FeedbackUpdateEventHandler FeedbackUpdateEvent;

        public event ExceptionEventHandler ExceptionEvent;

        public event StationsRefreshedHandler StationsRefreshed;

        public event StationsRefreshingHandler StationsRefreshing;

        public event LoadingNextSongHandler LoadingNextSong;

        public event SearchResultHandler SearchResult;

        public event StationCreatedHandler StationCreated;

        public event QuickMixSavedEventHandler QuickMixSavedEvent;

        public event LoginStatusEventHandler LoginStatusEvent;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        public bool Initialize(string bassRegEmail = "", string bassRegKey = "")
        {
            _pandora = new Pandora();
            _pandora.ConnectionEvent += _pandora_ConnectionEvent;
            _pandora.StationUpdateEvent += _pandora_StationUpdateEvent;
            _pandora.FeedbackUpdateEvent += _pandora_FeedbackUpdateEvent;
            _pandora.LoginStatusEvent += _pandora_LoginStatusEvent;
            _pandora.StationsUpdatingEvent += _pandora_StationsUpdatingEvent;
            _pandora.QuickMixSavedEvent += _pandora_QuickMixSavedEvent;

            _bass = new BassAudioEngine(bassRegEmail, bassRegKey);
            _bass.PlaybackProgress += bass_PlaybackProgress;
            _bass.PlaybackStateChanged += bass_PlaybackStateChanged;
            _bass.PlaybackStart += bass_PlaybackStart;
            _bass.PlaybackStop += bass_PlaybackStop;
            _bass.InitBass();

            _playlist = new Playlist();
            _playlist.MaxPlayed = 8;
            _playlist.PlaylistLow += _playlist_PlaylistLow;
            _playlist.PlayedSongQueued += _playlist_PlayedSongQueued;
            _playlist.PlayedSongDequeued += _playlist_PlayedSongDequeued;

            DailySkipLimitReached = false;
            DailySkipLimitTime = DateTime.MinValue;

            LoggedIn = false;
            return true;
        }        

        #region Properties

        private bool _paused;
        private bool _playing;
        private bool _stopped = true;
        public string Email { get; set; }
        public string Password { get; set; }

        public List<Station> Stations
        {
            get { return _pandora.Stations; }
        }

        public string ImageCachePath
        {
            get
            {
                if (_pandora != null)
                {
                    return _pandora.ImageCachePath;
                }
                else
                {
                    throw new Exception("Pandora object has not been initialized, cannot get ImagePathCache");
                }
            }
            set
            {
                if (_pandora != null)
                {
                    if (!Directory.Exists(value))
                        throw new Exception("ImagePathCache directory does not exist!");

                    _pandora.ImageCachePath = value;
                }
                else
                {
                    throw new Exception("Pandora object has not been initialized, cannot get ImagePathCache");
                }
            }
        }

        public bool LoggedIn { get; set; }

        [DefaultValue(null)]
        public Station CurrentStation { get; private set; }

        public bool IsStationLoaded
        {
            get { return (CurrentStation != null); }
        }

        public Song CurrentSong
        {
            get { return _playlist.Current; }
        }

        public string AudioFormat
        {
            get
            {
                if (_pandora != null) return _pandora.AudioFormat;
                else return "";
            }

            set
            {
                if (_pandora != null)
                {
                    _pandora.SetAudioFormat(value);
                }
            }
        }

        public bool ForceSSL
        {
            get
            {
                if (_pandora != null) return _pandora.ForceSSL;
                else return false;
            }

            set
            {
                if (_pandora != null)
                {
                    _pandora.ForceSSL = value;
                }
            }
        }

        public Pandora.SortOrder StationSortOrder
        {
            get
            {
                if (_pandora != null) return _pandora.StationSortOrder;
                else return Pandora.SortOrder.DateDesc;
            }

            set
            {
                if (_pandora != null)
                {
                    SetStationSortOrder(value);
                }
            }
        }

        public bool Paused
        {
            get { return _paused; }

            private set
            {
                if (value != _paused)
                {
                    _paused = value;
                    NotifyPropertyChanged("Paused");
                }
            }
        }

        public bool Playing
        {
            get { return _playing; }

            private set
            {
                if (value != _playing)
                {
                    _playing = value;
                    NotifyPropertyChanged("Playing");
                }
            }
        }

        public bool Stopped
        {
            get { return _stopped; }

            private set
            {
                if (value != _stopped)
                {
                    _stopped = value;
                    NotifyPropertyChanged("Stopped");
                }
            }
        }

        public int Volume
        {
            get { return _bass.Volume; }
            set { _bass.Volume = value; }
        }

        public bool DailySkipLimitReached { get; set; }
        public DateTime DailySkipLimitTime { get; set; }

        #endregion

        #region Private Methods

        private void SendPandoraError(ErrorCodes code, Exception ex)
        {
            if (ExceptionEvent != null)
                ExceptionEvent(this, code, ex);
        }

        private void PlayNextSong(int retry = 2)
        {
            if (!_playNext || retry < 2)
            {
                _playNext = true;
                Song song = null;
                if (LoadingNextSong != null)
                {
                    Log.O("Loading next song.");
                    LoadingNextSong(this);
                }

                try
                {
                    song = _playlist.NextSong();
                }
                catch (PandoraException pex)
                {
                    _playNext = false;
                    if (pex.Fault == ErrorCodes._END_OF_PLAYLIST)
                    {
                        Stop();
                        return;
                    }

                    throw;
                }

                Log.O("Play: " + song);
                if (SongStarted != null)
                    SongStarted(this, song);
                try
                {
                    _bass.Play(song.AudioUrl, song.FileGain);
                }
                catch (Exception ex)
                {
                    _playNext = false;
                    if (ex.GetType() == typeof (BassStreamException))
                    {
                        if (((BassStreamException)ex).ErrorCode == Un4seen.Bass.BASSError.BASS_ERROR_FILEOPEN)
                        {
                            _playlist.DoReload();
                        }

                        if (retry > 0)
                            PlayNextSong(retry - 1);
                        else
                        {
                            Stop();
                            throw new PandoraException(ErrorCodes.STREAM_ERROR, ex);
                        }
                    }
                    else
                        throw;
                }

                _playNext = false;
            }
        }

        private int UpdatePlaylist()
        {
            List<Song> result = new List<Song>();
            try
            {
                result = CurrentStation.GetPlaylist();
            }
            catch (PandoraException ex)
            {
                if (ex.Message == "DAILY_SKIP_LIMIT_REACHED")
                {
                    DailySkipLimitReached = true;
                    DailySkipLimitTime = DateTime.Now;
                }
            }

            if (result.Count == 0 && CurrentStation != null)
                result = CurrentStation.GetPlaylist();

            return _playlist.AddSongs(result);
        }

        private void PlayThread()
        {
            if (StationLoading != null)
                StationLoading(this, CurrentStation);

            _playlist.ClearSongs();

            if (UpdatePlaylist() == 0)
                throw new PandoraException(ErrorCodes._END_OF_PLAYLIST);

            if (StationLoaded != null)
                StationLoaded(this, CurrentStation);

            try
            {
                PlayNextSong();
            }
            catch (Playlist.PlaylistEmptyException pex)
            {
                if (UpdatePlaylist() == 0)
                    throw new PandoraException(ErrorCodes._END_OF_PLAYLIST);

                PlayNextSong();
            }
        }

        #endregion

        #region Public Methods

        private void RunTask(Action method)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          try
                                          {
                                              method();
                                          }
                                          catch (PandoraException pex)
                                          {
                                              Log.O(pex.Fault.ToString() + ": " + pex);
                                              SendPandoraError(pex.Fault, pex);
                                          }
                                          catch (Exception ex)
                                          {
                                              Log.O(ex.ToString());

                                              SendPandoraError(ErrorCodes.UNKNOWN_ERROR, ex);
                                          }
                                      }
                );
        }

        public void Logout()
        {
            LoggedIn = false;
            Stop();
            CurrentStation = null;
            _playlist.ClearSongs();
            _playlist.ClearHistory();
            _playlist.Current = null;
            _pandora.Logout();
            Email = string.Empty;
            Password = string.Empty;

            if (LogoutEvent != null)
                LogoutEvent(this);
        }

        public void Connect(string email, string password)
        {
            LoggedIn = false;
            Email = email;
            Password = password;
            RunTask(() => _pandora.Connect(Email, Password));
        }

        public Station GetStationFromID(string stationID)
        {
            foreach (Station s in Stations)
            {
                if (stationID == s.ID)
                {
                    return s;
                }
            }

            return null;
        }

        public void PlayStation(Station station)
        {
            CurrentStation = station;

            RunTask(PlayThread);
        }

        public bool PlayStation(string stationID)
        {
            Station s = GetStationFromID(stationID);
            if (s != null)
            {
                PlayStation(s);
                return true;
            }

            return false;
        }

        public void SongThumbUp(Song song)
        {
            RunTask(() => song.Rate(SongRating.love));
        }

        public void SongThumbDown(Song song)
        {
            RunTask(() => song.Rate(SongRating.ban));
        }

        public void SongDeleteFeedback(Song song)
        {
            RunTask(() => song.Rate(SongRating.none));
        }

        public void SongTired(Song song)
        {
            RunTask(song.SetTired);
        }

        public void SongBookmarkArtist(Song song)
        {
            RunTask(song.BookmarkArtist);
        }

        public void SongBookmark(Song song)
        {
            RunTask(song.Bookmark);
        }

        public void StationRename(Station station, string name)
        {
            RunTask(() => station.Rename(name));
        }

        public void SetStationSortOrder(Pandora.SortOrder order)
        {
            _pandora.StationSortOrder = order;
        }

        public void SetStationSortOrder(string order)
        {
            Pandora.SortOrder sort = Pandora.SortOrder.DateDesc;
            Enum.TryParse(order, true, out sort);
            SetStationSortOrder(sort);
        }

        public void RefreshStations()
        {
            RunTask(() => _pandora.RefreshStations());
        }

        public void StationDelete(Station station)
        {
            RunTask(() =>
                        {
                            bool playQuickMix = (CurrentStation == null) ? false : (station.ID == CurrentStation.ID);
                            station.Delete();
                            _pandora.RefreshStations();
                            if (playQuickMix)
                            {
                                Log.O("Current station deleted, playing Quick Mix");
                                PlayStation(Stations[0]); //Set back to quickmix because current was deleted
                            }
                        });
        }

        public void StationSearchNew(string query)
        {
            RunTask(() =>
                        {
                            List<SearchResult> result = _pandora.Search(query);
                            if (SearchResult != null)
                                SearchResult(this, result);
                        });
        }

        public void CreateStationFromSong(Song song)
        {
            RunTask(() =>
            {
                Station station = _pandora.CreateStationFromSong(song);
                if (StationCreated != null)
                    StationCreated(this, station);
            });
        }

        public void CreateStationFromArtist(Song song)
        {
            RunTask(() =>
            {
                Station station = _pandora.CreateStationFromArtist(song);
                if (StationCreated != null)
                    StationCreated(this, station);
            });
        }

        public void CreateStation(SearchResult result)
        {
            RunTask(() =>
                        {
                            Station station = _pandora.CreateStationFromSearch(result.MusicToken);
                            if (StationCreated != null)
                                StationCreated(this, station);
                        });
        }

        public void SaveQuickMix()
        {
            RunTask(() =>
            {
                _pandora.SaveQuickMix();
            });
        }

        public void PlayPause()
        {
            RunTask(() => _bass.PlayPause());
        }

        public void Stop()
        {
            RunTask(() =>
                        {
                            CurrentStation = null;
                            _bass.Stop();
                        });
        }

        public void Next()
        {
            RunTask(() => PlayNextSong());
        }

        #endregion

        #region Pandora Handlers
        private void _pandora_StationsUpdatingEvent(object sender)
        {
            if (StationsRefreshing != null)
                StationsRefreshing(this);
        }

        void _pandora_QuickMixSavedEvent(object sender)
        {
            if (QuickMixSavedEvent != null)
                QuickMixSavedEvent(this);
        }

        private void _pandora_LoginStatusEvent(object sender, string status)
        {
            if (LoginStatusEvent != null)
                LoginStatusEvent(this, status);
        }

        private void _pandora_FeedbackUpdateEvent(object sender, Song song, bool success)
        {
            if (FeedbackUpdateEvent != null)
                FeedbackUpdateEvent(this, song, success);
        }

        private void _pandora_StationUpdateEvent(object sender)
        {
            if (StationsRefreshed != null)
                StationsRefreshed(this);
        }

        private void _playlist_PlayedSongDequeued(object sender, Song oldSong)
        {
            if (PlayedSongRemoved != null)
                PlayedSongRemoved(this, oldSong);
        }

        private void _playlist_PlayedSongQueued(object sender, Song newSong)
        {
            if (PlayedSongAdded != null)
                PlayedSongAdded(this, newSong);
        }

        private void _pandora_ConnectionEvent(object sender, bool state, ErrorCodes code)
        {
            LoggedIn = state;

            if (ConnectionEvent != null)
                ConnectionEvent(this, state, code);
        }
        #endregion

        #region Playlist Event Handlers

        private void _playlist_PlaylistLow(object sender, int count)
        {
            RunTask(() => UpdatePlaylist());
        }

        #endregion

        #region BassPlayer Event Handlers

        private void bass_PlaybackProgress(object sender, BassAudioEngine.Progress prog)
        {
            if (PlaybackProgress != null) PlaybackProgress(this, prog);
        }

        private void bass_PlaybackStateChanged(object sender, BassAudioEngine.PlayState oldState,
                                               BassAudioEngine.PlayState newState)
        {
            if (PlaybackStateChanged != null) PlaybackStateChanged(this, oldState, newState);

            Log.O("Playstate: " + newState);

            Paused = newState == BassAudioEngine.PlayState.Paused;
            Playing = newState == BassAudioEngine.PlayState.Playing;
            Stopped = newState == BassAudioEngine.PlayState.Ended || newState == BassAudioEngine.PlayState.Stopped;

            if (newState == BassAudioEngine.PlayState.Ended && CurrentStation != null)
            {
                Log.O("Song ended, playing next song.");
                RunTask(() => PlayNextSong());
            }
        }

        private void bass_PlaybackStart(object sender, double duration)
        {
            if (CurrentSong != null)
                CurrentSong.Played = true;

            if (PlaybackStart != null) PlaybackStart(this, duration);
        }

        private void bass_PlaybackStop(object sender)
        {
            if (PlaybackStop != null) PlaybackStop(this);
        }

        #endregion
    }
}