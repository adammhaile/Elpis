using System.Collections.Generic;

namespace Lpfm.LastFmScrobbler.Api
{
    /// <summary>
    /// Defines an API for the Scrobbling methods of the <see cref="http://www.last.fm/api/scrobbling">Last.fm Track web service</see>
    /// </summary>
    public interface ITrackApi
    {
        /// <summary>
        /// Notifies Last.fm that a user has started listening to a track. 
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>
        /// <returns>A <see cref="ScrobbleResponse"/>DTO containing details of Last.FM's response</returns>
        /// <remarks>It is important to not use the corrections returned by the now playing service as input for the scrobble request, 
        /// unless they have been explicitly approved by the user</remarks>
        NowPlayingResponse UpdateNowPlaying(Track track, Authentication authentication);

        /// <summary>
        /// Notifies Last.fm that a user has started listening to a track. 
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="context">Optional. Sub-client version (not public, only enabled for certain API keys). Null if not used</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>
        /// <returns>A <see cref="ScrobbleResponse"/>DTO containing details of Last.FM's response</returns>
        /// <remarks>It is important to not use the corrections returned by the now playing service as input for the scrobble request, 
        /// unless they have been explicitly approved by the user</remarks>
        NowPlayingResponse UpdateNowPlaying(Track track, string context, Authentication authentication);

        /// <summary>
        /// Add a track-play to a user's profile
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>
        /// <returns>A <see cref="ScrobbleResponse"/>DTO containing details of Last.FM's response</returns>
        ScrobbleResponse Scrobble(Track track, Authentication authentication);

        /// <summary>
        /// Add a track-play to a user's profile
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="context">Optional. Sub-client version (not public, only enabled for certain API keys). Null if not used</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>
        /// <returns>A <see cref="ScrobbleResponse"/>DTO containing details of Last.FM's response</returns>
        ScrobbleResponse Scrobble(Track track, string context, Authentication authentication);

        /// <summary>
        /// Add a track-play to a user's profile
        /// </summary>
        /// <param name="tracks">A list of <see cref="Track"/></param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>
        /// <returns>A <see cref="ScrobbleResponse"/>DTO containing details of Last.FM's response</returns>
        ScrobbleResponses Scrobble(IList<Track> tracks, Authentication authentication);

        /// <summary>
        /// Add a track-play to a user's profile
        /// </summary>
        /// <param name="tracks">A list of <see cref="Track"/></param>
        /// <param name="context">Optional. Sub-client version (not public, only enabled for certain API keys). Null if not used</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>
        /// <returns>A <see cref="ScrobbleResponse"/>DTO containing details of Last.FM's response</returns>
        ScrobbleResponses Scrobble(IList<Track> tracks, string context, Authentication authentication);

        /// <summary>
        /// Notifies Last.FM that the user Loves the track
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>     
        /// <returns>int LastFM return code. 0 is Success, above 0 is failure</returns>
        RatingResponse Love(Track track, Authentication authentication);

        /// <summary>
        /// Notifies Last.FM that the user UnLoves the track
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>     
        /// <returns>int LastFM return code. 0 is Success, above 0 is failure</returns>
        RatingResponse UnLove(Track track, Authentication authentication);

        /// <summary>
        /// Notifies Last.FM that the user wants to Ban the track
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>     
        /// <returns>int LastFM return code. 0 is Success, above 0 is failure</returns>
        RatingResponse Ban(Track track, Authentication authentication);

        /// <summary>
        /// Notifies Last.FM that the user wants to UnBan the track
        /// </summary>
        /// <param name="track">A <see cref="Track"/> DTO containing track details</param>
        /// <param name="authentication"><see cref="Authentication"/> object</param>     
        /// <returns>int LastFM return code. 0 is Success, above 0 is failure</returns>
        RatingResponse UnBan(Track track, Authentication authentication);
    }
}
