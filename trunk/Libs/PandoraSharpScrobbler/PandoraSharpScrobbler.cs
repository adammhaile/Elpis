using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lastfm;             /*Note this requires lastfm-sharp.dll. Lastfm-sharp is   */
using Lastfm.Scrobbling;  /* an open source C# last.fm client library available at */
using Lastfm.Services;    /* https://code.google.com/p/lastfm-sharp */
using PandoraSharp;
using PandoraSharp.ControlQuery;
using Util;

namespace PandoraSharp.Plugins
{
    public class PandoraSharpScrobbler : PlayerControlQuery
    /* This class maintains a Last.fm ScrobbleManager object, monitors Elpis's
     * play progress and adds tracks to the scrobbling queue as they satisfy 
     * the 'played' definition - currently hardcoded to half the tracklength */
    {
        private const double PercentBeforeScrobble = 50.0;

        private Session _session = null;
        private string _username;
        private string _password;
        private Connection _connection = null;

        private ScrobbleManager _scrobbleQueue;
        private bool _scrobbledYet;
        //QuerySong _currentSong;

        public PandoraSharpScrobbler(string lastFMApiKey, string lastFMApiSecret)
        {
            _session = new Session(lastFMApiKey, lastFMApiSecret);   
        }

        public bool Connect(string username, string password)
        {
            _username = username;
            _password = password;

            /* Last.fm expects the MD5 hash of password. Note we stored an encrypted password using same 
             * method as Pandora password, but decrypted when we retrieved */
            Log.O("LastFM: Connecting");
            _session.Authenticate(username, Utilities.MD5(password));
            if (_session.Authenticated)
            {
                _connection = new Connection("Elpis", "1.0", username, _session);
                _scrobbleQueue = new ScrobbleManager(_connection);
                Log.O("LastFM: Connected");
            }
            else
            {
                _connection = null;
                _scrobbleQueue = null;
                Log.O("LastFM: Connection Failed");
            }

            _scrobbledYet = false;

            return _session.Authenticated;
        }

        public override void SongUpdateReceiver(QuerySong song)
        {
            if (!_session.Authenticated) return;
            _scrobbledYet = false;
            //_currentSong = song;
            _scrobbleQueue.ReportNowplaying(new NowplayingTrack(song.Artist, song.Title));
        }

        public override void StatsusUpdateReceiver(QueryStatus status)
        {
            
        }

        public override void ProgressUpdateReciever(QueryProgress progress)
        {
            if (!_session.Authenticated) return;
            if (progress.Progress.Percent > PercentBeforeScrobble && !_scrobbledYet)
            {
                Log.O("LastFM, Scrobbling: {0} - {1}", progress.Song.Artist, progress.Song.Title);
                _scrobbleQueue.Queue(
                    new Entry(progress.Song.Artist, progress.Song.Title, System.DateTime.Now - progress.Progress.ElapsedTime,
                        PlaybackSource.PersonalizedBroadcast, progress.Progress.TotalTime, ScrobbleMode.Played));
                _scrobbledYet = true;
            }
        }
    }
}
