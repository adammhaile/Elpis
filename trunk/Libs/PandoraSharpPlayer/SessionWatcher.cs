using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PandoraSharp.ControlQuery;
using PandoraSharp;

namespace PandoraSharpPlayer
{
    public class SessionWatcher : IPlayerControlQuery
    {
        public event PlayStateRequestEvent PlayStateRequest;
        public event PlayRequestEvent PlayRequest;
        public event PauseRequestEvent PauseRequest;
        public event NextRequestEvent NextRequest;
        public event StopRequestEvent StopRequest;

        private Util.SystemSessionState _sessionState;

        public bool IsEnabled { get; set; }

        private QueryStatusValue _oldState = QueryStatusValue.Invalid;

        public SessionWatcher()
        {
            _sessionState = new Util.SystemSessionState();
            _sessionState.SystemLocked += _sessionState_SystemLocked;
            _sessionState.SystemUnlocked += _sessionState_SystemUnlocked;
        }

        void _sessionState_SystemUnlocked()
        {
            if(!IsEnabled) return;

            if (_oldState == QueryStatusValue.Playing)
                PlayRequest(this);
        }

        void _sessionState_SystemLocked()
        {
            if (!IsEnabled) return;

            _oldState = PlayStateRequest(this);
            if (_oldState == QueryStatusValue.Playing)
                PauseRequest(this);
        }

        public void SongUpdateReceiver(QuerySong song)
        {

        }

        public void ProgressUpdateReciever(QueryProgress progress)
        {

        }

        public void StatusUpdateReceiver(QueryStatus status)
        {

        }

        public void RatingUpdateReceiver(QuerySong song, SongRating oldRating, SongRating newRating)
        {

        }
    }
}

