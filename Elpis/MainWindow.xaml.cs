/*

 * 
 * * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of Elpis.
 * Elpis is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * Elpis is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with Elpis. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shell;
using BassPlayer;
using Elpis.Hotkeys;
using Elpis.UpdateSystem;
using GUI.BorderlessWindow;
using GUI.PageTransition;
using NDesk.Options;
using PandoraSharp;
using PandoraSharpPlayer;
using Util;
using Log = Util.Log;
using UserControl = System.Windows.Controls.UserControl;
using PandoraSharp.Plugins;

namespace Elpis
{
    public partial class MainWindow : Window
    {
        #region Globals

        private readonly ErrorPage _errorPage;
        private HotKeyHost _keyHost;
        private readonly LoadingPage _loadingPage;
        private readonly ToolStripSeparator _notifyMenu_BreakSong = new ToolStripSeparator();
        private readonly ToolStripSeparator _notifyMenu_BreakStation = new ToolStripSeparator();
        private readonly ToolStripSeparator _notifyMenu_BreakVote = new ToolStripSeparator();
        private readonly ToolStripSeparator _notifyMenu_BreakExit = new ToolStripSeparator();
        private About _aboutPage;

        private string _configLocation;

        private Config _config;

        private bool _finalComplete;
        private bool _initComplete;
        private LoginPage _loginPage;

        private NotifyIcon _notify;
        private ContextMenuStrip _notifyMenu;
        private ToolStripMenuItem _notifyMenu_Album;
        private ToolStripMenuItem _notifyMenu_Artist;
        private ToolStripMenuItem _notifyMenu_Next;
        private ToolStripMenuItem _notifyMenu_PlayPause;
        private ToolStripMenuItem _notifyMenu_Stations;
        private ToolStripMenuItem _notifyMenu_Title;
        private ToolStripMenuItem _notifyMenu_UpVote;
        private ToolStripMenuItem _notifyMenu_DownVote;
        private ToolStripMenuItem _notifyMenu_Exit;
        private Player _player;
        private PlaylistPage _playlistPage;
        private UserControl _prevPage;
        private Search _searchPage;
        private Settings _settingsPage;
        private bool _showingError;
        private string _startupStation = null;
        private bool _stationLoaded;

        private SearchMode _searchMode = SearchMode.NewStation;

        private bool _configError = false;

        private bool _forceClose = false;

        private StationList _stationPage;
        private QuickMixPage _quickMixPage;
        private UpdateCheck _update;
        private UpdatePage _updatePage;
        private RestartPage _restartPage;
        private LastFMAuthPage _lastFMPage;

        private ErrorCodes _lastError = ErrorCodes.SUCCESS;
        private Exception _lastException = null;

        private PandoraSharpScrobbler _scrobbler;

        private bool _isActiveWindow;

#endregion

#region Release Data Values

        private string _bassRegEmail = "";
        private string _bassRegKey = "";

        public string ConfigLocation
        {
            get { return _configLocation; }
            set { _configLocation = value; }
        }

        public string StartupStation
        {
            get { return _startupStation; }
            set { _startupStation = value; }
        }

        public void InitReleaseData()
        {
#if APP_RELEASE
            _bassRegEmail = ReleaseData.BassRegEmail;
            _bassRegKey = ReleaseData.BassRegKey;
#endif
        }

#endregion

        public MainWindow()
        {
            InitializeComponent();

            //ContentBackground.Background.Opacity = 1.0;
            new WindowResizer(this,
                              new WindowBorder(BorderPosition.TopLeft, topLeft),
                              new WindowBorder(BorderPosition.Top, top),
                              new WindowBorder(BorderPosition.TopRight, topRight),
                              new WindowBorder(BorderPosition.Right, right),
                              new WindowBorder(BorderPosition.BottomRight, bottomRight),
                              new WindowBorder(BorderPosition.Bottom, bottom),
                              new WindowBorder(BorderPosition.BottomLeft, bottomLeft),
                              new WindowBorder(BorderPosition.Left, left));

            TitleBar.MouseLeftButtonDown += ((o, e) => DragMove());
            MinimizeButton.MouseLeftButtonDown += ((o, e) => WindowState = WindowState.Minimized);
            CloseButton.MouseLeftButtonDown += ((o, e) => Close());

            _errorPage = new ErrorPage();
            _errorPage.ErrorClose += _errorPage_ErrorClose;
            transitionControl.AddPage(_errorPage);

            _loadingPage = new LoadingPage();
            transitionControl.AddPage(_loadingPage);

            _update = new UpdateCheck();

            transitionControl.ShowPage(_loadingPage);

            _config = new Config(_configLocation ?? "");

            if (!_config.LoadConfig())
            {
                _configError = true;
            }
            else
            {
                if (_config.Fields.Proxy_Address != string.Empty)
                    PRequest.SetProxy(_config.Fields.Proxy_Address, _config.Fields.Proxy_Port,
                        _config.Fields.Proxy_User, _config.Fields.Proxy_Password);

                var loc = _config.Fields.Elpis_StartupLocation;
                var size = _config.Fields.Elpis_StartupSize;

                if (loc.X != -1 && loc.Y != -1)
                {
                    this.Left = loc.X;
                    this.Top = loc.Y;
                }

                if (size.Width != 0 && size.Height != 0)
                {
                    this.Width = size.Width;
                    this.Height = size.Height;
                }
            }


        }

        public static CommandLineOptions _clo;
        public static void SetCommandLine(CommandLineOptions clo)
        {
            _clo = clo;
        }

        public void DoCommandLine()
        {
            if (_clo.SkipTrack)
            {
                SkipTrack(null, null);
            }

            if (_clo.TogglePlayPause)
            {
                PlayPauseToggled(null, null);
            }

            if (_clo.DoThumbsUp)
            {
                ExecuteThumbsUp(null, null);
            }

            if (_clo.DoThumbsDown)
            {
                ExecuteThumbsDown(null, null);
            }

            if (_clo.StationToLoad != null)
            {
                LoadStation(_clo.StationToLoad);
            }
        }

        public void ShowWindow()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Microsoft.Shell.NativeMethods.ShowToFront((new System.Windows.Interop.WindowInteropHelper(this)).Handle);
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: Elpis [OPTIONS]");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

#region Setups

        private void SetupLogging()
        {
            if (_config.Fields.Debug_WriteLog)
            {
                _loadingPage.UpdateStatus("Initializing logging...");
                string logFilename = "elpis{0}.log";
                if (_config.Fields.Debug_Timestamp)
                    logFilename = string.Format(logFilename, DateTime.Now.ToString("_MMdd-hhmmss"));
                else
                    logFilename = string.Format(logFilename, "");

                string path = Path.Combine(_config.Fields.Debug_Logpath, logFilename);

                if (!Directory.Exists(_config.Fields.Debug_Logpath))
                    Directory.CreateDirectory(_config.Fields.Debug_Logpath);

                Log.SetLogPath(path);
            }
        }

        private void CloseSettings()
        {
            _scrobbler.IsEnabled = _config.Fields.LastFM_Scrobble;
            RestorePrevPage();
        }

        private void SetupPageEvents()
        {
            _settingsPage.Close += CloseSettings;
            _settingsPage.Restart += _settingsPage_Restart;
            _settingsPage.LastFMAuthRequest += _settingsPage_LastFMAuthRequest;
            _settingsPage.LasFMDeAuthRequest += _settingsPage_LasFMDeAuthRequest;
            _restartPage.RestartSelectionEvent += _restartPage_RestartSelectionEvent;
            _lastFMPage.ContinueEvent += _lastFMPage_ContinueEvent;
            _lastFMPage.CancelEvent += _lastFMPage_CancelEvent;
            _aboutPage.Close += RestorePrevPage;

            _searchPage.Cancel += _searchPage_Cancel;
            _searchPage.AddVariety += _searchPage_AddVariety;
            _loginPage.ConnectingEvent += _loginPage_ConnectingEvent;
        }

        void _settingsPage_LasFMDeAuthRequest()
        {
            _config.Fields.LastFM_SessionKey = string.Empty;
            _config.Fields.LastFM_Scrobble = false;
            _config.SaveConfig();
        }

        void _settingsPage_LastFMAuthRequest()
        {
            this.BeginDispatch(() =>
                {
                    try
                    {
                        string url = _scrobbler.GetAuthUrl();
                        _lastFMPage.SetAuthURL(url);
                        _scrobbler.LaunchAuthPage();

                        transitionControl.ShowPage(_lastFMPage);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ErrorCodes.ERROR_GETTING_TOKEN, ex);
                    }
                });
        }

        void _lastFMPage_CancelEvent()
        {
            transitionControl.ShowPage(_settingsPage);
        }

        void _lastFMPage_ContinueEvent()
        {
            this.Dispatch(() => GetLastFMSessionKey());
        }

        void _settingsPage_Restart()
        {
            transitionControl.ShowPage(_restartPage);
        }

        void DoRestart()
        {

            System.Collections.Generic.List<string> args = 
                new System.Collections.Generic.List<string>();
            var cmds = System.Environment.GetCommandLineArgs();
            foreach (string a in cmds)
                args.Add(a);

            args.RemoveAt(0);
            args.Remove("-restart");

            string sArgs = string.Empty;
            foreach(string s in args)
                sArgs += (s + " ");

            sArgs += " -restart";
            //Process.Start("Elpis.exe", sArgs);
            try
            {
                //run the program again and close this one
                Process.Start("Elpis.exe", sArgs);
                //or you can use Application.ExecutablePath

                //close this one
                Process.GetCurrentProcess().Kill();
            }
            catch
            { }
        }

        void _restartPage_RestartSelectionEvent(bool status)
        {
            if (status)
            {
                DoRestart();
                Close();
            }
            else
            {
                RestorePrevPage();
            }
        }

        DateTime _lastFMStart;
        bool _lastFMAuth = false;

        private void DoLastFMAuth()
        {
            try
            {
                _lastFMStart = DateTime.Now;
                while ((DateTime.Now - _lastFMStart).TotalMilliseconds < 5000) Thread.Sleep(10);

                string sk = _scrobbler.GetAuthSessionKey();
                _config.Fields.LastFM_Scrobble = true;
                _config.Fields.LastFM_SessionKey = sk;
                _config.SaveConfig();

                DoLastFMSuccess();
            }
            catch (Exception ex)
            {
                _config.Fields.LastFM_Scrobble = false;
                _config.Fields.LastFM_SessionKey = string.Empty;
                _config.SaveConfig();

                DoLastFMError(ex);
            }
        }

        private void DoLastFMSuccess()
        {
            _lastFMStart = DateTime.Now;
            this.BeginDispatch(() => _loadingPage.UpdateStatus("Success!"));
            while ((DateTime.Now - _lastFMStart).TotalMilliseconds < 1500) Thread.Sleep(10);
            this.BeginDispatch(() => transitionControl.ShowPage(_settingsPage));
            _lastFMAuth = false;
        }

        private void DoLastFMError(Exception ex)
        {
            _lastFMStart = DateTime.Now;
            this.BeginDispatch(() =>
                {
                    _lastError = ErrorCodes.ERROR_GETTING_SESSION;
                    //ShowError(_lastError, ex);
                    _loadingPage.UpdateStatus("Error Fetching Last.FM Session");
                });
            while ((DateTime.Now - _lastFMStart).TotalMilliseconds < 3000) Thread.Sleep(10);
            this.BeginDispatch(() => transitionControl.ShowPage(_settingsPage));
            _lastFMAuth = false;
        }

        private void GetLastFMSessionKey()
        {
            _lastFMAuth = true;
            _lastFMStart = DateTime.Now;
            _loadingPage.UpdateStatus("Fetching Last.FM Session");
            transitionControl.ShowPage(_loadingPage);

            Task.Factory.StartNew(() => DoLastFMAuth());
        }

        private void SetupUIEvents()
        {
            _player.ConnectionEvent += _player_ConnectionEvent;
            _player.LogoutEvent += _player_LogoutEvent;
            _player.StationLoaded += _player_StationLoaded;
            _player.StationsRefreshed += _player_StationsRefreshed;
            _player.StationsRefreshing += _player_StationsRefreshing;
            _player.ExceptionEvent += _player_ExceptionEvent;
            _player.PlaybackStateChanged += _player_PlaybackStateChanged;
            _player.LoginStatusEvent += _player_LoginStatusEvent;
            _player.PlaybackStart += _player_PlaybackStart;
            _player.StationCreated += _player_StationCreated;

            mainBar.PlayPauseClick += mainBar_PlayPauseClick;
            mainBar.NextClick += mainBar_NextClick;
            mainBar.AboutClick += mainBar_AboutClick;
            mainBar.SettingsClick += mainBar_SettingsClick;
            mainBar.StationListClick += mainBar_stationPageClick;
            mainBar.CreateStationClick += mainBar_searchPageClick;
            mainBar.ErrorClicked += mainBar_ErrorClicked;
            mainBar.VolumeChanged += mainBar_VolumeChanged;
            
            _loginPage.Loaded += _loginPage_Loaded;
            _aboutPage.Loaded += _aboutPage_Loaded;
            _settingsPage.Loaded += _settingsPage_Loaded;
            _settingsPage.Logout += _settingsPage_Logout;
            _searchPage.Loaded += _searchPage_Loaded;
            _stationPage.Loaded += _stationPage_Loaded;
            _stationPage.EditQuickMixEvent += _stationPage_EditQuickMixEvent;
            _stationPage.AddVarietyEvent += _stationPage_AddVarietyEvent;
            _quickMixPage.CancelEvent += _quickMixPage_CancelEvent;
            _quickMixPage.CloseEvent += _quickMixPage_CloseEvent;
            _playlistPage.Loaded += _playlistPage_Loaded;
        }

        private void SetupPages()
        {
            _searchPage = new Search(_player);
            transitionControl.AddPage(_searchPage);

            _settingsPage = new Settings(_player, _config, _keyHost);
            transitionControl.AddPage(_settingsPage);

            _restartPage = new RestartPage();
            transitionControl.AddPage(_restartPage);

            _aboutPage = new About();
            transitionControl.AddPage(_aboutPage);

            _stationPage = new StationList(_player);
            transitionControl.AddPage(_stationPage);

            _quickMixPage = new QuickMixPage(_player);
            transitionControl.AddPage(_quickMixPage);

            _loginPage = new LoginPage(_player, _config);
            transitionControl.AddPage(_loginPage);

            _playlistPage = new PlaylistPage(_player);
            transitionControl.AddPage(_playlistPage);

            _lastFMPage = new LastFMAuthPage();
            transitionControl.AddPage(_lastFMPage);
        }

        private void StationMenuClick(object sender, EventArgs e)
        {
            var station = (Station) ((ToolStripMenuItem) sender).Tag;
            _player.PlayStation(station);
        }

        private void AddStationMenuItems()
        {
            if (_notify != null && _notifyMenu != null && _player.Stations.Count > 0)
            {
                _notifyMenu_Stations.DropDown.Items.Clear();
                foreach (Station s in _player.Stations)
                {
                    var menu = new ToolStripMenuItem(s.Name);
                    menu.Click += StationMenuClick;
                    menu.Tag = s;
                    _notifyMenu_Stations.DropDown.Items.Add(menu);
                }
            }
        }

        private void LoadNotifyDetailUrl(object sender, EventArgs e)
        {
            try
            {
                Process.Start((string) ((ToolStripMenuItem) sender).Tag);
            }
            catch
            {
            }
        }

        private void LoadNotifyMenu()
        {
            bool showSongInfo = !_player.Stopped;
            bool showStations = false;
            if (_player.Stations != null)
                showStations = _player.Stations.Count > 0;

            _notifyMenu_Title.Visible =
                _notifyMenu_Artist.Visible =
                _notifyMenu_Album.Visible =
                _notifyMenu_BreakSong.Visible = 
                _notifyMenu_DownVote.Visible =
                _notifyMenu_UpVote.Visible =
                _notifyMenu_BreakVote.Visible = showSongInfo;

            _notifyMenu_PlayPause.Enabled =
                _notifyMenu_Next.Enabled = showSongInfo;

            if (showSongInfo)
            {
                _notifyMenu_Title.Text = _player.CurrentSong.SongTitle;
                _notifyMenu_Title.Tag = _player.CurrentSong.SongDetailUrl;

                _notifyMenu_Artist.Text = "by " + _player.CurrentSong.Artist;
                _notifyMenu_Artist.Tag = _player.CurrentSong.ArtistDetailUrl;

                _notifyMenu_Album.Text = "on " + _player.CurrentSong.Album;
                _notifyMenu_Album.Tag = _player.CurrentSong.AlbumDetailUrl;

                _notifyMenu_PlayPause.Text = _player.Playing ? "Pause" : "Play";
            }

            _notifyMenu_BreakStation.Visible =
                _notifyMenu_Stations.Visible = showStations;

            _notifyMenu_BreakExit.Visible = _notifyMenu_Exit.Visible = true;

            if (showStations)
                AddStationMenuItems();
        }

        private void SetupNotifyIcon()
        {
            _notifyMenu_Title = new ToolStripMenuItem("Title");
            _notifyMenu_Title.Click += LoadNotifyDetailUrl;
            _notifyMenu_Title.Image = Properties.Resources.menu_info;

            _notifyMenu_Artist = new ToolStripMenuItem("Artist");
            _notifyMenu_Artist.Click += LoadNotifyDetailUrl;
            _notifyMenu_Artist.Image = Properties.Resources.menu_info;

            _notifyMenu_Album = new ToolStripMenuItem("Album");
            _notifyMenu_Album.Click += LoadNotifyDetailUrl;
            _notifyMenu_Album.Image = Properties.Resources.menu_info;

            _notifyMenu_PlayPause = new ToolStripMenuItem("Play");
            _notifyMenu_PlayPause.Click += ((o, e) => _player.PlayPause());

            _notifyMenu_Next = new ToolStripMenuItem("Next Song");
            _notifyMenu_Next.Click += ((o, e) => _player.Next());

            _notifyMenu_Stations = new ToolStripMenuItem("Stations");

            _notifyMenu_DownVote = new ToolStripMenuItem("Dislike Song");
            _notifyMenu_DownVote.Click += ((o, e) => _playlistPage.ThumbDownCurrent() );

            _notifyMenu_UpVote = new ToolStripMenuItem("Like Song");
            _notifyMenu_UpVote.Click += ((o, e) => _playlistPage.ThumbUpCurrent() );


            _notifyMenu_Exit = new ToolStripMenuItem("Exit Elpis");
            _notifyMenu_Exit.Click += ((o, e) => { _forceClose = true; Close(); });

            var menus = new ToolStripItem[]
                            {
                                _notifyMenu_Title,
                                _notifyMenu_Artist,
                                _notifyMenu_Album,
                                _notifyMenu_BreakSong,
                                _notifyMenu_PlayPause,
                                _notifyMenu_Next,
                                _notifyMenu_BreakVote,
                                _notifyMenu_UpVote,
                                _notifyMenu_DownVote,
                                _notifyMenu_BreakStation,
                                _notifyMenu_Stations,
                                _notifyMenu_BreakExit,
                                _notifyMenu_Exit
                            };

            _notifyMenu = new ContextMenuStrip();
            _notifyMenu.Items.AddRange(menus);

            _notify = new NotifyIcon()
                          {
                              Text = "Elpis",
                              Icon = Properties.Resources.main_icon,
                              ContextMenuStrip = _notifyMenu,
                          };

            _notify.DoubleClick += ((o, e) =>
                                        {
                                            ShowWindow();
                                        });
            _notify.ContextMenuStrip.Opening += ((o, e) => LoadNotifyMenu());

            _notify.Visible = true;
        }

        private bool InitLogic()
        {
            while (transitionControl.CurrentPage != _loadingPage) Thread.Sleep(10);
            _loadingPage.UpdateStatus("Loading configuration...");
            InitReleaseData();

            if (_configError)
            {
                this.BeginDispatch(() => ShowError(ErrorCodes.CONFIG_LOAD_ERROR, null));
                return false;
            }

            try
            {
                SetupLogging();
            }
            catch (Exception ex)
            {
                ShowError(ErrorCodes.LOG_SETUP_ERROR, ex);
                return false;
            }
            _initComplete = true;
            return true;
        }

        private void FinalLoad()
        {
            Version ver = Assembly.GetEntryAssembly().GetName().Version;
            if (_config.Fields.Elpis_Version == null || _config.Fields.Elpis_Version < ver)
            {
                _loadingPage.UpdateStatus("Running update logic...");

                string oldVer = _config.Fields.Elpis_Version.ToString();

                _config.Fields.Elpis_Version = ver;
                _config.SaveConfig();

#if APP_RELEASE
                var post = new PostSubmitter(ReleaseData.AnalyticsPostURL);

                post.Add("guid", _config.Fields.Elpis_InstallID);
                post.Add("curver", oldVer);
                post.Add("newver", _config.Fields.Elpis_Version.ToString());
                post.Add("osver", SystemInfo.GetWindowsVersion());

                try
                {
                    post.Send();  
                }
                catch(Exception ex)
                {
                    Log.O(ex.ToString());
                }
#endif
            }

            _loadingPage.UpdateStatus("Loading audio engine...");
            try
            {
                _player = new Player();
                _player.Initialize(_bassRegEmail, _bassRegKey); //TODO - put this in the login sequence? 
                if(_config.Fields.Proxy_Address != string.Empty)
                    _player.SetProxy(_config.Fields.Proxy_Address, _config.Fields.Proxy_Port,
                        _config.Fields.Proxy_User, _config.Fields.Proxy_Password);
            }
            catch(Exception ex)
            {
                ShowError(ErrorCodes.ENGINE_INIT_ERROR, ex);
                return;
            }

            LoadLastFM();

            _player.AudioFormat = _config.Fields.Pandora_AudioFormat;
            _player.SetStationSortOrder(_config.Fields.Pandora_StationSortOrder);
            _player.Volume = _config.Fields.Elpis_Volume;
            _player.PauseOnLock = _config.Fields.Elpis_PauseOnLock;
            _player.MaxPlayed = _config.Fields.Elpis_MaxHistory;

            //_player.ForceSSL = _config.Fields.Misc_ForceSSL;


            _loadingPage.UpdateStatus("Setting up cache...");
            string cachePath = Path.Combine(Config.ElpisAppData, "Cache");
            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
            _player.ImageCachePath = cachePath;

            _loadingPage.UpdateStatus("Setting up UI...");

            this.Dispatch(() => {
                _keyHost = new HotKeyHost(this);
                ConfigureHotKeys();
            });

            //this.Dispatch(SetupJumpList);
            
            this.Dispatch(SetupNotifyIcon);

            this.Dispatch(() => mainBar.DataContext = _player); //To bind playstate

            this.Dispatch(SetupPages);
            this.Dispatch(SetupUIEvents);
            this.Dispatch(SetupPageEvents);

            if (_config.Fields.Login_AutoLogin &&
                (!string.IsNullOrEmpty(_config.Fields.Login_Email)) &&
                (!string.IsNullOrEmpty(_config.Fields.Login_Password)))
            {
                _player.Connect(_config.Fields.Login_Email, _config.Fields.Login_Password);
            }
            else
            {
                transitionControl.ShowPage(_loginPage);
            }

            this.Dispatch(() => mainBar.Volume = _player.Volume);

            _finalComplete = true;
        }



        private void LoadLastFM()
        {
            string apiKey = string.Empty;
            string apiSecret = string.Empty;
#if APP_RELEASE
                apiKey = ReleaseData.LastFMApiKey;
                apiSecret = ReleaseData.LastFMApiSecret;
#else
            //Put your own Last.FM API keys here
            apiKey = "dummy_key";
            apiSecret = "dummy_key";
#endif

            if (!string.IsNullOrEmpty(_config.Fields.LastFM_SessionKey))
                _scrobbler = new PandoraSharpScrobbler(apiKey, apiSecret, _config.Fields.LastFM_SessionKey);
            else
                _scrobbler = new PandoraSharpScrobbler(apiKey, apiSecret);

            _scrobbler.IsEnabled = _config.Fields.LastFM_Scrobble;
#if APP_RELEASE
#else
            if (_config.Fields.LastFM_Scrobble && !_scrobbler.IsEnabled)
            {
                System.Windows.MessageBox.Show("You are trying to use Last.FM Scrobbler without a LastFM API key. " +
                                               "In order to use it while in Debug mode, edit apiKey and apiSecret in LoadLastFM() in MainWindow.xaml.cs");
            }
#endif
 

            if (_config.Fields.Proxy_Address != string.Empty)
                _scrobbler.SetProxy(_config.Fields.Proxy_Address, _config.Fields.Proxy_Port,
                        _config.Fields.Proxy_User, _config.Fields.Proxy_Password);

            _player.RegisterPlayerControlQuery(_scrobbler);
        }

        private void LoadLogic()
        {
            if (InitLogic())
            {
#if APP_RELEASE
                _update = new UpdateCheck();
                if (_config.Fields.Elpis_CheckUpdates)
                {
                    _loadingPage.UpdateStatus("Checking for updates...");
                    if (_update.CheckForUpdate())
                    {
                        this.BeginDispatch(() =>
                                               {
                                                   _updatePage = new UpdatePage(_update);
                                                   _updatePage.UpdateSelectionEvent += _updatePage_UpdateSelectionEvent;
                                                   transitionControl.AddPage(_updatePage);
                                                   transitionControl.ShowPage(_updatePage);
                                               });
                    }
                    else
                    {
                        FinalLoad();
                    }
                }
                else
                {
                    FinalLoad();
                }
#else
                FinalLoad();
#endif
            }
        }

#endregion

#region Misc Methods
        private bool IsOnPlaylist()
        {
            return (IsActive && transitionControl.CurrentPage == _playlistPage);
        }

        private void SetupJumpList()
        {
            //JumpList jumpList = new JumpList();
            //jumpList.ShowRecentCategory = true;
            //JumpList.SetJumpList(System.Windows.Application.Current, jumpList);

            //JumpTask pause = JumpListManager.createJumpTask(PlayerCommands.PlayPause, "--playpause",1);
            //jumpList.JumpItems.Add(pause);

            //JumpTask next = JumpListManager.createJumpTask(PlayerCommands.Next, "--next",2);
            //jumpList.JumpItems.Add(next);

            //JumpTask thumbsUp  = JumpListManager.createJumpTask(PlayerCommands.ThumbsUp, "--thumbsup",3);
            //jumpList.JumpItems.Add(thumbsUp);

            //JumpTask thumbsDown = JumpListManager.createJumpTask(PlayerCommands.ThumbsDown, "--thumbsdown",4);
            //jumpList.JumpItems.Add(thumbsDown);

            //jumpList.Apply();
        }


        private void ConfigureHotKeys()
        {

            foreach(HotKey h in _config.Fields.Elpis_HotKeys.Values)
            {
                _keyHost.AddHotKey(h);
            }
            if(new List<HotKey>(_config.Fields.Elpis_HotKeys.Values).Count==0)
            {
                _keyHost.AddHotKey(new HotKey(PlayerCommands.PlayPause, Key.Space, ModifierKeys.None, false, true));

                _keyHost.AddHotKey(new HotKey(PlayerCommands.PlayPause, Key.MediaPlayPause, ModifierKeys.None, true, true));

                _keyHost.AddHotKey(new HotKey(PlayerCommands.Next, Key.Right, ModifierKeys.None, false, true));

                _keyHost.AddHotKey(new HotKey(PlayerCommands.Next, Key.MediaNextTrack, ModifierKeys.None, true, true));

                _keyHost.AddHotKey(new HotKey(PlayerCommands.ThumbsUp, Key.Up, ModifierKeys.None, false, true));

                _keyHost.AddHotKey(new HotKey(PlayerCommands.ThumbsDown, Key.Down, ModifierKeys.None, false, true));
            }

            Dictionary<int, HotkeyConfig> keys = new Dictionary<int, HotkeyConfig>();
            foreach (KeyValuePair<int, HotKey> pair in _keyHost.HotKeys)
            {
                keys.Add(pair.Key, new HotkeyConfig(pair.Value));
            }
            _config.Fields.Elpis_HotKeys = keys;

            _config.SaveConfig();
        }

        public void ShowStationList()
        {
            _stationPage.Stations = _player.Stations;
            transitionControl.ShowPage(_stationPage, PageTransitionType.Next);
        }

        private void RestorePrevPage()
        {
            transitionControl.ShowPage(_prevPage, PageTransitionType.Next);
            _prevPage = null;
        }

        private void ShowErrorPage(ErrorCodes code, Exception ex)
        {
            if (!_showingError)
            {
                _showingError = true;

                _prevPage = transitionControl.CurrentPage;
                _errorPage.SetError(Errors.GetErrorMessage(code), Errors.IsHardFail(code), ex);
                transitionControl.ShowPage(_errorPage);
            }
        }

        private void ShowError(ErrorCodes code, Exception ex, bool showLast = false)
        {
            if (transitionControl.CurrentPage != _errorPage)
            {
                if(showLast && _lastError != ErrorCodes.SUCCESS)
                {
                    ShowErrorPage(_lastError, _lastException);
                }
                else if (code != ErrorCodes.SUCCESS && ex != null)
                {
                    if(Errors.IsHardFail(code))
                    {
                        ShowErrorPage(code, ex);
                    }
                    else
                    {
                        _lastError = code;
                        _lastException = ex;
                        mainBar.ShowError(Errors.GetErrorMessage(code));

                        if (transitionControl.CurrentPage == _loadingPage && !_lastFMAuth)
                        {
                            _loginPage.LoginFailed = true;
                            transitionControl.ShowPage(_loginPage);
                        }
                    }
                }
            }
        }
#endregion

        protected override void OnActivated(EventArgs e)
        {
            _isActiveWindow = true;
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            _isActiveWindow = false;
            base.OnDeactivated(e);
        }

#region Event Handlers

        void mainBar_ErrorClicked()
        {
            ShowError(ErrorCodes.SUCCESS, null, true);
        }

        private void _player_StationsRefreshing(object sender)
        {
            _stationPage.SetStationsRefreshing();
        }

        private void _loginPage_ConnectingEvent()
        {
            this.BeginDispatch(() => transitionControl.ShowPage(_loadingPage, PageTransitionType.Next));
        }

        private void _errorPage_ErrorClose(bool hardFail)
        {
            if (hardFail || _prevPage == null)
                Close();
            else
            {
                _lastError = ErrorCodes.SUCCESS;
                _lastException = null;
                RestorePrevPage();
                _showingError = false;
            }
        }

        private void _updatePage_UpdateSelectionEvent(bool status)
        {
            if (status && _update.DownloadUrl != string.Empty)
            {
                //Process.Start(_update.DownloadUrl);
                Close();
            }
            else
            {
                transitionControl.ShowPage(_loadingPage);
            }
        }

        private void _player_StationCreated(object sender, Station station)
        {
            _player.RefreshStations();
            this.BeginDispatch(() => _player.PlayStation(station));
        }

        private void _searchPage_Cancel(object sender)
        {
            this.BeginDispatch(() =>
                                   {
                                       if (_searchMode == SearchMode.AddVariety)
                                           ShowStationList();
                                       else
                                       {
                                           if (_prevPage == _stationPage)
                                               ShowStationList();
                                           else
                                               RestorePrevPage();
                                           //transitionControl.ShowPage(_playlistPage);
                                       }
                                   });
        }

        void _searchPage_AddVariety(object sender)
        {
            ShowStationList();
        }

        private void _player_PlaybackStart(object sender, double duration)
        {
            this.BeginDispatch(() =>
            {
                if (_config.Fields.Elpis_ShowTrayNotifications)
                {
                    string tipText = _player.CurrentSong.SongTitle;
                    _notify.BalloonTipTitle = tipText;
                    _notify.BalloonTipText = " by " + _player.CurrentSong.Artist;
    
                    _notify.ShowBalloonTip(5000);
                }
            });
        }

        private void _player_PlaybackStateChanged(object sender, BassAudioEngine.PlayState oldState,
                                                  BassAudioEngine.PlayState newState)
        {
            this.BeginDispatch(() =>
                                   {
                                       if (newState == BassAudioEngine.PlayState.Playing)
                                       {
                                           string title = "Elpis | " + _player.CurrentSong.Artist + " / " +
                                                          _player.CurrentSong.SongTitle;

                                           _notify.Text = title.StringEllipses(63);
                                               //notify text cannot be more than 63 chars
                                           Title = title;
                                       }
                                       else if (newState == BassAudioEngine.PlayState.Paused)
                                       {
                                           Title = _notify.Text = "Elpis";
                                       }
                                       else if (newState == BassAudioEngine.PlayState.Stopped)
                                       {
                                           mainBar.SetPlaying(false);
                                           Title = _notify.Text = "Elpis";
                                           if(_player.LoggedIn)
                                              ShowStationList();
                                       }
                                   });
        }

        private void _player_ExceptionEvent(object sender, ErrorCodes code, Exception ex)
        {
            ShowError(code, ex);
        }

        private void _player_LoginStatusEvent(object sender, string status)
        {
            _loadingPage.UpdateStatus(status);
        }

        void _stationPage_EditQuickMixEvent()
        {
            transitionControl.ShowPage(_quickMixPage);
        }

        void _stationPage_AddVarietyEvent(Station station)
        {
            _searchPage.SearchMode = _searchMode = SearchMode.AddVariety;
            _searchPage.VarietyStation = station;
            transitionControl.ShowPage(_searchPage);
        }

        void _quickMixPage_CloseEvent()
        {
            ShowStationList();
        }

        void _quickMixPage_CancelEvent()
        {
            ShowStationList();
        }

        private void _playlistPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.O("Show Playlist");
            mainBar.SetModePlayList();
        }

        private void _stationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.O("Show Stations");
            mainBar.SetModeStationList(_player.CurrentStation != null);
        }

        private void _searchPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.O("Show Search");
            mainBar.SetModeSearch();
        }

        void _settingsPage_Logout()
        {
            if (_player.LoggedIn)
                _player.Logout();
        }

        private void _settingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.O("Show Settings");
            mainBar.SetModeSettings();
        }

        private void _aboutPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.O("Show About");
            mainBar.SetModeAbout();
        }

        private void _loginPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.O("Show Login");
            mainBar.SetModeLogin();
        }

        private void mainBar_stationPageClick()
        {
            //_prevPage = transitionControl.CurrentPage;
            if (transitionControl.CurrentPage == _stationPage || transitionControl.CurrentPage == _quickMixPage)
            {
                if (_stationLoaded)
                    transitionControl.ShowPage(_playlistPage);
            }
            else
            {
                transitionControl.ShowPage(_stationPage);
            }
        }

        private void mainBar_searchPageClick()
        {
            _prevPage = transitionControl.CurrentPage;
            _searchPage.SearchMode = _searchMode = SearchMode.NewStation;
            transitionControl.ShowPage(_searchPage, PageTransitionType.Previous);
        }

        private void mainBar_VolumeChanged(double vol)
        {
            _player.Volume = (int)vol;
        }

        private void mainBar_SettingsClick()
        {
            if (_prevPage == null)
                _prevPage = transitionControl.CurrentPage;

            if (transitionControl.CurrentPage == _settingsPage)
                RestorePrevPage();
            else
                transitionControl.ShowPage(_settingsPage, PageTransitionType.Previous);
        }

        private void mainBar_AboutClick()
        {
            if (_prevPage == null)
                _prevPage = transitionControl.CurrentPage;

            if (transitionControl.CurrentPage == _aboutPage)
                RestorePrevPage();
            else
                transitionControl.ShowPage(_aboutPage, PageTransitionType.Previous);
        }

        private void _player_StationsRefreshed(object sender)
        {
            _stationPage.Stations = _player.Stations;
        }

        private void mainBar_NextClick()
        {
            //if (transitionControl.CurrentPage == _playlistPage)
            _player.Next();

            transitionControl.ShowPage(_playlistPage);   
        }

        private void mainBar_PlayPauseClick()
        {
            //if (transitionControl.CurrentPage == _playlistPage)
            _player.PlayPause();

            transitionControl.ShowPage(_playlistPage);    
        }

        private void _player_StationLoaded(object sender, Station station)
        {
            this.BeginDispatch(() =>
                                   {
                                       mainBar.SetModePlayList();
                                       transitionControl.ShowPage(_playlistPage,
                                                                  PageTransitionType.Next);
                                       if (_config.Fields.Pandora_AutoPlay)
                                       {
                                           _config.Fields.Pandora_LastStationID = station.ID;
                                           _config.SaveConfig();
                                       }

                                       _stationLoaded = true;
                                   }
                );
        }

        void _player_LogoutEvent(object sender)
        {
            transitionControl.ShowPage(_loginPage);
        }

        private void _player_ConnectionEvent(object sender, bool state, ErrorCodes code)
        {
            if (state)
            {
                if (_config.Fields.Pandora_AutoPlay)
                {
                    Station s = null;
                    if (StartupStation != null)
                        s = _player.GetStationFromString(StartupStation);
                    if (s == null)
                    {
                        s = _player.GetStationFromID(_config.Fields.Pandora_LastStationID);
                    }
                    if (s != null)
                    {
                        _loadingPage.UpdateStatus("Loading Station:" + Environment.NewLine + s.Name);
                        _player.PlayStation(s);
                    }
                    else
                    {
                        ShowStationList();
                    }
                }
                else
                {
                    this.BeginDispatch(ShowStationList);
                }
            }
            else
            {
                transitionControl.ShowPage(_loginPage);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(LoadLogic);
        }

        private void transitionControl_CurrentPageSet(UserControl page)
        {
            if (page == _loadingPage && _initComplete && !_finalComplete)
                Task.Factory.StartNew(FinalLoad);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_forceClose && _config.Fields.Elpis_MinimizeToTray)
            {
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;

                e.Cancel = true;
                return;
            }

            if (_notify != null)
            {
                _notify.Dispose();
                _notify = null;
            }

            if (_config != null)
            {
                _config.Fields.Elpis_StartupLocation = new Point(this.Left, this.Top);
                _config.Fields.Elpis_StartupSize = new Size(this.Width, this.Height);
                if(_player != null)
                    _config.Fields.Elpis_Volume = _player.Volume;
                _config.SaveConfig();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _config.Fields.Elpis_MinimizeToTray)
                ShowInTaskbar = false;
            else
                ShowInTaskbar = true;
        }

        public void LoadStation(string station)
        {
            Station s = _player.GetStationFromString(station);
            if(s != null)
            {
                _player.PlayStation(s);
            }
        }

        public void PlayPauseToggled(object sender, ExecutedRoutedEventArgs e)
        {
            _player.PlayPause();
        }

        public void SkipTrack(object sender, ExecutedRoutedEventArgs e)
        {
            _player.Next();
        }

        private void CanExecutePlayPauseSkip(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!_isActiveWindow)
            {
                e.CanExecute = true;
            }
            else
            {
                if (IsOnPlaylist())
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }

        public void ExecuteThumbsUp(object sender, ExecutedRoutedEventArgs e)
        {
            _playlistPage.ThumbUpCurrent();
        }


        public void ExecuteThumbsDown(object sender, ExecutedRoutedEventArgs e)
        {
            _playlistPage.ThumbDownCurrent();
        }

        private void CanExecuteThumbsUpDown(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!_isActiveWindow&& _player.CurrentSong != null)
            {
                e.CanExecute = true;
            }
            else
            {
                if (IsOnPlaylist() && _player.CurrentSong != null)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }

#endregion


    }
}
