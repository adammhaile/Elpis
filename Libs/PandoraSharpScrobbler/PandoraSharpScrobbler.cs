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

namespace PandoraSharp.Plugins
{
    public class PandoraSharpScrobbler : IPlayerControlQuery
    /* This class maintains a Last.fm ScrobbleManager object, monitors Elpis's
     * play progress and adds tracks to the scrobbling queue as they satisfy 
     * the 'played' definition - currently hardcoded to half the tracklength */
    {
        private const double PercentNowPlaying = 5.0;
        private const double PercentBeforeScrobble = 75.0;

        private bool _doneScrobble;
        private bool _doneNowPlaying;
        QuerySong _currentSong;

        QueuingScrobbler _scrobbler;

        ProcessScrobblesDelegate _processScrobbleDelegate;

        public bool IsEnabled { get; set; }

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
            _scrobbler = new QueuingScrobbler(APIKey, APISecret, SessionKey);
        }

        private void DoScrobbles()
        {
            try
            {
                _scrobbler.Process();
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
                throw;
            }
            return SessionKey;
        }

        private static Track QueryProgressToTrack(QueryProgress prog)
        {
            var track = new Track
            {
                TrackName = prog.Song.Title,
                AlbumName = prog.Song.Album,
                ArtistName = prog.Song.Artist,
                Duration = prog.Progress.TotalTime,
            };
            return track;
        }

        #region IPlayerControlQuery Members
        
        public void SongUpdateReceiver(QuerySong song)
        {
            //Nothing to do here
        }

        public void StatsusUpdateReceiver(QueryStatus status)
        {
            //Nothing to do here
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
                if (progress.Progress.Percent > PercentBeforeScrobble && !_doneScrobble)
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
        #endregion
    }
}
