using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;
using PandoraSharp;

namespace PandoraSharp.ControlQuery
{
    public class ControlQueryManager
    {
        private List<IPlayerControlQuery> _pcqList;

        private object _lastQueryStatusLock = new object();
        private QueryStatusValue _lastQueryStatus = QueryStatusValue.Waiting;

        private object _lastQuerySongLock = new object();
        private Song _lastQuerySong;

        public ControlQueryManager()
        {
            _lastQuerySong = null;

            _pcqList = new List<IPlayerControlQuery>();
        }

        public Song LastSong { get { lock (_lastQuerySongLock) { return _lastQuerySong; } } }
        public QueryStatusValue LastQueryStatus { get { lock (_lastQueryStatusLock) { return _lastQueryStatus; } } }

        public void RegisterPlayerControlQuery(IPlayerControlQuery obj)
        {
            _pcqList.Add(obj);

            obj.PlayStateRequest += PlayStateRequestHandler;
            obj.PlayRequest += PlayRequestHandler;
            obj.PauseRequest += PauseRequestHandler;
            obj.NextRequest += NextRequestHandler;
            obj.StopRequest += StopRequestHandler;

            obj.SetSongMetaRequest += obj_SetSongMetaRequest;
        }

        public event PlayStateRequestEvent PlayStateRequest;
        public event PlayRequestEvent PlayRequest;
        public event PauseRequestEvent PauseRequest;
        public event NextRequestEvent NextRequest;
        public event StopRequestEvent StopRequest;

        public event SetSongMetaRequestEvent SetSongMetaRequest;

        void obj_SetSongMetaRequest(object sender, object meta)
        {
            if (SetSongMetaRequest != null) SetSongMetaRequest(sender, meta);
        }

        void StopRequestHandler(object sender)
        {
            if (StopRequest != null) StopRequest(sender);
        }

        void NextRequestHandler(object sender)
        {
            if (NextRequest != null) NextRequest(sender);
        }

        void PauseRequestHandler(object sender)
        {
            if (PauseRequest != null) PauseRequest(sender);
        }

        void PlayRequestHandler(object sender)
        {
            if (PlayRequest != null) PlayRequest(sender);
        }

        QueryStatusValue PlayStateRequestHandler(object sender)
        {
            if (PlayStateRequest != null) return PlayStateRequest(sender);
            else return QueryStatusValue.Invalid;
        }

        public void SendSongUpdate(Song song)
        {
            lock (_lastQuerySongLock)
            {
                _lastQuerySong = song;
            }

            Log.O("Song Update: {0} | {1} | {2}", song.Artist, song.Album, song.SongTitle);
            foreach (var obj in _pcqList)
            {
                obj.SongUpdateReceiver(new QuerySong() 
                { 
                    Artist = song.Artist,
                    Album = song.Album,
                    Title = song.SongTitle,
                    Meta = song.GetMetaObject(obj)
                });
            }
        }

        public void SendStatusUpdate(QueryStatus status)
        {
            lock (_lastQueryStatusLock)
            {
                _lastQueryStatus = status.CurrentStatus;
            }

            Log.O("Status Update: {0} -> {1}",
                status.PreviousStatus.ToString(),
                status.CurrentStatus.ToString());

            foreach (var obj in _pcqList)
            {
                obj.StatusUpdateReceiver(status);
            }
        }

        public void SendStatusUpdate(QueryStatusValue previous, QueryStatusValue current)
        {
            SendStatusUpdate(new QueryStatus() { PreviousStatus = previous, CurrentStatus = current });
        }

        public void SendStatusUpdate(QueryStatusValue current)
        {
            SendStatusUpdate(_lastQueryStatus, current);
        }

        public void SendProgressUpdate(Song song, QueryTrackProgress progress)
        {
            foreach (var obj in _pcqList)
            {
                var prog = new QueryProgress()
                {
                    Song = new QuerySong()
                    {
                        Artist = song.Artist,
                        Album = song.Album,
                        Title = song.SongTitle,
                        Meta = song.GetMetaObject(obj)
                    },
                    Progress = progress
                };

                obj.ProgressUpdateReciever(prog);
            }
        }

        public void SendProgressUpdate(Song song, TimeSpan TotalTime, TimeSpan ElapsedTime)
        {
            SendProgressUpdate(song, new QueryTrackProgress() { TotalTime = TotalTime, ElapsedTime = ElapsedTime });
        }

        public void SendRatingUpdate(QuerySong song, SongRating oldRating, SongRating newRating)
        {
            foreach (var obj in _pcqList)
            {
                obj.RatingUpdateReceiver(song, oldRating, newRating);
            }
        }

        public void SendRatingUpdate(string artist, string album, string song, SongRating oldRating, SongRating newRating)
        {
            SendRatingUpdate(new QuerySong() { Artist = artist, Album = album, Title = song }, oldRating, newRating);
        }
    }
}
