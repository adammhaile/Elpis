using System;
using System.Collections.Generic;

namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// A list of <see cref="ScrobbleResponse"/>
    /// </summary>
    public class ScrobbleResponses : List<ScrobbleResponse>
    {
        /// <summary>
        /// The number of Scrobbles in this list that were accepted by the web service
        /// </summary>
        public int AcceptedCount { get; set; }

        /// <summary>
        /// The number of Scrobbles in this list that were ignored by the web service
        /// </summary>
        public int IgnoredCount { get; set; }
    }

    /// <summary>
    /// An abstract Last.fm Scrobble response DTO
    /// </summary>
    public abstract class Response
    {
        protected Response()
        {
            Track = new CorrectedTrack();
        }

        /// <summary>
        /// A <see cref="Track"/>
        /// </summary>
        public Track Track { get; set; }

        /// <summary>
        /// The message provided by the web service if the scrobble was ignored
        /// </summary>
        public string IgnoredMessage { get; set; }

        /// <summary>
        /// The code provided by the web service if the scrobble was ignored
        /// </summary>
        public int IgnoredMessageCode { get; set; }

        /// <summary>
        /// The exception thrown when scrobbling (if any)
        /// </summary>
        public Exception Exception { get; set; }

        public int ErrorCode { get; set; }
    }

    /// <summary>
    /// A Now Playing Response DTO
    /// </summary>
    public class NowPlayingResponse : Response
    {}

    /// <summary>
    /// A Scrobble Response DTO
    /// </summary>
    public class ScrobbleResponse : Response
    {}

    /// <summary>
    /// A Rating (Love, UnLove, Ban, UnBan) Response DTO
    /// </summary>
    public class RatingResponse : Response
    {
        public bool Success { get { return ErrorCode == 0; } }
    }
}
