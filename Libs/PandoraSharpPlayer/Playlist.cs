/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using PandoraSharp;
using PandoraSharp.Exceptions;
using Util;

namespace PandoraSharpPlayer
{
    public class Playlist
    {
        #region Delegates

        public delegate void CurrentSongChangedHandler(object sender, Song newSong);

        public delegate void PlayedSongDequeuedHandler(object sender, Song oldSong);

        public delegate void PlayedSongQueuedHandler(object sender, Song newSong);

        public delegate void PlaylistLowHandler(object sender, int count);

        #endregion

        private readonly object _currentLock = new object();

        private readonly ConcurrentQueue<Song> _nextSongs;
        private readonly ConcurrentQueue<Song> _playedSongs;
        private Song _currentSong;

        private bool _emptyPlaylist = false;

        public Playlist(int maxPlayed = 4, int lowCount = 1)
        {
            MaxPlayed = maxPlayed;
            LowPlaylistCount = lowCount;
            _nextSongs = new ConcurrentQueue<Song>();
            _playedSongs = new ConcurrentQueue<Song>();
        }

        public int MaxPlayed { get; set; }
        public int LowPlaylistCount { get; set; }

        public Song Current
        {
            get
            {
                lock (_currentLock)
                {
                    return _currentSong;
                }
            }

            set
            {
                lock (_currentLock)
                {
                    _currentSong = value;
                }
            }
        }

        public event PlaylistLowHandler PlaylistLow;
        public event CurrentSongChangedHandler CurrentSongChanged;
        public event PlayedSongDequeuedHandler PlayedSongDequeued;
        public event PlayedSongQueuedHandler PlayedSongQueued;


        public void ClearSongs()
        {
            Song trash = null;
            while (_nextSongs.TryDequeue(out trash))
            {
            }
        }

        public void ClearHistory()
        {
            Song trash = null;
            while (_playedSongs.TryDequeue(out trash))
            {
            }
        }

        public int AddSongs(List<Song> songs)
        {
            if(songs.Count == 0)
            {
                _emptyPlaylist = true;
                return 0;
            }

            foreach (Song s in songs)
            {
                Log.O("Adding: " + s);
                _nextSongs.Enqueue(s);
            }

            return songs.Count;
        }

        private void WaitForPlaylistReload()
        {
            DateTime start = DateTime.Now;
            Log.O("Waiting for playlist to reload.");
            while (_nextSongs.IsEmpty && !_emptyPlaylist)
            {
                if ((DateTime.Now - start).TotalSeconds >= 20)
                {
                    Log.O("Playlist did not reload within 20 seconds, ");
                    throw new PandoraException(ErrorCodes._END_OF_PLAYLIST);
                }

                Thread.Sleep(25);
            }

            if (_emptyPlaylist)
            {
                Log.O("WaitForPlaylist: Still Empty");
                throw new PandoraException(ErrorCodes._END_OF_PLAYLIST);
            }

            Log.O("WaitForPlaylist: Complete");
        }

        private Song DequeueSong()
        {
            Song result = null;
            if(!_nextSongs.TryDequeue(out result))
                throw new PandoraException(ErrorCodes._END_OF_PLAYLIST);

            return result;
        }

        private void SendPlaylistLow()
        {
            _emptyPlaylist = false;
            if (PlaylistLow != null)
                    PlaylistLow(this, _nextSongs.Count);
        }

        public void DoReload()
        {

            ClearSongs();
            SendPlaylistLow();

            try
            {
                WaitForPlaylistReload();
            }
            catch
            {
                if (_nextSongs.IsEmpty)
                    throw;
            }
        }

        public Song NextSong()
        {
            if (_nextSongs.IsEmpty)
            {
                Log.O("PlaylistEmpty - Reloading");

                DoReload();
            }

            var next = DequeueSong();

            if(!next.IsStillValid)
            {
                Log.O("Song was invalid, reloading and skipping any more invalid songs.");
                //clear songs that are now invalid
                DoReload();
            }

            Log.O("NextSong: " + next);
            Song oldSong = Current;

            Current = next;

            if (_nextSongs.Count <= LowPlaylistCount)
            {
                Log.O("PlaylistLow");
                SendPlaylistLow();
            }

            if (CurrentSongChanged != null)
                CurrentSongChanged(this, Current);

            if (oldSong != null && oldSong.Played)
            {
                _playedSongs.Enqueue(oldSong);
                Log.O("SongQueued");
                if (PlayedSongQueued != null)
                    PlayedSongQueued(this, oldSong);
            }

            if (_playedSongs.Count > MaxPlayed)
            {
                Song trash = null;
                if (_playedSongs.TryDequeue(out trash))
                {
                    Log.O("OldSongDequeued");
                    if (PlayedSongDequeued != null)
                        PlayedSongDequeued(this, trash);
                }
            }

            return Current;
        }

        #region Nested type: PlaylistEmptyException

        public class PlaylistEmptyException : Exception
        {
        }

        #endregion
    }
}