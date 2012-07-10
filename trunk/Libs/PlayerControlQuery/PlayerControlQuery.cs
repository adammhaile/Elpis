using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandoraSharp.ControlQuery
{
    public class QuerySong 
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
    }

    public class QueryTrackProgress
    {
        public TimeSpan TotalTime { get; set; }
        public TimeSpan ElapsedTime { get; set; }

        public TimeSpan RemainingTime
        {
            get { return TotalTime - ElapsedTime; }
        }

        public double Percent
        {
            get
            {
                if (TotalTime.Ticks == 0)
                    return 0.0;

                return ((ElapsedTime.TotalSeconds / TotalTime.TotalSeconds) * 100);
            }
        }
    }

    public class QueryProgress
    {
        public QuerySong Song { get; set; }
        public QueryTrackProgress Progress { get; set; } 
    }

    public enum QueryStatusValue
    {
        Waiting,
        Connecting,
        Connected,
        Disconnected,
        StationLoading,
        StationLoaded,
        Playing,
        Paused,
        Stopped,
        Error
    }

    public class QueryStatus
    {
        public QueryStatusValue PreviousStatus { get; set; }
        public QueryStatusValue CurrentStatus { get; set; }
    }

    public abstract class PlayerControlQuery
    {
        public PlayerControlQuery(){}

        //Query Delegates
        public delegate void SongUpdate(QuerySong song);
        virtual public void SongUpdateReceiver(QuerySong song) { throw new NotImplementedException(); }

        public delegate void ProgressUpdate(QueryProgress progress);
        virtual public void ProgressUpdateReciever(QueryProgress progress) { throw new NotImplementedException(); }

        public delegate void StatusUpdate(QueryStatus status);
        virtual public void StatsusUpdateReceiver(QueryStatus status) { throw new NotImplementedException(); }

        //TODO: Add Control Delegates
    }
}

