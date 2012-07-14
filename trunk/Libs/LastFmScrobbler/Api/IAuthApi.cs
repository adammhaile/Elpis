namespace Lpfm.LastFmScrobbler.Api
{
    /// <summary>
    /// Defines a Last.fm Auth API
    /// </summary>
    public interface IAuthApi
    {
        /// <summary>
        /// Fetch an unathorized request token for an API account
        /// </summary>
        AuthenticationToken GetToken(Authentication authentication);

        /// <summary>
        /// Fetch a session key for a user
        /// </summary>
        void GetSession(Authentication authentication, AuthenticationToken token);
    }
}