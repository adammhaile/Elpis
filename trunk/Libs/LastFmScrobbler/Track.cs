using System;
using System.ComponentModel.DataAnnotations;

namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// A DTO for Last.fm Track details
    /// </summary>
    public class Track
    {
        /// <summary>
        /// API param name for <see cref="TrackName"/>
        /// </summary>
        public const string TrackNameParamName = "track";

        /// <summary>
        /// API param name for <see cref="ArtistName"/>
        /// </summary>
        public const string ArtistNameParamName = "artist";

        /// <summary>
        /// API param name for <see cref="AlbumName"/>
        /// </summary>        
        public const string AlbumNameParamName = "album";

        /// <summary>
        /// API param name for <see cref="AlbumArtist"/>
        /// </summary>        
        public const string AlbumArtistParamName = "albumArtist";

        /// <summary>
        /// API param name for <see cref="TrackNumber"/>
        /// </summary>
        public const string TrackNumberParamName = "trackNumber";

        /// <summary>
        /// API param name for <see cref="MusicBrainzId"/>
        /// </summary>
        public const string MusicBrainzIdParamName = "mbid";

        /// <summary>
        /// API param name for <see cref="Duration"/>
        /// </summary>
        public const string DurationParamName = "duration";

        /// <summary>
        /// API param name for <see cref="WhenStartedPlaying"/>
        /// </summary>
        public const string WhenStartedPlayingParamName = "timestamp";

        /// <summary>
        /// Required. The track name
        /// </summary>
        [Required]
        public string TrackName { get; set; }

        /// <summary>
        /// Required. The artist name
        /// </summary>
        [Required]
        public string ArtistName { get; set; }
        
        /// <summary>
        /// Optional. The album name
        /// </summary>
        public string AlbumName { get; set; }
        
        /// <summary>
        /// Optional. The album artist - if this differs from the track artist
        /// </summary>
        public string AlbumArtist { get; set; }

        /// <summary>
        /// Optional. The track number of the track on the album
        /// </summary>
        public int TrackNumber { get; set; }

        /// <summary>
        /// Optional. The MusicBrainz Track ID
        /// </summary>
        public int MusicBrainzId { get; set; }

        /// <summary>
        /// The length of the track
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Required for Scrobbling, not used for Now Playing. The Date & time that the track started playing in local time. 
        /// The DateTime will be converted to UTC using <see cref="DateTime.ToUniversalTime"/>
        /// </summary>
        public DateTime? WhenStartedPlaying { get; set; }

    }
}
