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
using System.Net;
using Util;

namespace PandoraSharp
{
    public class Song
    {
        private readonly object _albumLock = new object();
        private readonly Pandora _pandora;
        private byte[] _albumImage;

        public Song(Pandora p, PDict d)
        {
            _pandora = p;

            Album = (string) d["albumTitle"];
            Artist = (string) d["artistSummary"];
            ArtistMusicID = (string) d["artistMusicId"];

            var aUrl = (string) d["audioURL"];
            AudioUrl = aUrl.Substring(0, aUrl.Length - 48) +
                       PandoraCrypt.Decrypt(aUrl.Substring(aUrl.Length - 48, 48));

            double gain = 0.0;
            double.TryParse(((string) d["fileGain"]), out gain);
            FileGain = gain;

            Identity = (string) d["identity"];
            MusicID = (string) d["musicId"];
            TrackToken = (string) d["trackToken"];
            Rating = (((int) d["rating"]) > 0 ? SongRating.love : SongRating.none);
            StationID = (string) d["stationId"];
            SongTitle = (string) d["songTitle"];
            UserSeed = (string) d["userSeed"];
            SongDetailUrl = (string) d["songDetailURL"];
            ArtistDetailUrl = (string) d["artistDetailURL"];
            AlbumDetailUrl = (string) d["albumDetailURL"];
            ArtRadio = (string) d["artRadio"];
            //SongType = (int) d["songType"];

            Tired = false;
            Message = "";
            StartTime = DateTime.MinValue;
            Finished = false;
            PlaylistTime = Time.Unix();

            if (ArtRadio != string.Empty)
            {
                try
                {
                    AlbumImage = PRequest.ByteRequest(ArtRadio);
                }
                catch { }
                //PRequest.ByteRequestAsync(this.ArtRadio, AlbumArtDownloadHandler);
            }
        }

        public bool Played { get; set; }

        public string Album { get; private set; }
        public string Artist { get; private set; }
        public string ArtistMusicID { get; private set; }
        public string AudioUrl { get; private set; }
        public double FileGain { get; private set; }
        public string Identity { get; private set; }
        public string MusicID { get; private set; }
        public SongRating Rating { get; private set; }

        public bool Loved
        {
            get { return Rating == SongRating.love; }
        }

        public bool Banned
        {
            get { return Rating == SongRating.ban; }
        }

        public string RatingString
        {
            get { return Rating.ToString(); }
        }

        public string StationID { get; private set; }
        public string TrackToken { get; private set; }
        public string SongTitle { get; private set; }
        public string UserSeed { get; private set; }
        public string SongDetailUrl { get; private set; }
        public string ArtistDetailUrl { get; set; }
        public string AlbumDetailUrl { get; private set; }
        public string ArtRadio { get; private set; }

        public byte[] AlbumImage
        {
            get
            {
                lock (_albumLock)
                {
                    return _albumImage;
                }
            }
            private set
            {
                lock (_albumLock)
                {
                    _albumImage = value;
                }
            }
        }

        //public int SongType { get; private set; }

        public bool Tired { get; private set; }
        public string Message { get; private set; }
        public DateTime StartTime { get; private set; }
        public bool Finished { get; private set; }
        public int PlaylistTime { get; private set; }

        public Station Station
        {
            get { return _pandora.GetStationByID(StationID); }
        }

        public string FeedbackID
        {
            get { return _pandora.GetFeedbackID(StationID, MusicID); }
        }

        public bool IsStillValid
        {
            get { return ((Time.Unix() - PlaylistTime) < Const.PLAYLIST_VALIDITY_TIME); }
        }

        private void AlbumArtDownloadHandler(object sender, DownloadDataCompletedEventArgs e)
        {
            //if error or zero length, we don't care, empty image
            if (e.Error != null || e.Result.Length == 0)
                return;

            AlbumImage = e.Result;
        }

        public void Rate(SongRating rating)
        {
            if (Rating != rating)
            {
                try
                {
                    Station.TransformIfShared();
                    if (rating == SongRating.none)
                        _pandora.DeleteFeedback(FeedbackID);
                    else
                        _pandora.AddFeedback(StationID, TrackToken, rating);

                    Rating = rating;
                    _pandora.CallFeedbackUpdateEvent(this, true);
                }
                catch (Exception ex)
                {
                    Log.O(ex.ToString());
                    _pandora.CallFeedbackUpdateEvent(this, false);
                }
            }
        }

        public void SetTired()
        {
            if (!Tired)
            {
                try
                {
                    _pandora.CallRPC("listener.addTiredSong", new object[] { Identity });
                    Tired = true;
                }
                catch { } //TODO: Give this a failed event to notify UI
            }
        }

        public void Bookmark()
        {
            try
            {
                _pandora.CallRPC("station.createBookmark", new object[] { StationID, MusicID });
            }
            catch { } //TODO: Give this a failed event to notify UI
        }

        public void BookmarkArtist()
        {
            try
            {
                _pandora.CallRPC("station.createArtistBookmark", new object[] { ArtistMusicID });
            }
            catch { } //TODO: Give this a failed event to notify UI
        }

        public override string ToString()
        {
            return Artist + " - " + SongTitle;
        }
    }
}