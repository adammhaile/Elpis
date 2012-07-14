namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// A Last.fm Authentication DTO
    /// </summary>
    /// <remarks>See http://www.last.fm/api/desktopauth </remarks>
    public class Authentication
    {
        /// <summary>
        /// An API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// An API Secret
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// A <see cref="Session"/>
        /// </summary>
        public Session Session { get; set; }
    }
}
