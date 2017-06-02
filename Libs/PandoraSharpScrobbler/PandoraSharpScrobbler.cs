using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PandoraSharp;
using PandoraSharp.ControlQuery;
using Util;
using Lpfm.LastFmScrobbler;
using Lpfm.LastFmScrobbler.Api;
using System.Diagnostics;
using System.Net;
using PandoraSharp;

namespace PandoraSharp.Plugins
{
    public class PandoraSharpScrobbler : IPlayerControlQuery
    /* This class maintains a Last.fm ScrobbleManager object, monitors Elpis's
     * play progress and adds tracks to the scrobbling queue as they satisfy 
     * the 'played' definition - currently hardcoded to half the tracklength */
    {
        private const double PercentNowPlaying = 5.0;
        private const double SecondsBeforeScrobble = 4*60 + 5;
        private const double PercentBeforeScrobble = 51.0;
        private const double MinTrackLength = 35.0; //buffered to 35 seconds, just in case 

        private bool _doneScrobble;
        private bool _doneNowPlaying;
        private QuerySong _currentSong;

        QueuingScrobbler _scrobbler;

        ProcessScrobblesDelegate _processScrobbleDelegate;

        private bool _isEnabled = false;
        public bool IsEnabled { get { return _isEnabled && APIKey != "dummy_key"; } set { _isEnabled = value; } }

        public string APIKey { get; set; }
        public string APISecret { get; set; }
        private string _sessionKey = string.Empty;
        public string SessionKey 
        { 
            get { return _sessionKey; } 
            set { _sessionKey = value; InitScrobblers(); } 
        }
        public string AuthURL { get; set; }

        private delegate void ProcessScrobblesDelegate();

        public PandoraSharpScrobbler(string lastFMApiKey, string lastFMApiSecret, string lastFMSessionKey = null)
        {
            APIKey = lastFMApiKey;
            APISecret = lastFMApiSecret;
            SessionKey = lastFMSessionKey;
            AuthURL = string.Empty;

            InitScrobblers();

            _processScrobbleDelegate = new ProcessScrobblesDelegate(DoScrobbles);
        }

        private void InitScrobblers()
        {
            Log.O("Initializing Scrobblers...");
            try
            {
                _scrobbler = new QueuingScrobbler(APIKey, APISecret, SessionKey);
            }
            catch(ArgumentNullException e)
            {
                Log.O(e.ParamName + " was empty or null, exiting scrobblers");
            }
        }

        private void DoScrobbles()
        {
            try
            {
                var response = _scrobbler.Process();
                if (response.Count == 1)
                {
                    if (response[0].Exception != null)
                    {
                        Log.O("Last.FM Error!: " + response[0].Exception.ToString());
                    }
                    if (response[0] is NowPlayingResponse || response[0] is ScrobbleResponse)
                    {
                        SetSongMetaRequest(this, response[0].Track);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.O("Last.FM Error!: " + ex.ToString());
            }
        }

        private void ProcessScrobbles()
        {
            _processScrobbleDelegate.BeginInvoke(null, null);
        }
        
        public string GetAuthUrl()
        {
            AuthURL = _scrobbler.BaseScrobbler.GetAuthorisationUri();
            return AuthURL;
        }

        public void LaunchAuthPage()
        {
            if (AuthURL == string.Empty)
                AuthURL = GetAuthUrl();

            Process.Start(AuthURL); 
        }

        public string GetAuthSessionKey()
        {
            try
            {
                SessionKey = _scrobbler.BaseScrobbler.GetSession();
            }
            catch (LastFmApiException exception)
            {
                Log.O("LastFM Error: " + exception.ToString());
                throw;
            }
            return SessionKey;
        }

        private Track QueryProgressToTrack(QueryProgress prog)
        {
            var track = new Track
            {
                TrackName = prog.Song.Title,
                AlbumName = prog.Song.Album,
                ArtistName = prog.Song.Artist,
                Duration = prog.Progress.TotalTime,
                WhenStartedPlaying = DateTime.Now.Subtract(prog.Progress.ElapsedTime),
            };
            return track;
        }

        private Track QuerySongToTrack(QuerySong song)
        {
            var track = new Track
            {
                TrackName = song.Title,
                AlbumName = song.Album,
                ArtistName = song.Artist,
            };
            return track;
        }

        public void SetProxy(string address, int port, string user = "", string password = "")
        {
            var p = new WebProxy(address, port);

            if (user != "")
                p.Credentials = new NetworkCredential(user, password);

            Scrobbler.SetWebProxy(p);
        }

        #region IPlayerControlQuery Members

        public event PlayStateRequestEvent PlayStateRequest;
        public event PlayRequestEvent PlayRequest;
        public event PauseRequestEvent PauseRequest;
        public event NextRequestEvent NextRequest;
        public event StopRequestEvent StopRequest;

        public event SetSongMetaRequestEvent SetSongMetaRequest;

        public void SongUpdateReceiver(QuerySong song)
        {
            //Nothing to do here
        }

        public void StatusUpdateReceiver(QueryStatus status)
        {
            //Nothing to do here
            if (status.CurrentStatus == QueryStatusValue.Playing && status.PreviousStatus != QueryStatusValue.Paused)
            {
                _doneNowPlaying = false;
            }
        }

        public void ProgressUpdateReciever(QueryProgress progress)
        {
            if (!IsEnabled || !_scrobbler.BaseScrobbler.HasSession) return;

            try
            {
                if (progress.Progress.Percent < PercentNowPlaying && !_doneNowPlaying)
                {
                    _doneScrobble = false;
                    Log.O("LastFM, Now Playing: {0} - {1}", progress.Song.Artist, progress.Song.Title);
                    _currentSong = progress.Song;
                    _scrobbler.NowPlaying(QueryProgressToTrack(progress));
                    _doneNowPlaying = true;
                }

                //A track should only be scrobbled when the following conditions have been met: 
                //The track must be longer than 30 seconds. 
                //And the track has been played for at least half its duration, 
                //or for 4 minutes (whichever occurs earlier). 
                //See http://www.last.fm/api/scrobbling
                //This is enforced by LPFM so might as well enforce it here to avoid issues
                if (progress.Progress.TotalTime.TotalSeconds >= MinTrackLength && 
                    (progress.Progress.ElapsedTime.TotalSeconds >= SecondsBeforeScrobble || progress.Progress.Percent > PercentBeforeScrobble) && 
                    !_doneScrobble)
                {
                    _doneNowPlaying = false;
                    Log.O("LastFM, Scrobbling: {0} - {1}", progress.Song.Artist, progress.Song.Title);
                    _scrobbler.Scrobble(QueryProgressToTrack(progress));
                    _doneScrobble = true;
                }

                if (_scrobbler.QueuedCount > 0) ProcessScrobbles();
            }
            catch (Exception ex)
            {
                Log.O("Last.FM Error!: " + ex.ToString());
            }
        }

        public void RatingUpdateReceiver(QuerySong song, SongRating oldRating, SongRating newRating)
        {
            if (!IsEnabled || !_scrobbler.BaseScrobbler.HasSession) return;

            try
            {
                Log.O("LastFM, Rating: {0} - {1} - {2}", song.Artist, song.Title, newRating.ToString());
                Track track = null;

                //Get corrected track if there is one
                //Without getting the corrected track, 
                //ratings will not work if there were corrections.
                if (song.Meta == null)
                {
                    track = QuerySongToTrack(song);
                }
                else
                    track = (Track)song.Meta;

                switch (newRating)
                {
                    case SongRating.love:
                        _scrobbler.UnBan(track);
                        _scrobbler.Love(track);
                        break;
                    case SongRating.ban:
                        _scrobbler.UnLove(track);
                        _scrobbler.Ban(track);
                        break;
                    case SongRating.none:
                        if(oldRating == SongRating.love)
                            _scrobbler.UnLove(track);
                        else if(oldRating == SongRating.ban)
                            _scrobbler.UnBan(track);
                        break;
                }

                if (_scrobbler.QueuedCount > 0) ProcessScrobbles();
            }
            catch (Exception ex)
            {
                Log.O("Last.FM Error!: " + ex.ToString());
            }
        }

        #endregion
    }
}
