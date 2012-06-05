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
using Newtonsoft.Json.Linq;
using PandoraSharp.Exceptions;

namespace PandoraSharp
{
    public class Song
    {
        private readonly object _albumLock = new object();
        private readonly Pandora _pandora;
        private byte[] _albumImage;

        public Song(Pandora p, JToken song)
        {
            _pandora = p;

            TrackToken = (string)song["trackToken"];
            Artist = (string)song["artistName"];
            Album = (string)song["albumName"];

            AmazonAlbumID = (string)song["amazonAlbumDigitalAsin"];
            AmazonTrackID = (string)song["amazonSongDigitalAsin"];

            //"HTTP_64_AACPLUS_ADTS,HTTP_128_MP3,HTTP_192_MP3";
                
            //var aUrl = (string) song["audioURL"];
            //AudioUrl = aUrl.Substring(0, aUrl.Length - 48) +
            //           PandoraCrypt.Decrypt(aUrl.Substring(aUrl.Length - 48, 48));

            var songUrls = song["additionalAudioUrl"].ToObject<string[]>();
            if(songUrls.Length == 0)
            {                
                throw new PandoraException(ErrorCodes.NO_AUDIO_URLS);
            }
            else if (songUrls.Length == 1)
            {
                AudioUrl = songUrls[0];
            }
            else if(songUrls.Length > 1)
            {
                if (_pandora.AudioFormat == PAudioFormat.MP3_HIFI)
                {
                    if (songUrls.Length >= 3)
                        AudioUrl = songUrls[2];
                    else
                        AudioUrl = songUrls[1];
                }
                else if (_pandora.AudioFormat == PAudioFormat.AACPlus)
                {
                    AudioUrl = songUrls[0];
                }
                else //default to PAudioFormat.MP3
                {
                    AudioUrl = songUrls[1];
                }
            }

            double gain = 0.0;
            double.TryParse(((string)song["trackGain"]), out gain);
            FileGain = gain;

            Rating = (((int)song["songRating"]) > 0 ? SongRating.love : SongRating.none);
            StationID = (string)song["stationId"];
            SongTitle = (string)song["songName"];
            SongDetailUrl = (string)song["songDetailUrl"];
            ArtistDetailUrl = (string)song["artistDetailUrl"];
            AlbumDetailUrl = (string)song["albumDetailUrl"];
            AlbumArtUrl = (string)song["albumArtUrl"];
  
            Tired = false;
            StartTime = DateTime.MinValue;
            Finished = false;
            PlaylistTime = Time.Unix();

            if (!AlbumArtUrl.IsNullOrEmpty())
            {
                try
                {
                    AlbumImage = PRequest.ByteRequest(AlbumArtUrl);
                }
                catch { }
            }
        }

        public bool Played { get; set; }

        public string Album { get; private set; }
        public string Artist { get; private set; }
        public string AudioUrl { get; private set; }
        public double FileGain { get; private set; }
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
        public string SongDetailUrl { get; private set; }
        public string ArtistDetailUrl { get; set; }
        public string AlbumDetailUrl { get; private set; }
        public string AlbumArtUrl { get; private set; }

        public string AmazonAlbumID { get; private set; }
        public string AmazonTrackID { get; private set; }

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
        public DateTime StartTime { get; private set; }
        public bool Finished { get; private set; }
        public int PlaylistTime { get; private set; }

        public Station Station
        {
            get { return _pandora.GetStationByID(StationID); }
        }

        public string FeedbackID
        {
            get { return ""; }// _pandora.GetFeedbackID(StationID, MusicID); }
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
                        _pandora.AddFeedback(Station.IdToken, TrackToken, rating);

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
                    _pandora.CallRPC("user.sleepSong", "trackToken", TrackToken);
                    Tired = true;
                }
                catch { } //TODO: Give this a failed event to notify UI
            }
        }

        public void Bookmark()
        {
            try
            {
                _pandora.CallRPC("bookmark.addSongBookmark", "trackToken", TrackToken);
            }
            catch { } //TODO: Give this a failed event to notify UI
        }

        public void BookmarkArtist()
        {
            try
            {
                _pandora.CallRPC("bookmark.addArtistBookmark", "trackToken", TrackToken);
            }
            catch { } //TODO: Give this a failed event to notify UI
        }

        public void CreateStation()
        {

        }

        public override string ToString()
        {
            return Artist + " - " + SongTitle;
        }
    }
}