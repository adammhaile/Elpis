using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PandoraSharp.ControlQuery;
using PandoraSharp;

namespace Template
{
    //Use this as a template for classes that inherit IPlayerControlQuery
    public class Template : IPlayerControlQuery
    {
        public event PlayStateRequestEvent PlayStateRequest;
        public event PlayRequestEvent PlayRequest;
        public event PauseRequestEvent PauseRequest;
        public event NextRequestEvent NextRequest;
        public event StopRequestEvent StopRequest;

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
