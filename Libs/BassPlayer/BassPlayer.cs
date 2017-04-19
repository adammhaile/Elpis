/*
 * Copyright 2012 - Adam Haile / Media Portal
 * http://adamhaile.net
 *
 * This file is part of BassPlayer.
 * BassPlayer is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * BassPlayer is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with BassPlayer. If not, see http://www.gnu.org/licenses/.
 * 
 * Note: Below is a heavily modified version of BassAudio.cs from
 * http://sources.team-mediaportal.com/websvn/filedetails.php?repname=MediaPortal&path=%2Ftrunk%2Fmediaportal%2FCore%2FMusicPlayer%2FBASS%2FBassAudio.cs
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Midi;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.AddOn.WaDsp;
using Un4seen.Bass.Misc;
using System.Text;
using System.Linq;

namespace BassPlayer
{
    /// <summary>
    /// This singleton class is responsible for managing the BASS audio Engine object. 
    /// </summary>
    //public class BassMusicPlayer
    //{
    //    #region Variables

    //    internal static BassAudioEngine _Player;
    //    private static Thread BassAsyncLoadThread;

    //    private static string _email = string.Empty;
    //    private static string _key = string.Empty;

    //    #endregion

    //    #region Constructors/Destructors

    //    // Singleton -- make sure we can't instantiate this class
    //    private BassMusicPlayer()
    //    {
    //    }

    //    #endregion

    //    #region Properties

    //    /// <summary>
    //    /// Returns the BassAudioEngine Object
    //    /// </summary>
    //    public static BassAudioEngine Player
    //    {
    //        get
    //        {
    //            if (_Player == null)
    //            {
    //                _Player = new BassAudioEngine(_email, _key);
    //            }

    //            return _Player;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns a Boolean if the BASS Audio Engine is initialised
    //    /// </summary>
    //    public static bool Initialized
    //    {
    //        get { return _Player != null && _Player.Initialized; }
    //    }

    //    /// <summary>
    //    /// Is the BASS Engine Freed?
    //    /// </summary>
    //    public static bool BassFreed
    //    {
    //        get { return _Player.BassFreed; }
    //    }

    //    #endregion

    //    #region Public Methods

    //    /// <summary>
    //    /// Create the BASS Audio Engine Objects
    //    /// </summary>
    //    //public static void CreatePlayerAsync()
    //    //{
    //    //    if (_Player != null)
    //    //    {
    //    //        return;
    //    //    }
    //    //    ThreadStart ts = InternalCreatePlayerAsync;
    //    //    BassAsyncLoadThread = new Thread(ts);
    //    //    BassAsyncLoadThread.Name = "BassAudio";
    //    //    BassAsyncLoadThread.Start();
    //    //}
    //    public static void SetRegistration(string email, string key)
    //    {
    //        _email = email;
    //        _key = key;
    //    }

    //    /// <summary>
    //    /// Frees, the BASS Audio Engine.
    //    /// </summary>
    //    public static void FreeBass()
    //    {
    //        if (_Player == null)
    //        {
    //            return;
    //        }

    //        _Player.FreeBass();
    //    }

    //    #endregion

    //    #region Private Methods

    //    /// <summary>
    //    /// Thread for Creating the BASS Audio Engine objects.
    //    /// </summary>
    //    private static void InternalCreatePlayerAsync()
    //    {
    //        if (_Player == null)
    //        {
    //            _Player = new BassAudioEngine();
    //        }
    //    }

    //    #endregion
    //}


    public class BassException : Exception
    {
        public BassException()
        {
        }

        public BassException(string msg) : base(msg)
        {
        }
    }

    public class BassStreamException : Exception
    {
        public BassStreamException()
        {
        }

        public BassStreamException(string msg) : base(msg)
        {
        }

        public BassStreamException(string msg, BASSError error) : base(msg)
        {
            ErrorCode = error;
        }

        public BASSError ErrorCode { get; set; }
    }

    /// <summary>
    /// Handles playback of Audio files and Internet streams via the BASS Audio Engine.
    /// </summary>
    public class BassAudioEngine : IDisposable // : IPlayer
    {
        #region Enums

        /// <summary>
        /// The various States for Playback
        /// </summary>
        public enum PlayState
        {
            Init,
            Playing,
            Paused,
            Ended,
            Stopped
        }

        #region Nested type: PlayBackType

        /// <summary>
        /// States, how the Playback is handled
        /// </summary>
        private enum PlayBackType
        {
            NORMAL = 0,
            GAPLESS = 1,
            CROSSFADE = 2
        }

        #endregion

        #region Nested type: Progress

        public class Progress
        {
            public TimeSpan TotalTime { get; set; }
            public TimeSpan ElapsedTime { get; set; }

            public TimeSpan RemainingTime
            {
                get { return TotalTime - ElapsedTime; }
            }

            public double Percent
            {
                get
                {
                    if (TotalTime.Ticks == 0)
                        return 0.0;

                    return ((ElapsedTime.TotalSeconds/TotalTime.TotalSeconds)*100);
                }
            }
        }

        #endregion

        #endregion

        #region Delegates

        public delegate void CrossFadeHandler(object sender, string filePath);

        public delegate void DownloadCanceledHandler(object sender, string downloadFile);

        public delegate void DownloadCompleteHandler(object sender, string downloadFile);

        public delegate void InternetStreamSongChangedHandler(object sender);

        public delegate void PlaybackProgressHandler(object sender, Progress prog);

        public delegate void PlaybackStartHandler(object sender, double duration);

        public delegate void PlaybackStateChangedHandler(object sender, PlayState oldState, PlayState newState);

        public delegate void PlaybackStopHandler(object sender);

        public delegate void TrackPlaybackCompletedHandler(object sender, string filePath);

        #endregion

        private DOWNLOADPROC DownloadProcDelegate;
        private SYNCPROC MetaTagSyncProcDelegate;
        private SYNCPROC PlaybackEndProcDelegate;
        private SYNCPROC PlaybackFadeOutProcDelegate;
        private SYNCPROC PlaybackStreamFreedProcDelegate;

        #region Variables

        private const int MAXSTREAMS = 1;
        private readonly List<int> DecoderPluginHandles = new List<int>();
        private readonly List<List<int>> StreamEventSyncHandles = new List<List<int>>();
        private readonly List<int> Streams = new List<int>(MAXSTREAMS);
        private readonly BASSTimer UpdateTimer = new BASSTimer();

        private readonly float[,] _MixingMatrix = new float[8,2]
                                                      {
                                                          {1, 0}, // left front out = left in
                                                          {0, 1}, // right front out = right in
                                                          {1, 0}, // centre out = left in
                                                          {0, 1}, // LFE out = right in
                                                          {1, 0}, // left rear/side out = left in
                                                          {0, 1}, // right rear/side out = right in
                                                          {1, 0}, // left-rear center out = left in
                                                          {0, 1} // right-rear center out = right in
                                                      };

        private readonly string _regEmail = string.Empty;
        private readonly string _regKey = string.Empty;

        private readonly List<int> soundFontHandles = new List<int>();

        private int CurrentStreamIndex;

        private string FilePath = string.Empty;
        private bool NeedUpdate = true;
        private bool NotifyPlaying = true;
        private bool _BassFreed;
        private int _BufferingMS = 5000;

        private int _CrossFadeIntervalMS = 4000;
        private bool _CrossFading; // true if crossfading has started
        private int _DefaultCrossFadeIntervalMS = 4000;
        private bool _Initialized;

        private bool _Mixing;
        private bool _SoftStop = true;
        private string _SoundDevice = "";

        private PlayState _State = PlayState.Init;
        private int _StreamVolume = 100;

        private string _downloadFile = string.Empty;
        private bool _downloadFileComplete;
        private FileStream _downloadStream;
        private DSP_Gain _gain;

        private bool _isRadio;
        private int _mixer;
        private int _playBackType;
        private int _progUpdateInterval = 500; //update every 500 ms
        private int _speed = 1;
        private TAG_INFO _tagInfo;

        // Midi File support
        private BASS_MIDI_FONT[] soundFonts;

        //Registration

        #endregion

        #region Properties

        public string SoundDevice
        {
            get { return _SoundDevice; }
            set { ChangeOutputDevice(value); }
        }

        /// <summary>
        /// Returns, if the player is in initialising stage
        /// </summary>
        public bool Initializing
        {
            get { return (_State == PlayState.Init); }
        }

        /// <summary>
        /// Returns the Duration of an Audio Stream
        /// </summary>
        public double Duration
        {
            get
            {
                int stream = GetCurrentStream();

                if (stream == 0)
                {
                    return 0;
                }

                double duration = GetTotalStreamSeconds(stream);

                return duration;
            }
        }

        /// <summary>
        /// Returns the Current Position in the Stream
        /// </summary>
        public double CurrentPosition
        {
            get
            {
                int stream = GetCurrentStream();

                if (stream == 0)
                {
                    return 0;
                }

                long pos = Bass.BASS_ChannelGetPosition(stream); // position in bytes

                double curPosition = Bass.BASS_ChannelBytes2Seconds(stream, pos); // the elapsed time length

                return curPosition;
            }
        }

        public int ProgressUpdateInterval
        {
            get { return _progUpdateInterval; }
            set
            {
                _progUpdateInterval = value;
                UpdateTimer.Interval = _progUpdateInterval;
            }
        }

        /// <summary>
        /// Returns the Current Play State
        /// </summary>
        public PlayState State
        {
            get { return _State; }
        }

        /// <summary>
        /// Has the Playback Ended?
        /// </summary>
        public bool Ended
        {
            get { return _State == PlayState.Ended; }
        }

        /// <summary>
        /// Is Playback Paused?
        /// </summary>
        public bool Paused
        {
            get { return (_State == PlayState.Paused); }
        }

        /// <summary>
        /// Is the Player Playing?
        /// </summary>
        public bool Playing
        {
            get { return (_State == PlayState.Playing || _State == PlayState.Paused); }
        }

        /// <summary>
        /// Is Player Stopped?
        /// </summary>
        public bool Stopped
        {
            get { return (_State == PlayState.Init || _State == PlayState.Stopped); }
        }

        /// <summary>
        /// Returns the File, currently played
        /// </summary>
        public string CurrentFile
        {
            get { return FilePath; }
        }

        /// <summary>
        /// Gets/Sets the Playback Volume
        /// </summary>
        public int Volume
        {
            get { return _StreamVolume; }
            set
            {
                if (_StreamVolume != value)
                {
                    if (value > 100)
                    {
                        value = 100;
                    }

                    if (value < 0)
                    {
                        value = 0;
                    }

                    _StreamVolume = value;
                    _StreamVolume = value;
                    Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, _StreamVolume*100);
                }
            }
        }

        /// <summary>
        /// Returns the Playback Speed
        /// </summary>
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Gets/Sets the Crossfading Interval
        /// </summary>
        public int CrossFadeIntervalMS
        {
            get { return _CrossFadeIntervalMS; }
            set { _CrossFadeIntervalMS = value; }
        }

        /// <summary>
        /// Gets/Sets the Buffering of BASS Streams
        /// </summary>
        public int BufferingMS
        {
            get { return _BufferingMS; }
            set
            {
                if (_BufferingMS == value)
                {
                    return;
                }

                _BufferingMS = value;
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, _BufferingMS);
            }
        }

        /// <summary>
        /// Returns the instance of the Visualisation Manager
        /// </summary>
        /// 
        //R//
        //public IVisualizationManager IVizManager
        //{
        //  get { return VizManager; }
        //}
        public bool IsRadio
        {
            get { return _isRadio; }
        }

        /// <summary>
        /// Returns the Playback Type
        /// </summary>
        public int PlaybackType
        {
            get { return _playBackType; }
        }

        /// <summary>
        /// Returns the instance of the Video Window
        /// </summary>
        //R//public VisualizationWindow VisualizationWindow
        //{
        //  get { return VizWindow; }
        //}
        /// <summary>
        /// Is the Audio Engine initialised
        /// </summary>
        public bool Initialized
        {
            get { return _Initialized; }
        }

        /// <summary>
        /// Is Crossfading enabled
        /// </summary>
        public bool CrossFading
        {
            get { return _CrossFading; }
        }

        /// <summary>
        /// Is Crossfading enabled
        /// </summary>
        public bool CrossFadingEnabled
        {
            get { return _CrossFadeIntervalMS > 0; }
        }

        /// <summary>
        /// Is BASS freed?
        /// </summary>
        public bool BassFreed
        {
            get { return _BassFreed; }
        }

        /// <summary>
        /// Returns the Stream, currently played
        /// </summary>
        public int CurrentAudioStream
        {
            get { return GetCurrentVizStream(); }
        }

        #endregion

        #region Constructors/Destructors

        public BassAudioEngine(string email = "", string key = "")
        {
            _regEmail = email;
            _regKey = key;

            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Release the Player
        /// </summary>
        public void Dispose()
        {
            if (!Stopped) // Check if stopped already to avoid that Stop() is called two or three times
            {
                Stop(true);
            }
            _proxyValue.Free();
        }

        /// <summary>
        /// Dispose the BASS Audio engine. Free all BASS and Visualisation related resources
        /// </summary>
        public void DisposeAndCleanUp()
        {
            Dispose();
            // Clean up BASS Resources
            try
            {
                // Some Winamp dsps might raise an exception when closing
                BassWaDsp.BASS_WADSP_Free();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw new BassException(ex.ToString());
            }
            if (_mixer != 0)
            {
                Bass.BASS_ChannelStop(_mixer);
            }

            Bass.BASS_Stop();
            Bass.BASS_Free();

            foreach (int stream in Streams)
            {
                FreeStream(stream);
            }

            foreach (int pluginHandle in DecoderPluginHandles)
            {
                Bass.BASS_PluginFree(pluginHandle);
            }
        }

        /// <summary>
        /// The BASS engine itself is not initialised at this stage, since it may cause S/PDIF for Movies not working on some systems.
        /// </summary>
        private void Initialize()
        {
            try
            {
                Log.Info("BASS: Initialize BASS environment ...");
                LoadSettings();

                //TODO: Make this configurable
                if (_regEmail != string.Empty)
                    BassNet.Registration(_regEmail, _regKey);

                // Set the Global Volume. 0 = silent, 10000 = Full
                // We get 0 - 100 from Configuration, so multiply by 100
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, _StreamVolume*100);

                if (_Mixing)
                {
                    // In case of mixing use a Buffer of 500ms only, because the Mixer plays the complete bufer, before for example skipping
                    BufferingMS = 500;
                }
                else
                {
                    Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, _BufferingMS);
                }

                for (int i = 0; i < MAXSTREAMS; i++)
                {
                    Streams.Add(0);
                }

                PlaybackFadeOutProcDelegate = PlaybackFadeOutProc;
                PlaybackEndProcDelegate = PlaybackEndProc;
                PlaybackStreamFreedProcDelegate = PlaybackStreamFreedProc;
                MetaTagSyncProcDelegate = MetaTagSyncProc;

                DownloadProcDelegate = DownloadProc;


                StreamEventSyncHandles.Add(new List<int>());
                StreamEventSyncHandles.Add(new List<int>());

                LoadAudioDecoderPlugins();

                Log.Info("BASS: Initializing BASS environment done.");

                _Initialized = true;
                _BassFreed = true;
            }

            catch (Exception ex)
            {
                Log.Error("BASS: Initialize thread failed.  Reason: {0}", ex.Message);
                throw new BassException("BASS: Initialize thread failed.  Reason: " + ex);
            }
        }

        /// <summary>
        /// Free BASS, when not playing Audio content, as it might cause S/PDIF output stop working
        /// </summary>
        public void FreeBass()
        {
            if (!_BassFreed)
            {
                Log.Info("BASS: Freeing BASS. Non-audio media playback requested.");

                if (_mixer != 0)
                {
                    Bass.BASS_ChannelStop(_mixer);
                    _mixer = 0;
                }

                Bass.BASS_Free();
                _BassFreed = true;
            }
        }

        /// <summary>
        /// Init BASS, when a Audio file is to be played
        /// </summary>
        public void InitBass()
        {
            try
            {
                Log.Info("BASS: Initializing BASS audio engine...");
                bool initOK = false;

                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_DEV_DEFAULT, true); //Allows following Default device (Win 7 Only)
                int soundDevice = GetSoundDevice();

                initOK =
                    (Bass.BASS_Init(soundDevice, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_LATENCY,
                                    IntPtr.Zero));
                if (initOK)
                {
                    // Create an 8 Channel Mixer, which should be running until stopped.
                    // The streams to play are added to the active screen
                    if (_Mixing && _mixer == 0)
                    {
                        _mixer = BassMix.BASS_Mixer_StreamCreate(44100, 8,
                                                                 BASSFlag.BASS_MIXER_NONSTOP |
                                                                 BASSFlag.BASS_STREAM_AUTOFREE);
                    }

                    UpdateTimer.Interval = _progUpdateInterval;
                    UpdateTimer.Tick += OnUpdateTimerTick;

                    Log.Info("BASS: Initialization done.");
                    _Initialized = true;
                    _BassFreed = false;
                }
                else
                {
                    BASSError error = Bass.BASS_ErrorGetCode();
                    Log.Error("BASS: Error initializing BASS audio engine {0}",
                              Enum.GetName(typeof (BASSError), error));
                    throw new Exception("Init Error: " + error.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error("BASS: Initialize failed. Reason: {0}", ex.Message);
                throw new BassException("BASS: Initialize failed. Reason: }" + ex.Message);
            }
        }

        private GCHandle _proxyValue;
        public void SetProxy(string address, int port, string user = "", string password = "")
        {
            string proxy = address + ":" + port.ToString();
            if(user != "")
                proxy = user + ":" + password + "@" + proxy;

            byte[] proxyBytes = Encoding.Default.GetBytes(proxy);
            _proxyValue = GCHandle.Alloc(proxyBytes, GCHandleType.Pinned);
            Bass.BASS_SetConfigPtr(BASSConfig.BASS_CONFIG_NET_PROXY,
                _proxyValue.AddrOfPinnedObject());

            //_proxyValue.Free();
        }

        /// <summary>
        /// Get the Sound devive as set in the Configuartion
        /// </summary>
        /// <returns></returns>
        private int GetSoundDevice()
        {
            int sounddevice = -1;
            // Check if the specified Sounddevice still exists
            if (_SoundDevice == "Default")
            {
                Log.Info("BASS: Using default Sound Device");
                sounddevice = -1;
            }
            else
            {
                BASS_DEVICEINFO[] soundDeviceDescriptions = Bass.BASS_GetDeviceInfos();
                bool foundDevice = false;
                for (int i = 0; i < soundDeviceDescriptions.Length; i++)
                {
                    if (soundDeviceDescriptions[i].name == _SoundDevice)
                    {
                        foundDevice = true;
                        sounddevice = i;
                        break;
                    }
                }
                if (!foundDevice)
                {
                    Log.Warn("BASS: specified Sound device does not exist. Using default Sound Device");
                    sounddevice = -1;
                }
                else
                {
                    Log.Info("BASS: Using Sound Device {0}", _SoundDevice);
                }
            }
            return sounddevice;
        }

        /// <summary>
        /// Load Settings 
        /// </summary>
        private void LoadSettings()
        {
            //TODO - Load Settings

            //using (Profile.Settings xmlreader = new Profile.MPSettings())
            //{
            _SoundDevice = "Default";
            //xmlreader.GetValueAsString("audioplayer", "sounddevice", "Default Sound Device");

            _StreamVolume = 100; // xmlreader.GetValueAsInt("audioplayer", "streamOutputLevel", 85);
            _BufferingMS = 5000; // xmlreader.GetValueAsInt("audioplayer", "buffering", 5000);

            if (_BufferingMS <= 0)
            {
                _BufferingMS = 1000;
            }

            else if (_BufferingMS > 8000)
            {
                _BufferingMS = 8000;
            }

            _CrossFadeIntervalMS = 0; //xmlreader.GetValueAsInt("audioplayer", "crossfade", 4000);

            if (_CrossFadeIntervalMS < 0)
            {
                _CrossFadeIntervalMS = 0;
            }

            else if (_CrossFadeIntervalMS > 16000)
            {
                _CrossFadeIntervalMS = 16000;
            }

            _DefaultCrossFadeIntervalMS = _CrossFadeIntervalMS;

            _SoftStop = true; //xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);

            _Mixing = false; //xmlreader.GetValueAsBool("audioplayer", "mixing", false);

            bool doGaplessPlayback = false; //xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);

            if (doGaplessPlayback)
            {
                _CrossFadeIntervalMS = 200;
                _playBackType = (int) PlayBackType.GAPLESS;
            }
            else
            {
                if (_CrossFadeIntervalMS == 0)
                {
                    _playBackType = (int) PlayBackType.NORMAL;
                    _CrossFadeIntervalMS = 100;
                }
                else
                {
                    _playBackType = (int) PlayBackType.CROSSFADE;
                }
            }
            //}
        }

        /// <summary>
        /// Return the BASS Stream to be used for Visualisation purposes.
        /// We will extract the WAVE and FFT data to be provided to the Visualisation Plugins
        /// In case of Mixer active, we need to return the Mixer Stream. 
        /// In all other cases the current actove stream is used.
        /// </summary>
        /// <returns></returns>
        internal int GetCurrentVizStream()
        {
            if (Streams.Count == 0)
            {
                return -1;
            }

            if (_Mixing)
            {
                return _mixer;
            }
            else
            {
                return GetCurrentStream();
            }
        }

        /// <summary>
        /// Returns the Current Stream 
        /// </summary>
        /// <returns></returns>
        internal int GetCurrentStream()
        {
            if (Streams.Count == 0)
            {
                return -1;
            }

            if (CurrentStreamIndex < 0)
            {
                CurrentStreamIndex = 0;
            }

            else if (CurrentStreamIndex >= Streams.Count)
            {
                CurrentStreamIndex = Streams.Count - 1;
            }

            return Streams[CurrentStreamIndex];
        }

        /// <summary>
        /// Returns the Next Stream
        /// </summary>
        /// <returns></returns>
        private int GetNextStream()
        {
            int currentStream = GetCurrentStream();

            if (currentStream == -1)
            {
                return -1;
            }

            if (currentStream == 0 || Bass.BASS_ChannelIsActive(currentStream) == BASSActive.BASS_ACTIVE_STOPPED)
            {
                return currentStream;
            }

            CurrentStreamIndex++;

            if (CurrentStreamIndex >= Streams.Count)
            {
                CurrentStreamIndex = 0;
            }

            return Streams[CurrentStreamIndex];
        }

        private void UpdateProgress(int stream)
        {
            if (PlaybackProgress != null)
            {
                var totaltime = new TimeSpan(0, 0, (int) GetTotalStreamSeconds(stream));
                var elapsedtime = new TimeSpan(0, 0, (int) GetStreamElapsedTime(stream));
                PlaybackProgress(this, new Progress {TotalTime = totaltime, ElapsedTime = elapsedtime});
            }
        }

        private void GetProgressInternal()
        {
            int stream = GetCurrentStream();

            if (StreamIsPlaying(stream))
            {
                UpdateProgress(stream);
            }
            else
            {
                UpdateTimer.Stop();
            }
        }

        public void GetProgress()
        {
            Task.Factory.StartNew(GetProgressInternal);
        }

        /// <summary>
        /// Timer to update the Playback Process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            GetProgressInternal();
        }


        /// <summary>
        /// Load External BASS Audio Decoder Plugins
        /// </summary>
        private void LoadAudioDecoderPlugins()
        {
            //In this case, only load AAC to save load time
            Log.Info("BASS: Loading AAC Decoder");

            string decoderFolderPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof (BassAudioEngine)).Location);
            if(decoderFolderPath == null)
            {
                Log.Error(@"BASS: Unable to load AAC decoder.");
                throw new BassException(@"BASS: Unable to load AAC decoder.");
            }

            string aacDecoder = Path.Combine(decoderFolderPath, "bass_aac.dll");

            int pluginHandle = 0;
            if ((pluginHandle = Bass.BASS_PluginLoad(aacDecoder)) != 0)
            {
                DecoderPluginHandles.Add(pluginHandle);
                Log.Debug("BASS: Added DecoderPlugin: {0}", aacDecoder);
            }
            else
            {
                Log.Error(@"BASS: Unable to load AAC decoder.");
                throw new BassException(@"BASS: Unable to load AAC decoder.");
            }

            /*
            if (!Directory.Exists(decoderFolderPath))
            {
                Log.Error(@"BASS: Unable to find decoders path.");
                throw new BassException(@"BASS: Unable to find decoders path.");
            }

            var dirInfo = new DirectoryInfo(decoderFolderPath);
            FileInfo[] decoders = dirInfo.GetFiles();

            int pluginHandle = 0;
            int decoderCount = 0;

            foreach (FileInfo file in decoders)
            {
                if (Path.GetExtension(file.Name).ToLower() != ".dll")
                {
                    continue;
                }

                pluginHandle = Bass.BASS_PluginLoad(file.FullName);

                if (pluginHandle != 0)
                {
                    DecoderPluginHandles.Add(pluginHandle);
                    decoderCount++;
                    Log.Debug("BASS: Added DecoderPlugin: {0}", file.FullName);
                }

                else
                {
                    Log.Debug("BASS: Unable to load: {0}", file.FullName);
                }
            }

            if (decoderCount > 0)
            {
                Log.Info("BASS: Loaded {0} Audio Decoders.", decoderCount);
            }

            else
            {
                Log.Error(@"BASS: No audio decoders were loaded. Confirm decoders are present in path.");
                throw new BassException(@"BASS: No audio decoders were loaded. Confirm decoders are present in path.");
            }
             * */
        }

        public void SetGain(double gainDB)
        {
            if (_gain == null)
            {
                _gain = new DSP_Gain();
            }

            if (gainDB > 60.0)
                gainDB = 60.0;

            if (gainDB == 0.0)
            {
                _gain.SetBypass(true);
            }
            else
            {
                _gain.SetBypass(false);
                _gain.Gain_dBV = gainDB;
            }
        }

        private void FinalizeDownloadStream()
        {
            if (_downloadStream != null)
            {
                lock (_downloadStream)
                {
                    if (!_downloadFileComplete)
                    {
                        if (DownloadCanceled != null)
                            DownloadCanceled(this, _downloadFile);
                    }

                    _downloadStream.Flush();
                    _downloadStream.Close();
                    _downloadStream = null;

                    _downloadFile = string.Empty;
                    _downloadFileComplete = false;
                }
            }
        }

        private void SetupDownloadStream(string outputFile)
        {
            FinalizeDownloadStream();
            _downloadFile = outputFile;
            _downloadFileComplete = false;
            _downloadStream = new FileStream(outputFile, FileMode.Create);
        }

        public bool PlayStreamWithDownload(string url, string outputFile, double gainDB)
        {
            SetGain(gainDB);
            return PlayStreamWithDownload(url, outputFile);
        }

        public bool PlayStreamWithDownload(string url, string outputFile)
        {
            FinalizeDownloadStream();
            SetupDownloadStream(outputFile);
            return Play(url);
        }

        public bool Play(string filePath, double gainDB)
        {
            SetGain(gainDB);
            return Play(filePath);
        }

        /// <summary>
        /// Starts Playback of the given file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Play(string filePath)
        {
            if (!_Initialized)
            {
                return false;
            }

            try
            {
                UpdateTimer.Stop();
            }
            catch
            {
                throw new BassStreamException("Bass Error: Update Timer Error");
            }
            int stream = GetCurrentStream();

            bool doFade = false;
            bool result = true;
            Speed = 1; // Set playback Speed to normal speed

            try
            {
                if (Paused || (filePath.ToLower().CompareTo(FilePath.ToLower()) == 0 && stream != 0))
                {
                    bool doReturn = !Paused;
                    // Selected file is equal to current stream
                    if (_State == PlayState.Paused)
                    {
                        // Resume paused stream
                        if (_SoftStop)
                        {
                            Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
                        }
                        else
                        {
                            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 1);
                        }

                        result = Bass.BASS_Start();

                        if (result)
                        {
                            _State = PlayState.Playing;
                            UpdateTimer.Start();
                            if (PlaybackStateChanged != null)
                            {
                                PlaybackStateChanged(this, PlayState.Paused, _State);
                            }
                        }

                        if (doReturn)
                            return result;
                    }
                }

                if (stream != 0 && StreamIsPlaying(stream))
                {
                    int oldStream = stream;
                    double oldStreamDuration = GetTotalStreamSeconds(oldStream);
                    double oldStreamElapsedSeconds = GetStreamElapsedTime(oldStream);
                    double crossFadeSeconds = _CrossFadeIntervalMS;

                    if (crossFadeSeconds > 0)
                        crossFadeSeconds = crossFadeSeconds/1000.0;

                    if ((oldStreamDuration - (oldStreamElapsedSeconds + crossFadeSeconds) > -1))
                    {
                        FadeOutStop(oldStream);
                    }
                    else
                    {
                        Bass.BASS_ChannelStop(oldStream);
                    }

                    doFade = true;
                    stream = GetNextStream();

                    if (stream != 0 || StreamIsPlaying(stream))
                    {
                        FreeStream(stream);
                    }
                }

                if (stream != 0)
                {
                    if (!Stopped) // Check if stopped already to avoid that Stop() is called two or three times
                    {
                        Stop(true);
                    }
                    FreeStream(stream);
                }

                _State = PlayState.Init;

                // Make sure Bass is ready to begin playing again
                Bass.BASS_Start();

                if (filePath != string.Empty)
                {
                    // Turn on parsing of ASX files
                    Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 2);

                    BASSFlag streamFlags;
                    if (_Mixing)
                    {
                        streamFlags = BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT;
                        // Don't use the BASS_STREAM_AUTOFREE flag on a decoding channel. will produce a BASS_ERROR_NOTAVAIL
                    }
                    else
                    {
                        streamFlags = BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_AUTOFREE;
                    }

                    FilePath = filePath;

                    _isRadio = false;

                    if (filePath.ToLower().Contains(@"http://") || filePath.ToLower().Contains(@"https://") ||
                        filePath.ToLower().StartsWith("mms") || filePath.ToLower().StartsWith("rtsp"))
                    {
                        _isRadio = true; // We're playing Internet Radio Stream

                        stream = Bass.BASS_StreamCreateURL(filePath, 0, streamFlags, DownloadProcDelegate, IntPtr.Zero);

                        if (stream != 0)
                        {
                            // Get the Tags and set the Meta Tag SyncProc
                            _tagInfo = new TAG_INFO(filePath);
                            SetStreamTags(stream);

                            if (BassTags.BASS_TAG_GetFromURL(stream, _tagInfo))
                            {
                                GetMetaTags();
                            }

                            Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_META, 0, MetaTagSyncProcDelegate,
                                                     IntPtr.Zero);
                        }
                        Log.Debug("BASSAudio: Webstream found - trying to fetch stream {0}", Convert.ToString(stream));
                    }
                    else if (IsMODFile(filePath))
                    {
                        // Load a Mod file
                        stream = Bass.BASS_MusicLoad(filePath, 0, 0,
                                                     BASSFlag.BASS_SAMPLE_SOFTWARE | BASSFlag.BASS_SAMPLE_FLOAT |
                                                     BASSFlag.BASS_MUSIC_AUTOFREE | BASSFlag.BASS_MUSIC_PRESCAN |
                                                     BASSFlag.BASS_MUSIC_RAMP, 0);
                    }
                    else
                    {
                        // Create a Standard Stream
                        stream = Bass.BASS_StreamCreateFile(filePath, 0, 0, streamFlags);
                    }

                    // Is Mixing, then we create a mixer channel and assign the stream to the mixer
                    if ((_Mixing) && stream != 0)
                    {
                        // Do an upmix of the stereo according to the matrix. 
                        // Now Plugin the stream to the mixer and set the mixing matrix
                        BassMix.BASS_Mixer_StreamAddChannel(_mixer, stream,
                                                            BASSFlag.BASS_MIXER_MATRIX | BASSFlag.BASS_STREAM_AUTOFREE |
                                                            BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_BUFFER);
                        BassMix.BASS_Mixer_ChannelSetMatrix(stream, _MixingMatrix);
                    }

                    Streams[CurrentStreamIndex] = stream;

                    if (stream != 0)
                    {
                        // When we have a MIDI file, we need to assign the sound banks to the stream
                        if (IsMidiFile(filePath) && soundFonts != null)
                        {
                            BassMidi.BASS_MIDI_StreamSetFonts(stream, soundFonts, soundFonts.Length);
                        }

                        StreamEventSyncHandles[CurrentStreamIndex] = RegisterPlaybackEvents(stream, CurrentStreamIndex);

                        if (doFade && _CrossFadeIntervalMS > 0)
                        {
                            _CrossFading = true;

                            // Reduce the stream volume to zero so we can fade it in...
                            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 0);

                            // Fade in from 0 to 1 over the _CrossFadeIntervalMS duration 
                            Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 1,
                                                            _CrossFadeIntervalMS);
                        }
                    }
                    else
                    {
                        BASSError error = Bass.BASS_ErrorGetCode();
                        Log.Error("BASS: Unable to create Stream for {0}.  Reason: {1}.", filePath,
                                  Enum.GetName(typeof(BASSError), error));
                        throw new BassStreamException("Bass Error: Unable to create stream - " +
                                                      Enum.GetName(typeof(BASSError), error), error);
                    }

                    bool playbackStarted = false;
                    if (_Mixing)
                    {
                        if (Bass.BASS_ChannelIsActive(_mixer) == BASSActive.BASS_ACTIVE_PLAYING)
                        {
                            playbackStarted = true;
                        }
                        else
                        {
                            playbackStarted = Bass.BASS_ChannelPlay(_mixer, false);
                        }
                    }
                    else
                    {
                        playbackStarted = Bass.BASS_ChannelPlay(stream, false);
                    }

                    if (stream != 0 && playbackStarted)
                    {
                        Log.Info("BASS: playback started");

                        PlayState oldState = _State;
                        _State = PlayState.Playing;

                        UpdateTimer.Start();

                        if (oldState != _State && PlaybackStateChanged != null)
                        {
                            PlaybackStateChanged(this, oldState, _State);
                        }

                        if (PlaybackStart != null)
                        {
                            PlaybackStart(this, GetTotalStreamSeconds(stream));
                        }
                    }

                    else
                    {
                        BASSError error = Bass.BASS_ErrorGetCode();
                        Log.Error("BASS: Unable to play {0}.  Reason: {1}.", filePath,
                                  Enum.GetName(typeof(BASSError), error));
                        throw new BassStreamException("Bass Error: Unable to play - " +
                                                      Enum.GetName(typeof(BASSError), error), error);

                        // Release all of the sync proc handles
                        if (StreamEventSyncHandles[CurrentStreamIndex] != null)
                        {
                            UnregisterPlaybackEvents(stream, StreamEventSyncHandles[CurrentStreamIndex]);
                        }

                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                Log.Error("BASS: Play caused an exception:  {0}.", ex);

                if (ex.GetType() == typeof (BassStreamException))
                    throw;

                throw new BassException("BASS: Play caused an exception: " + ex);
            }

            return result;
        }


        /// <summary>
        /// Is this a MOD file?
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsMODFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();

            switch (ext)
            {
                case ".mod":
                case ".mo3":
                case ".it":
                case ".xm":
                case ".s3m":
                case ".mtm":
                case ".umx":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is this a MIDI file?
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool IsMidiFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();

            switch (ext)
            {
                case ".midi":
                case ".mid":
                case ".rmi":
                case ".kar":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Register the various Playback Events
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamIndex"></param>
        /// <returns></returns>
        private List<int> RegisterPlaybackEvents(int stream, int streamIndex)
        {
            if (stream == 0)
            {
                return null;
            }

            var syncHandles = new List<int>();

            // Don't register the fade out event for last.fm radio, as it causes problems
            // if (!_isLastFMRadio)
            syncHandles.Add(RegisterPlaybackFadeOutEvent(stream, streamIndex, _CrossFadeIntervalMS));

            syncHandles.Add(RegisterPlaybackEndEvent(stream, streamIndex));
            syncHandles.Add(RegisterStreamFreedEvent(stream));

            return syncHandles;
        }

        /// <summary>
        /// Register the Fade out Event
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamIndex"></param>
        /// <param name="fadeOutMS"></param>
        /// <returns></returns>
        private int RegisterPlaybackFadeOutEvent(int stream, int streamIndex, int fadeOutMS)
        {
            int syncHandle = 0;
            long len = Bass.BASS_ChannelGetLength(stream); // length in bytes
            double totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len); // the total time length
            double fadeOutSeconds = 0;

            if (fadeOutMS > 0)
                fadeOutSeconds = fadeOutMS/1000.0;

            long bytePos = Bass.BASS_ChannelSeconds2Bytes(stream, totaltime - fadeOutSeconds);

            syncHandle = Bass.BASS_ChannelSetSync(stream,
                                                  BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_POS,
                                                  bytePos, PlaybackFadeOutProcDelegate,
                                                  IntPtr.Zero);

            if (syncHandle == 0)
            {
                Log.Debug("BASS: RegisterPlaybackFadeOutEvent of stream {0} failed with error {1}", stream,
                          Enum.GetName(typeof (BASSError), Bass.BASS_ErrorGetCode()));
            }

            return syncHandle;
        }

        /// <summary>
        /// Register the Playback end Event
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamIndex"></param>
        /// <returns></returns>
        private int RegisterPlaybackEndEvent(int stream, int streamIndex)
        {
            int syncHandle = 0;

            syncHandle = Bass.BASS_ChannelSetSync(stream,
                                                  BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_END,
                                                  0, PlaybackEndProcDelegate,
                                                  IntPtr.Zero);

            if (syncHandle == 0)
            {
                Log.Debug("BASS: RegisterPlaybackEndEvent of stream {0} failed with error {1}", stream,
                          Enum.GetName(typeof (BASSError), Bass.BASS_ErrorGetCode()));
            }

            return syncHandle;
        }

        /// <summary>
        /// Register Stream Free Event
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private int RegisterStreamFreedEvent(int stream)
        {
            int syncHandle = 0;

            syncHandle = Bass.BASS_ChannelSetSync(stream, BASSSync.BASS_SYNC_FREE, 0, PlaybackStreamFreedProcDelegate,
                                                  IntPtr.Zero);

            if (syncHandle == 0)
            {
                Log.Debug("BASS: RegisterStreamFreedEvent of stream {0} failed with error {1}", stream,
                          Enum.GetName(typeof (BASSError), Bass.BASS_ErrorGetCode()));
            }

            return syncHandle;
        }


        /// <summary>
        /// Unregister the Playback Events
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="syncHandles"></param>
        /// <returns></returns>
        private bool UnregisterPlaybackEvents(int stream, List<int> syncHandles)
        {
            try
            {
                foreach (int syncHandle in syncHandles)
                {
                    if (syncHandle != 0)
                    {
                        Bass.BASS_ChannelRemoveSync(stream, syncHandle);
                    }
                }
            }

            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Free a Stream
        /// </summary>
        /// <param name="stream"></param>
        private void FreeStream(int stream)
        {
            int streamIndex = -1;

            for (int i = 0; i < Streams.Count; i++)
            {
                if (Streams[i] == stream)
                {
                    streamIndex = i;
                    break;
                }
            }

            if (streamIndex != -1)
            {
                List<int> eventSyncHandles = StreamEventSyncHandles[streamIndex];

                foreach (int syncHandle in eventSyncHandles)
                {
                    Bass.BASS_ChannelRemoveSync(stream, syncHandle);
                }
            }

            Bass.BASS_StreamFree(stream);
            stream = 0;

            _CrossFading = false; // Set crossfading to false, Play() will update it when the next song starts
        }

        /// <summary>
        /// Is stream Playing?
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private bool StreamIsPlaying(int stream)
        {
            return stream != 0 && (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING);
        }

        /// <summary>
        /// Get Total Seconds of the Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private double GetTotalStreamSeconds(int stream)
        {
            if (stream == 0)
            {
                return 0;
            }

            // length in bytes
            long len = Bass.BASS_ChannelGetLength(stream);

            // the total time length
            double totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len);
            return totaltime;
        }

        /// <summary>
        /// Retrieve the elapsed time
        /// </summary>
        /// <returns></returns>
        private double GetStreamElapsedTime()
        {
            return GetStreamElapsedTime(GetCurrentStream());
        }

        /// <summary>
        /// Retrieve the elapsed time
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private double GetStreamElapsedTime(int stream)
        {
            if (stream == 0)
            {
                return 0;
            }

            // position in bytes
            long pos = Bass.BASS_ChannelGetPosition(stream);

            // the elapsed time length
            double elapsedtime = Bass.BASS_ChannelBytes2Seconds(stream, pos);
            return elapsedtime;
        }

        private void DownloadProc(IntPtr buffer, int length, IntPtr user)
        {
            if (_downloadStream == null)
                return;

            Log.Debug("DownloadProc: " + length.ToString());
            try
            {
                if (buffer != IntPtr.Zero)
                {
                    var managedBuffer = new byte[length];
                    Marshal.Copy(buffer, managedBuffer, 0, length);
                    _downloadStream.Write(managedBuffer, 0, length);
                    _downloadStream.Flush();
                }
                else
                {
                    _downloadFileComplete = true;
                    string file = _downloadFile;

                    FinalizeDownloadStream();

                    if (DownloadComplete != null)
                        DownloadComplete(this, file);
                }
            }
            catch (Exception ex)
            {
                Log.Error("BASS: Exception in DownloadProc: {0} {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Fade Out  Procedure
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <param name="userData"></param>
        private void PlaybackFadeOutProc(int handle, int stream, int data, IntPtr userData)
        {
            Log.Debug("BASS: PlaybackFadeOutProc of stream {0}", stream);

            if (CrossFade != null)
            {
                CrossFade(this, FilePath);
            }

            Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, -1, _CrossFadeIntervalMS);
            bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
            if (removed)
            {
                Log.Debug("BassAudio: *** BASS_ChannelRemoveSync in PlaybackFadeOutProc");
            }
        }

        /// <summary>
        /// Playback end Procedure
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <param name="userData"></param>
        private void PlaybackEndProc(int handle, int stream, int data, IntPtr userData)
        {
            Log.Debug("BASS: PlaybackEndProc of stream {0}", stream);

            if (TrackPlaybackCompleted != null)
            {
                TrackPlaybackCompleted(this, FilePath);
            }

            bool removed = Bass.BASS_ChannelRemoveSync(stream, handle);
            if (removed)
            {
                Log.Debug("BassAudio: *** BASS_ChannelRemoveSync in PlaybackEndProc");
            }
        }

        /// <summary>
        /// Stream Freed Proc
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <param name="userData"></param>
        private void PlaybackStreamFreedProc(int handle, int stream, int data, IntPtr userData)
        {
            //Util.Log.O("PlaybackStreamFreedProc");
            Log.Debug("BASS: PlaybackStreamFreedProc of stream {0}", stream);

            HandleSongEnded(false);

            for (int i = 0; i < Streams.Count; i++)
            {
                if (stream == Streams[i])
                {
                    Streams[i] = 0;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the tags from the Internet Stream.
        /// </summary>
        /// <param name="stream"></param>
        private void SetStreamTags(int stream)
        {
            //TODO - Make this output to something useful??
            string[] tags = Bass.BASS_ChannelGetTagsICY(stream);
            if (tags != null)
            {
                foreach (string item in tags)
                {
                    if (item.ToLower().StartsWith("icy-name:"))
                    {
                        //GUIPropertyManager.SetProperty("#Play.Current.Album", item.Substring(9));
                    }

                    if (item.ToLower().StartsWith("icy-genre:"))
                    {
                        //GUIPropertyManager.SetProperty("#Play.Current.Genre", item.Substring(10));
                    }

                    Log.Info("BASS: Connection Information: {0}", item);
                }
            }
            else
            {
                tags = Bass.BASS_ChannelGetTagsHTTP(stream);
                if (tags != null)
                {
                    foreach (string item in tags)
                    {
                        Log.Info("BASS: Connection Information: {0}", item);
                    }
                }
            }
        }

        /// <summary>
        /// This Callback Procedure is called by BASS, once a song changes.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void MetaTagSyncProc(int handle, int channel, int data, IntPtr user)
        {
            // BASS_SYNC_META is triggered on meta changes of SHOUTcast streams
            if (_tagInfo.UpdateFromMETA(Bass.BASS_ChannelGetTags(channel, BASSTag.BASS_TAG_META), false, false))
            {
                GetMetaTags();
            }
        }

        /// <summary>
        /// Set the Properties out of the Tags
        /// </summary>
        private void GetMetaTags()
        {
            // There seems to be an issue with setting correctly the title via taginfo
            // So let's filter it out ourself
            string title = _tagInfo.title;
            int streamUrlIndex = title.IndexOf("';StreamUrl=");
            if (streamUrlIndex > -1)
            {
                title = _tagInfo.title.Substring(0, streamUrlIndex);
            }

            Log.Info("BASS: Internet Stream. New Song: {0} - {1}", _tagInfo.artist, title);

            if (InternetStreamSongChanged != null)
            {
                InternetStreamSongChanged(this);
            }
        }

        private void HandleSongEnded(bool bManualStop, bool songSkipped = false)
        {
            Log.Debug("BASS: HandleSongEnded - manualStop: {0}, CrossFading: {1}", bManualStop, _CrossFading);
            PlayState oldState = _State;

            if (!bManualStop)
            {
                //if (_CrossFading)
                //{
                //    _State = PlayState.Playing;
                //}
                //else
                //{
                FilePath = "";
                _State = PlayState.Ended;

                //}
            }
            else
            {
                _State = songSkipped ? PlayState.Init : PlayState.Stopped;
            }

            Util.Log.O("BASS: Playstate Changed - " + _State);

            if (oldState != _State && PlaybackStateChanged != null)
            {
                PlaybackStateChanged(this, oldState, _State);
            }

            FinalizeDownloadStream();
            _CrossFading = false; // Set crossfading to false, Play() will update it when the next song starts
        }

        /// <summary>
        /// Fade out Song
        /// </summary>
        /// <param name="stream"></param>
        private void FadeOutStop(int stream)
        {
            Log.Debug("BASS: FadeOutStop of stream {0}", stream);

            if (!StreamIsPlaying(stream))
            {
                return;
            }

            //int level = Bass.BASS_ChannelGetLevel(stream);
            Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, -1, _CrossFadeIntervalMS);
        }

        /// <summary>
        /// Pause Playback
        /// </summary>
        public void PlayPause()
        {
            _CrossFading = false;
            int stream = GetCurrentStream();

            Log.Debug("BASS: Pause of stream {0}", stream);
            try
            {
                PlayState oldPlayState = _State;

                if (oldPlayState == PlayState.Ended || oldPlayState == PlayState.Init)
                {
                    return;
                }

                if (oldPlayState == PlayState.Paused)
                {
                    _State = PlayState.Playing;

                    if (_SoftStop)
                    {
                        // Fade-in over 500ms
                        Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
                        Bass.BASS_Start();
                    }

                    else
                    {
                        Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 1);
                        Bass.BASS_Start();
                    }

                    UpdateTimer.Start();
                }

                else
                {
                    _State = PlayState.Paused;
                    UpdateTimer.Stop();

                    if (_SoftStop)
                    {
                        // Fade-out over 500ms
                        Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 0, 500);

                        // Wait until the slide is done
                        while (Bass.BASS_ChannelIsSliding(stream, BASSAttribute.BASS_ATTRIB_VOL))
                            Thread.Sleep(20);

                        Bass.BASS_Pause();
                    }

                    else
                    {
                        Bass.BASS_Pause();
                    }
                }

                if (oldPlayState != _State)
                {
                    if (PlaybackStateChanged != null)
                    {
                        PlaybackStateChanged(this, oldPlayState, _State);
                    }
                }
            }

            catch
            {
            }
        }

        /// <summary>
        /// Stopping Playback
        /// </summary>
        public void Stop(bool songSkipped = false)
        {
            _CrossFading = false;

            int stream = GetCurrentStream();
            Log.Debug("BASS: Stop of stream {0}", stream);
            try
            {
                UpdateTimer.Stop();
                if (_SoftStop)
                {
                    Bass.BASS_ChannelSlideAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, -1, 500);

                    // Wait until the slide is done
                    while (Bass.BASS_ChannelIsSliding(stream, BASSAttribute.BASS_ATTRIB_VOL))
                        Thread.Sleep(20);
                }
                if (_Mixing)
                {
                    Bass.BASS_ChannelStop(stream);
                    BassMix.BASS_Mixer_ChannelRemove(stream);
                }
                else
                {
                    Bass.BASS_ChannelStop(stream);
                }

                stream = 0;

                if (PlaybackStop != null)
                {
                    PlaybackStop(this);
                }

                HandleSongEnded(true, songSkipped);
            }

            catch (Exception ex)
            {
                Log.Error("BASS: Stop command caused an exception - {0}", ex.Message);
                throw new BassException("BASS: Stop command caused an exception - }" + ex.Message);
            }

            NotifyPlaying = false;
        }

        /// <summary>
        /// Is Seeking enabled 
        /// </summary>
        /// <returns></returns>
        public bool CanSeek()
        {
            return true;
        }

        /// <summary>
        /// Seek Forward in the Stream
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public bool SeekForward(int ms)
        {
            if (_speed == 1) // not to exhaust log when ff
                Log.Debug("BASS: SeekForward for {0} ms", Convert.ToString(ms));
            _CrossFading = false;

            if (State != PlayState.Playing)
            {
                return false;
            }

            if (ms <= 0)
            {
                return false;
            }

            bool result = false;

            try
            {
                int stream = GetCurrentStream();
                long len = Bass.BASS_ChannelGetLength(stream); // length in bytes
                double totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len); // the total time length

                long pos = 0; // position in bytes
                if (_Mixing)
                {
                    pos = BassMix.BASS_Mixer_ChannelGetPosition(stream);
                }
                else
                {
                    pos = Bass.BASS_ChannelGetPosition(stream);
                }

                double timePos = Bass.BASS_ChannelBytes2Seconds(stream, pos);
                double offsetSecs = ms/1000.0;

                if (timePos + offsetSecs >= totaltime)
                {
                    return false;
                }

                if (_Mixing)
                {
                    BassMix.BASS_Mixer_ChannelSetPosition(stream,
                                                          Bass.BASS_ChannelSeconds2Bytes(stream, timePos + offsetSecs));
                    // the elapsed time length
                }
                else
                    Bass.BASS_ChannelSetPosition(stream, timePos + offsetSecs); // the elapsed time length
            }

            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Seek Backwards within the stream
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public bool SeekReverse(int ms)
        {
            if (_speed == 1) // not to exhaust log
                Log.Debug("BASS: SeekReverse for {0} ms", Convert.ToString(ms));
            _CrossFading = false;

            if (State != PlayState.Playing)
            {
                return false;
            }

            if (ms <= 0)
            {
                return false;
            }

            int stream = GetCurrentStream();
            bool result = false;

            try
            {
                //long len = Bass.BASS_ChannelGetLength(stream); // length in bytes

                long pos = 0; // position in bytes
                if (_Mixing)
                {
                    pos = BassMix.BASS_Mixer_ChannelGetPosition(stream);
                }
                else
                {
                    pos = Bass.BASS_ChannelGetPosition(stream);
                }

                double timePos = Bass.BASS_ChannelBytes2Seconds(stream, pos);
                double offsetSecs = ms/1000.0;

                if (timePos - offsetSecs <= 0)
                {
                    return false;
                }

                if (_Mixing)
                {
                    BassMix.BASS_Mixer_ChannelSetPosition(stream,
                                                          Bass.BASS_ChannelSeconds2Bytes(stream, timePos - offsetSecs));
                    // the elapsed time length
                }
                else
                    Bass.BASS_ChannelSetPosition(stream, timePos - offsetSecs); // the elapsed time length
            }

            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Seek to a specific position in the stream
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool SeekToTimePosition(int position)
        {
            Log.Debug("BASS: SeekToTimePosition: {0} ", Convert.ToString(position));
            _CrossFading = false;

            bool result = true;

            try
            {
                int stream = GetCurrentStream();

                if (StreamIsPlaying(stream))
                {
                    if (_Mixing)
                    {
                        BassMix.BASS_Mixer_ChannelSetPosition(stream, Bass.BASS_ChannelSeconds2Bytes(stream, position));
                    }
                    else
                    {
                        Bass.BASS_ChannelSetPosition(stream, (float) position);
                    }
                }
            }

            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Seek Relative in the Stream
        /// </summary>
        /// <param name="dTime"></param>
        public void SeekRelative(double dTime)
        {
            _CrossFading = false;

            if (_State != PlayState.Init)
            {
                double dCurTime = GetStreamElapsedTime();

                dTime = dCurTime + dTime;

                if (dTime < 0.0d)
                {
                    dTime = 0.0d;
                }

                if (dTime < Duration)
                {
                    SeekToTimePosition((int) dTime);
                }
            }
        }

        /// <summary>
        /// Seek Absoluet in the Stream
        /// </summary>
        /// <param name="dTime"></param>
        public void SeekAbsolute(double dTime)
        {
            _CrossFading = false;

            if (_State != PlayState.Init)
            {
                if (dTime < 0.0d)
                {
                    dTime = 0.0d;
                }

                if (dTime < Duration)
                {
                    SeekToTimePosition((int) dTime);
                }
            }
        }

        /// <summary>
        /// Seek Relative Percentage
        /// </summary>
        /// <param name="iPercentage"></param>
        public void SeekRelativePercentage(int iPercentage)
        {
            _CrossFading = false;

            if (_State != PlayState.Init)
            {
                double dCurrentPos = GetStreamElapsedTime();
                double dDuration = Duration;
                double fOnePercentDuration = Duration/100.0d;

                double dSeekPercentageDuration = fOnePercentDuration*iPercentage;
                double dPositionMS = dDuration += dSeekPercentageDuration;

                if (dPositionMS < 0)
                {
                    dPositionMS = 0d;
                }

                if (dPositionMS > dDuration)
                {
                    dPositionMS = dDuration;
                }

                SeekToTimePosition((int) dDuration);
            }
        }

        /// <summary>
        /// Seek Absolute Percentage
        /// </summary>
        /// <param name="iPercentage"></param>
        public void SeekAsolutePercentage(int iPercentage)
        {
            _CrossFading = false;

            if (_State != PlayState.Init)
            {
                if (iPercentage < 0)
                {
                    iPercentage = 0;
                }

                if (iPercentage >= 100)
                {
                    iPercentage = 100;
                }

                if (iPercentage == 0)
                {
                    SeekToTimePosition(0);
                }

                else
                {
                    SeekToTimePosition((int) (Duration*(iPercentage/100d)));
                }
            }
        }

        /// <summary>
        /// Return the dbLevel to be used by a VUMeter
        /// </summary>
        /// <param name="dbLevelL"></param>
        /// <param name="dbLevelR"></param>
        public void RMS(out double dbLevelL, out double dbLevelR)
        {
            int peakL = 0;
            int peakR = 0;
            double dbLeft = 0.0;
            double dbRight = 0.0;

            // Find out with which stream to deal with
            int level = 0;
            if (_Mixing)
            {
                level = BassMix.BASS_Mixer_ChannelGetLevel(GetCurrentStream());
            }
            else
            {
                level = Bass.BASS_ChannelGetLevel(GetCurrentStream());
            }

            peakL = Utils.LowWord32(level); // the left level
            peakR = Utils.HighWord32(level); // the right level

            dbLeft = Utils.LevelToDB(peakL, 65535);
            dbRight = Utils.LevelToDB(peakR, 65535);

            dbLevelL = dbLeft;
            dbLevelR = dbRight;
        }

        public IList<string> GetOutputDevices()
        {
            BASS_DEVICEINFO[] soundDeviceDescriptions = Bass.BASS_GetDeviceInfos();

            var deviceList = (from a in soundDeviceDescriptions select a.name).ToList();

            return deviceList;
        }

        private void ChangeOutputDevice(string newOutputDevice)
        {
            if (newOutputDevice == null)
                throw new BassException("Null value provided to ChangeOutputDevice(string)");

            // Attempt to find the device number for the given string
            int oldDeviceId = Bass.BASS_GetDevice();
            int newDeviceId = -1;
            BASS_DEVICEINFO[] soundDeviceDescriptions = Bass.BASS_GetDeviceInfos();
            for (int i = 0; i < soundDeviceDescriptions.Length; i++)
            {
                if (newOutputDevice.Equals(soundDeviceDescriptions[i].name))
                    newDeviceId = i;
            }
            if (newDeviceId == -1)
                throw new BassException("Cannot find an output device matching description [" + newOutputDevice + "]");

            Log.Info("BASS: Old device ID " + oldDeviceId);
            Log.Info("BASS: New device ID " + newDeviceId);

            // Make sure we're actually changing devices
            if (oldDeviceId == newDeviceId) return;

            // Initialize the new device
            bool initOK = false;
            BASS_DEVICEINFO info = Bass.BASS_GetDeviceInfo(newDeviceId);
            if (!info.IsInitialized)
            {
                Log.Info("BASS: Initializing new device ID " + newDeviceId);
                initOK = (Bass.BASS_Init(newDeviceId, 44100, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero));
                if (!initOK)
                {
                    BASSError error = Bass.BASS_ErrorGetCode();
                    throw new BassException("Cannot initialize output device [" + newOutputDevice + "], error is [" + Enum.GetName(typeof(BASSError), error) + "]");
                }
            }

            // If anything is playing, move the stream to the new output device
            if (State == PlayState.Playing)
            {
                Log.Info("BASS: Moving current stream to new device ID " + newDeviceId);
                int stream = GetCurrentStream();
                Bass.BASS_ChannelSetDevice(stream, newDeviceId);
            }

            // If the previous device was init'd, free it
            if (oldDeviceId >= 0)
            {
                info = Bass.BASS_GetDeviceInfo(oldDeviceId);
                if (info.IsInitialized)
                {
                    Log.Info("BASS: Freeing device " + oldDeviceId);
                    Bass.BASS_SetDevice(oldDeviceId);
                    Bass.BASS_Free();
                    Bass.BASS_SetDevice(newDeviceId);
                }
            }

            _SoundDevice = newOutputDevice; 
        }

        #endregion

        public event PlaybackStartHandler PlaybackStart;

        public event PlaybackStopHandler PlaybackStop;

        public event PlaybackProgressHandler PlaybackProgress;

        public event TrackPlaybackCompletedHandler TrackPlaybackCompleted;

        public event CrossFadeHandler CrossFade;

        public event PlaybackStateChangedHandler PlaybackStateChanged;

        public event InternetStreamSongChangedHandler InternetStreamSongChanged;

        public event DownloadCompleteHandler DownloadComplete;

        public event DownloadCanceledHandler DownloadCanceled;
    }
}