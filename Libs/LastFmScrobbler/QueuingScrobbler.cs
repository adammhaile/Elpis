using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lpfm.LastFmScrobbler
{
    /// <summary>
    /// A Scrobbler object that scrobbles to a queue until the application is ready to process
    /// </summary>
    /// <remarks>Use this version of the Scrobbler as a helper for asynchronous scrobbling</remarks>
    /// 
    public class QueuingScrobbler
    {
        private ConcurrentQueue<Track> ScrobbleQueue { get; set; }
        public int ScrobbleQueueCount { get { if (ScrobbleQueue != null) return ScrobbleQueue.Count; else return 0; } }
        private ConcurrentQueue<Track> NowPlayingQueue { get; set; }
        public int NowPlayingQueueCount { get { if (NowPlayingQueue != null) return NowPlayingQueue.Count; else return 0; } }

        public int QueuedCount { get { return ScrobbleQueueCount + NowPlayingQueueCount; } }

        private Scrobbler _scrobbler;
        public Scrobbler BaseScrobbler { get { return _scrobbler;  } }

        private string ApiKey { get; set; }
        private string ApiSecret { get; set; }
        private string SessionKey { get; set; }

        /// <summary>
        /// Instantiates an instance of a <see cref="QueuingScrobbler"/>
        /// </summary>
        /// <param name="apiKey">Required. An API Key from Last.fm. See http://www.last.fm/api/account </param>
        /// <param name="apiSecret">Required. An API Secret from Last.fm. See http://www.last.fm/api/account </param>
        /// <param name="sessionKey">Required. An authorized Last.fm Session Key. See <see cref="Scrobbler.GetSession"/></param>
        /// <exception cref="ArgumentNullException"/>
        public QueuingScrobbler(string apiKey, string apiSecret, string sessionKey)
        {
            if (string.IsNullOrEmpty(apiKey)) throw new ArgumentNullException("apiKey");
            if (string.IsNullOrEmpty(apiSecret)) throw new ArgumentNullException("apiSecret");
            //if (string.IsNullOrEmpty(sessionKey)) throw new ArgumentNullException("sessionKey");

            ApiKey = apiKey;
            ApiSecret = apiSecret;
            SessionKey = sessionKey;
            NowPlayingQueue = new ConcurrentQueue<Track>();
            ScrobbleQueue = new ConcurrentQueue<Track>();

            _scrobbler = new Scrobbler(ApiKey, ApiSecret, SessionKey);
        }

        /// <summary>
        /// Enqueues a NowPlaying request but does not send it. Call <see cref="Process"/> to send
        /// </summary>
        /// <param name="track">The <see cref="Track"/> that is now playing</param>
        /// <remarks>This method is thread-safe. Will not check for invalid tracks until Processed. You should validate the Track before calling NowPlaying</remarks>
        public void NowPlaying(Track track)
        {
            NowPlayingQueue.Enqueue(track);
        }

        /// <summary>
        /// Enqueues a Srobble request but does not send it. Call <see cref="Process"/> to send
        /// </summary>
        /// <param name="track">The <see cref="Track"/> that has played</param>
        /// <remarks>This method is thread-safe. Will not check for invalid tracks until Processed. You should validate the Track before calling Scrobble</remarks>
        public void Scrobble(Track track)
        {
            ScrobbleQueue.Enqueue(track);
        }

        /// <summary>
        /// Synchronously processes all scrobbles and now playing notifications that are in the Queues, and returns the results
        /// </summary>
        /// <param name="throwExceptionDuringProcess">When true, will throw the first Exception encountered during Scrobbling (and cease to process). 
        /// When false, any exceptions raised will be attached to the corresponding <see cref="ScrobbleResponse"/>, but will not be thrown. Default is false.</param>
        /// <returns><see cref="ScrobbleResponses"/>, a list of <see cref="ScrobbleResponse"/> </returns>
        /// <remarks>This method will complete synchronously and may take some time. This should be invoked by a single timer. This 
        /// method may not be thread safe</remarks>
        public List<Response> Process(bool throwExceptionDuringProcess = false)
        {
            if (string.IsNullOrEmpty(SessionKey)) return null;
            var results = new List<Response>();
            
            Track track;
            while(NowPlayingQueue.TryDequeue(out track))
            {
                try
                {
                    results.Add(_scrobbler.NowPlaying(track));
                }
                catch (Exception exception)
                {
                    if (throwExceptionDuringProcess) throw;
                    results.Add(new NowPlayingResponse {Track = track, Exception = exception});
                }
            }

            while(ScrobbleQueue.TryDequeue(out track))
            {
                //TODO: Implement bulk scrobble
                try
                {
                    results.Add(_scrobbler.Scrobble(track));
                }
                catch (Exception exception)
                {
                    if (throwExceptionDuringProcess) throw;
                    results.Add(new ScrobbleResponse {Track = track, Exception = exception});
                }
            }

            return results;
        }
    }
}
