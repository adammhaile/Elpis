namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// A Last.fm Session DTO
    /// </summary>
    /// <remarks>Only <see cref="Key"/> is used by LPFM and is reliably persisted</remarks>
    public class Session
    {
        /// <summary>
        /// The Username. Can be Null
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The Session Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// True if the user is currently a subscriber. Is not always set - can be false by default
        /// </summary>
        public bool IsSubscriber { get; set; }
    }
}
