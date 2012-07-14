using System;

namespace Lpfm.LastFmScrobbler.Api
{
    /// <summary>
    /// A Last.fm API exception
    /// </summary>
    public class LastFmApiException : Exception
    {
        /// <summary>
        /// Instantiates a Last.fm API exception
        /// </summary>
        public LastFmApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Instantiates a Last.fm API exception
        /// </summary>
        public LastFmApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
