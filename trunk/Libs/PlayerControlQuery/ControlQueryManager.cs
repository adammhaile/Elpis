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
        private QuerySong _lastQuerySong;

        private object _lastProgressLock = new object();
        private QueryProgress _lastProgress;

        public ControlQueryManager()
        {
            _lastQuerySong = new QuerySong();
            _lastProgress = new QueryProgress();

            _pcqList = new List<IPlayerControlQuery>();
        }

        public QuerySong LastSong { get { lock (_lastQuerySongLock) { return _lastQuerySong; } } }
        public QueryStatusValue LastQueryStatus { get { lock (_lastQueryStatusLock) { return _lastQueryStatus; } } }

        public void RegisterPlayerControlQuery(IPlayerControlQuery obj)
        {
            _pcqList.Add(obj);

            obj.PlayStateRequest += PlayStateRequestHandler;
            obj.PlayRequest += PlayRequestHandler;
            obj.PauseRequest += PauseRequestHandler;
            obj.NextRequest += NextRequestHandler;
            obj.StopRequest += StopRequestHandler;
        }

        public event PlayStateRequestEvent PlayStateRequest;
        public event PlayRequestEvent PlayRequest;
        public event PauseRequestEvent PauseRequest;
        public event NextRequestEvent NextRequest;
        public event StopRequestEvent StopRequest;

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

        public void SendSongUpdate(QuerySong song)
        {
            lock (_lastQuerySongLock)
            {
                _lastQuerySong = song;
            }

            Log.O("Song Update: {0} | {1} | {2}", song.Artist, song.Album, song.Title);
            foreach (var obj in _pcqList)
            {
                obj.SongUpdateReceiver(song);
            }
        }

        public void SendSongUpdate(string artist, string album, string song)
        {
            SendSongUpdate(new QuerySong() { Artist = artist, Album = album, Title = song});
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

        public void SendProgressUpdate(QueryProgress progress)
        {
            lock (_lastProgressLock)
            {
                _lastProgress = progress;
            }

            foreach (var obj in _pcqList)
            {
                obj.ProgressUpdateReciever(progress);
            }
        }

        public void SendProgressUpdate(QuerySong song, QueryTrackProgress progress)
        {
            SendProgressUpdate(new QueryProgress() { Song = song, Progress = progress });
        }

        public void SendProgressUpdate(QuerySong song, TimeSpan TotalTime, TimeSpan ElapsedTime)
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
