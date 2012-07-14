namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// Extends a Track DTO with field corrected meta-data
    /// </summary>
    public class CorrectedTrack : Track
    {
        /// <summary>
        /// True when the Track was corrected by the web service
        /// </summary>
        public bool TrackNameCorrected { get; set; }

        /// <summary>
        /// True when the Artist was corrected by the web service
        /// </summary>
        public bool ArtistNameCorrected { get; set; }

        /// <summary>
        /// True when the Album was corrected by the web service
        /// </summary>
        public bool AlbumNameCorrected { get; set; }

        /// <summary>
        /// True when the Album Artist was corrected by the web service
        /// </summary>
        public bool AlbumArtistCorrected { get; set; }
    }
}
