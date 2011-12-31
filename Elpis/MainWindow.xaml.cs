/*
 * Copyright 2012 - Adam Haile
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BassPlayer;
using Elpis.KeyboardHook;
using Elpis.UpdateSystem;
using GUI.BorderlessWindow;
using GUI.PageTransition;
using PandoraSharp;
using PandoraSharpPlayer;
using Util;
using Log = Util.Log;
using UserControl = System.Windows.Controls.UserControl;
using WinForms = System.Windows.Forms;
using System.Windows.Interop;

namespace Elpis
{
    public partial class MainWindow : Window
    {
#region Globals

        private readonly ErrorPage _errorPage;
        private readonly KeyboardListener _keyListen;
        private readonly LoadingPage _loadingPage;
        private readonly ToolStripSeparator _notifyMenu_BreakSong = new ToolStripSeparator();
        private readonly ToolStripSeparator _notifyMenu_BreakStation = new ToolStripSeparator();
        private About _aboutPage;

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
        private Player _player;
        private PlaylistPage _playlistPage;
        private UserControl _prevPage;
        private Search _searchPage;
        private Settings _settingsPage;
        private bool _showingError;
        private bool _stationLoaded;

        private bool _configError = false;

        private StationList _stationPage;
        private UpdateCheck _update;
        private UpdatePage _updatePage;

        private string _lastError = string.Empty;
        private Exception _lastException = null;
#endregion

#region Release Data Values

        private string _bassRegEmail = "";
        private string _bassRegKey = "";

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

            _keyListen = new KeyboardListener();
            _keyListen.KeyDown += _keyListen_KeyDown;
            _keyListen.KeyUp += _keyListen_KeyUp;

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

            _config = new Config();
            if (!_config.LoadConfig())
            {
                _configError = true;
            }
            else
            {
                var loc = _config.Fields.Elpis_StartupLocation;
                var size = _config.Fields.Elpis_StartupSize;

                if(loc.X != -1 && loc.Y != -1)
                {
                    this.Left = loc.X;
                    this.Top = loc.Y;
                }

                if(size.Width != 0 && size.Height != 0)
                {
                    this.Width = size.Width;
                    this.Height = size.Height;
                }
            }
        }

        public void ShowWindow()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Microsoft.Shell.NativeMethods.ShowToFront((new System.Windows.Interop.WindowInteropHelper(this)).Handle);
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

        private void SetupPageEvents()
        {
            _settingsPage.Close += RestorePrevPage;
            _aboutPage.Close += RestorePrevPage;

            _searchPage.Cancel += _searchPage_Cancel;
            _searchPage.StationCreated += _searchPage_StationCreated;

            _loginPage.ConnectingEvent += _loginPage_ConnectingEvent;
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

            mainBar.PlayPauseClick += mainBar_PlayPauseClick;
            mainBar.NextClick += mainBar_NextClick;
            mainBar.AboutClick += mainBar_AboutClick;
            mainBar.SettingsClick += mainBar_SettingsClick;
            mainBar.StationListClick += mainBar_stationPageClick;
            mainBar.CreateStationClick += mainBar_searchPageClick;
            mainBar.ErrorClicked += mainBar_ErrorClicked;

            _loginPage.Loaded += _loginPage_Loaded;
            _aboutPage.Loaded += _aboutPage_Loaded;
            _settingsPage.Loaded += _settingsPage_Loaded;
            _settingsPage.Logout += _settingsPage_Logout;
            _searchPage.Loaded += _searchPage_Loaded;
            _stationPage.Loaded += _stationPage_Loaded;
            _playlistPage.Loaded += _playlistPage_Loaded;
        }

        private void SetupPages()
        {
            _searchPage = new Search(_player);
            transitionControl.AddPage(_searchPage);

            _settingsPage = new Settings(_player, _config);
            transitionControl.AddPage(_settingsPage);

            _aboutPage = new About();
            transitionControl.AddPage(_aboutPage);

            _stationPage = new StationList(_player);
            transitionControl.AddPage(_stationPage);

            _loginPage = new LoginPage(_player, _config);
            transitionControl.AddPage(_loginPage);

            _playlistPage = new PlaylistPage(_player);
            transitionControl.AddPage(_playlistPage);
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
                _notifyMenu_BreakSong.Visible = showSongInfo;

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

            var menus = new ToolStripItem[]
                            {
                                _notifyMenu_Title,
                                _notifyMenu_Artist,
                                _notifyMenu_Album,
                                _notifyMenu_BreakSong,
                                _notifyMenu_PlayPause,
                                _notifyMenu_Next,
                                _notifyMenu_BreakStation,
                                _notifyMenu_Stations
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
                this.BeginDispatch(() => ShowError("CONFIG_LOAD_ERROR", null));
                return false;
            }

            try
            {
                SetupLogging();
            }
            catch (Exception ex)
            {
                ShowError("LOG_SETUP_ERROR", ex);
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
            }
            catch(Exception ex)
            {
                ShowError("ENGINE_INIT_ERROR", ex);
                return;
            }
            
            _player.AudioFormat = _config.Fields.Pandora_AudioFormat;
            _player.SetStationSortOrder(_config.Fields.Pandora_StationSortOrder);

            _loadingPage.UpdateStatus("Setting up cache...");
            string cachePath = Path.Combine(Config.ElpisAppData, "Cache");
            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
            _player.ImageCachePath = cachePath;

            _loadingPage.UpdateStatus("Setting up UI...");

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

            _finalComplete = true;
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
        private bool _mediaPlayDown = false;
        private bool _mediaNextDown = false;
        private bool _mediaSpaceDown = false;
        private bool _mediaReturnDown = false;
        private bool _mediaArrowNextDown = false;
        private bool _mediaArrowLove = false;
        private bool _mediaArrowBan = false;
        private bool IsOnPlaylist()
        {
            return (IsActive && transitionControl.CurrentPage == _playlistPage);
        }

        private void HandleKeyHook(Key key, bool down)
        {
            if (!_finalComplete) return;

            if ((IsActive || !IsActive && _config.Fields.Elpis_GlobalMediaKeys) && _player.IsStationLoaded)
            {
                
                switch (key)
                {
                    case Key.MediaPlayPause:
                        if (down)
                        {
                            if (!_mediaPlayDown)
                            {
                                _mediaPlayDown = true;
                                _player.PlayPause();
                            }
                        }
                        else
                        {
                            _mediaPlayDown = false;
                        }
                        break;
                    case Key.MediaNextTrack:
                        if (down)
                        {
                            if (!_mediaNextDown)
                            {
                                _mediaNextDown = true;
                                _player.Next();
                            }
                        }
                        else
                        {
                            _mediaNextDown = false;
                        }
                        break;
                    case Key.Space:
                        if (down)
                        {
                            if (!_mediaSpaceDown)
                            {
                                _mediaSpaceDown = true;
                                if (IsOnPlaylist())
                                    _player.PlayPause();
                            }
                        }
                        else
                        {
                            _mediaSpaceDown = false;
                        }
                        break;
                    case Key.Return:
                        if (down)
                        {
                            if (!_mediaReturnDown)
                            {
                                _mediaReturnDown = true;
                                if (IsOnPlaylist())
                                    _player.PlayPause();
                            }
                        }
                        else
                        {
                            _mediaReturnDown = false;
                        }
                        break;
                    case Key.Right:
                        if (down)
                        {
                            if (!_mediaArrowNextDown)
                            {
                                _mediaArrowNextDown = true;
                                if (IsOnPlaylist())
                                    _player.Next();
                            }
                        }
                        else
                        {
                            _mediaArrowNextDown = false;
                        }
                        break;
                    case Key.Up:
                        if (down)
                        {
                            if (!_mediaArrowLove)
                            {
                                _mediaArrowLove = true;
                                if (IsOnPlaylist())
                                    if (_player.CurrentSong != null)
                                        _playlistPage.ThumbUpCurrent();
                            }
                        }
                        else
                        {
                            _mediaArrowLove = false;
                        }
                        break;
                    case Key.Down:
                        if (down)
                        {
                            if (!_mediaArrowBan)
                            {
                                _mediaArrowBan = true;
                                if (IsOnPlaylist())
                                    if(_player.CurrentSong != null)
                                        _playlistPage.ThumbDownCurrent();
                            }
                        }
                        else
                        {
                            _mediaArrowBan = false;
                        }
                        break;
                }
            }
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

        private void ShowErrorPage(string code, Exception ex)
        {
            if (!_showingError)
            {
                _showingError = true;

                _prevPage = transitionControl.CurrentPage;
                ErrorCode error = Errors.GetError(code);
                _errorPage.SetError(error.Description, error.HardFail, ex);
                transitionControl.ShowPage(_errorPage);
            }
        }

        private void ShowError(string code, Exception ex, bool showLast = false)
        {
            if (transitionControl.CurrentPage != _errorPage)
            {
                if(showLast && _lastError != string.Empty)
                {
                    ShowErrorPage(_lastError, _lastException);
                }
                else if(code != string.Empty && ex != null)
                {
                    var err = Errors.GetError(code);
                    if(err.HardFail)
                    {
                        ShowErrorPage(code, ex);
                    }
                    else
                    {
                        _lastError = code;
                        _lastException = ex;
                        mainBar.ShowError(err.Description);
                    }
                }
            }
        }
#endregion

#region Event Handlers

        void mainBar_ErrorClicked()
        {
            ShowError(string.Empty, null, true);
        }

        private void _keyListen_KeyDown(object sender, RawKeyEventArgs args)
        {
            this.BeginDispatch(() => HandleKeyHook(args.Key, true));
        }

        void _keyListen_KeyUp(object sender, RawKeyEventArgs args)
        {
            this.BeginDispatch(() => HandleKeyHook(args.Key, false));
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
                _lastError = string.Empty;
                _lastException = null;
                RestorePrevPage();
                _showingError = false;
            }
        }

        private void _updatePage_UpdateSelectionEvent(bool status)
        {
            if (status && _update.DownloadUrl != string.Empty)
            {
                Process.Start(_update.DownloadUrl);
                Close();
            }
            else
            {
                transitionControl.ShowPage(_loadingPage);
            }
        }

        private void _searchPage_StationCreated(object sender, Station station)
        {
            _player.RefreshStations();
            this.BeginDispatch(() => _player.PlayStation(station));
        }

        private void _searchPage_Cancel(object sender)
        {
            this.BeginDispatch(() =>
                                   {
                                       if (_prevPage == _stationPage)
                                           ShowStationList();
                                       else
                                           transitionControl.ShowPage(_playlistPage);
                                   });
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

        private void _player_ExceptionEvent(object sender, string code, Exception ex)
        {
            ShowError(code, ex);
        }

        private void _player_LoginStatusEvent(object sender, string status)
        {
            _loadingPage.UpdateStatus(status);
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
            if (transitionControl.CurrentPage == _stationPage)
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
            transitionControl.ShowPage(_searchPage, PageTransitionType.Previous);
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

        private void _player_ConnectionEvent(object sender, bool state, string msg)
        {
            if (state)
            {
                if (_config.Fields.Pandora_AutoPlay)
                {
                    Station s = _player.GetStationFromID(_config.Fields.Pandora_LastStationID);
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
            if (_notify != null)
            {
                _notify.Dispose();
                _notify = null;
            }

            if (_config != null)
            {
                _config.Fields.Elpis_StartupLocation = new Point(this.Left, this.Top);
                _config.Fields.Elpis_StartupSize = new Size(this.Width, this.Height);
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

#endregion      
    }
}