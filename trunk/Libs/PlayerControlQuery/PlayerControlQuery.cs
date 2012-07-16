using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PandoraSharp;

namespace PandoraSharp.ControlQuery
{
    public class QuerySong 
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public SongRating Rating { get; set; }
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
        Invalid,
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

    public delegate void SongUpdate(QuerySong song);
    public delegate void ProgressUpdate(QueryProgress progress);
    public delegate void StatusUpdate(QueryStatus status);
    public delegate void RatingUpdate(QuerySong song, SongRating rating);

    public delegate QueryStatusValue PlayStateRequestEvent(object sender);
    public delegate void PlayRequestEvent(object sender);
    public delegate void PauseRequestEvent(object sender);
    public delegate void NextRequestEvent(object sender);
    public delegate void StopRequestEvent(object sender);

    public interface IPlayerControlQuery
    {    
        void SongUpdateReceiver(QuerySong song);
    
        void ProgressUpdateReciever(QueryProgress progress);

        void StatusUpdateReceiver(QueryStatus status);

        void RatingUpdateReceiver(QuerySong song, SongRating oldRating, SongRating newRating);

        event PlayStateRequestEvent PlayStateRequest;
        event PlayRequestEvent PlayRequest;
        event PauseRequestEvent PauseRequest;
        event NextRequestEvent NextRequest;
        event StopRequestEvent StopRequest;

        //TODO: Add Control Methods
    }
}

