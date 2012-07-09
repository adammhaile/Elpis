using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PandoraSharpPlayer
{
    public class QuerySong 
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Song { get; set; }
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

        public delegate void StatusUpdate(QueryStatus status);
        virtual public void StatsusUpdateReceiver(QueryStatus status) { throw new NotImplementedException(); }

        //TODO: Add Control Delegates
    }
}
