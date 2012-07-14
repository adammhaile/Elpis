using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace PandoraSharp.ControlQuery
{
    public class ControlQueryManager
    {
        private List<SongUpdate> _songUpdateDelegates;
        private List<StatusUpdate> _statusUpdateDelegates;
        private List<ProgressUpdate> _progressUpdateDelegates;

        private object _lastQueryStatusLock = new object();
        private QueryStatusValue _lastQueryStatus = QueryStatusValue.Waiting;

        private object _lastQuerySongLock = new object();
        private QuerySong _lastQuerySong;

        private object _lastProgressLock = new object();
        private QueryProgress _lastProgress;

        public ControlQueryManager()
        {
            _lastQuerySong = new QuerySong();
            _songUpdateDelegates = new List<SongUpdate>();
            _statusUpdateDelegates = new List<StatusUpdate>();
            _progressUpdateDelegates = new List<ProgressUpdate>();
        }

        public QuerySong LastSong { get { lock (_lastQuerySongLock) { return _lastQuerySong; } } }

        public void RegisterPlayerControlQuery(IPlayerControlQuery obj)
        {
            _songUpdateDelegates.Add(obj.SongUpdateReceiver);
            _statusUpdateDelegates.Add(obj.StatsusUpdateReceiver);
            _progressUpdateDelegates.Add(obj.ProgressUpdateReciever);
        }

        public void SendSongUpdate(QuerySong song)
        {
            lock (_lastQuerySongLock)
            {
                _lastQuerySong = song;
            }

            Log.O("Song Update: {0} | {1} | {2}", song.Artist, song.Album, song.Title);
            foreach (var del in _songUpdateDelegates)
            {
                del(song);
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
            foreach (var del in _statusUpdateDelegates)
            {
                del(status);
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

            foreach (var del in _progressUpdateDelegates)
            {
                del(progress);
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
    }
}
