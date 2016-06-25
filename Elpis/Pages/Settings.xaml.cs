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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Elpis.Hotkeys;
using PandoraSharpPlayer;
using System;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.ComponentModel;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        #region Delegates

        public delegate void CloseEvent();

        public delegate void RestartEvent();

        public delegate void LogoutEvent();

        public delegate void LastFMAuthRequestEvent();

        public delegate void LasFMDeAuthRequestEvent();

        #endregion

        private readonly Config _config;

        private readonly Player _player;
        private readonly HotKeyHost _keyHost;

        public Settings(Player player, Config config, HotKeyHost keyHost)
        {
            InitializeComponent();

            _config = config;
            _player = player;
            _keyHost = keyHost;
            HotKeyItems.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("HotKeys") {Source = _keyHost, NotifyOnSourceUpdated=true, Mode=BindingMode.OneWay });
        }

        public event CloseEvent Close;

        public event RestartEvent Restart;

        public event LogoutEvent Logout;

        public event LastFMAuthRequestEvent LastFMAuthRequest;

        public event LasFMDeAuthRequestEvent LasFMDeAuthRequest;

        private void LoadConfig()
        {
            chkAutoLogin.IsChecked = _config.Fields.Login_AutoLogin;
            cmbAudioFormat.SelectedValue = _config.Fields.Pandora_AudioFormat;
            cmbStationSort.SelectedValue = _config.Fields.Pandora_StationSortOrder;

            chkAutoPlay.IsChecked = _config.Fields.Pandora_AutoPlay;
            chkCheckUpdates.IsChecked = _config.Fields.Elpis_CheckUpdates;
            chkTrayMinimize.IsChecked = _config.Fields.Elpis_MinimizeToTray;
            chkShowNotify.IsChecked = _config.Fields.Elpis_ShowTrayNotifications;
            chkPauseOnLock.IsChecked = _config.Fields.Elpis_PauseOnLock;
            chkCheckBetaUpdates.IsChecked = _config.Fields.Elpis_CheckBetaUpdates;
            chkRemoteControlEnabled.IsChecked = _config.Fields.Elpis_RemoteControlEnabled;

            _config.Fields.Pandora_AudioFormat = _player.AudioFormat;

            _config.Fields.Pandora_StationSortOrder = _config.Fields.Pandora_StationSortOrder;

            txtProxyAddress.Text = _config.Fields.Proxy_Address;
            txtProxyPort.Text = _config.Fields.Proxy_Port.ToString();
            txtProxyUser.Text = _config.Fields.Proxy_User;
            txtProxyPassword.Password = _config.Fields.Proxy_Password;

            chkEnableScrobbler.IsChecked = _config.Fields.LastFM_Scrobble;

            txtIPAddress.ItemsSource = getLocalIPAddresses();

            // Build list of all output devices
            cmbOutputDevice.Items.Clear();
            foreach (string device in _player.GetOutputDevices())
                cmbOutputDevice.Items.Add(device);

            // Get current output device
            cmbOutputDevice.SelectedValue = _player.OutputDevice;

            _config.SaveConfig();

            UpdateLastFMControlState();
        }

        private List<string> getLocalIPAddresses()
        {
            List<string> ips = new List<string>();
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress ip in host.AddressList) {
                        if (!(ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6Teredo)) {
                            ips.Add(ip.ToString());
                        }
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("There was a socket error attempting to get local ips: " + e.ToString());
                }
            }
            return ips;
        }

        private void SaveConfig()
        {
            _config.Fields.Login_AutoLogin = (bool) chkAutoLogin.IsChecked;
            _config.Fields.Pandora_AudioFormat = (string) cmbAudioFormat.SelectedValue;
            _config.Fields.Pandora_StationSortOrder = (string) cmbStationSort.SelectedValue;
            if (!_config.Fields.Pandora_AutoPlay &&
                (bool) chkAutoPlay.IsChecked && _player.CurrentStation != null)
                _config.Fields.Pandora_LastStationID = _player.CurrentStation.ID;
            _config.Fields.Pandora_AutoPlay = (bool) chkAutoPlay.IsChecked;
            _config.Fields.Elpis_CheckUpdates = (bool) chkCheckUpdates.IsChecked;
            _config.Fields.Elpis_CheckBetaUpdates = (bool)chkCheckBetaUpdates.IsChecked;
            _config.Fields.Elpis_RemoteControlEnabled = (bool)chkRemoteControlEnabled.IsChecked;
            _config.Fields.Elpis_MinimizeToTray = (bool) chkTrayMinimize.IsChecked;
            _config.Fields.Elpis_ShowTrayNotifications = (bool) chkShowNotify.IsChecked;
            _player.PauseOnLock = _config.Fields.Elpis_PauseOnLock = (bool)chkPauseOnLock.IsChecked;

            _player.AudioFormat = _config.Fields.Pandora_AudioFormat;
            //In case MP3-HiFi was rejected
            _config.Fields.Pandora_AudioFormat = _player.AudioFormat;

            _player.SetStationSortOrder(_config.Fields.Pandora_StationSortOrder);
            _config.Fields.Pandora_StationSortOrder = _player.StationSortOrder.ToString();

            _config.Fields.Proxy_Address = txtProxyAddress.Text;
            int port = _config.Fields.Proxy_Port;
            Int32.TryParse(txtProxyPort.Text, out port);
            _config.Fields.Proxy_Port = port;
            _config.Fields.Proxy_User = txtProxyUser.Text;
            _config.Fields.Proxy_Password = txtProxyPassword.Password;

            _config.Fields.LastFM_Scrobble = (bool)chkEnableScrobbler.IsChecked;
            Dictionary<int, HotkeyConfig> keys = new Dictionary<int, HotkeyConfig>();
            foreach (KeyValuePair<int,HotKey> pair in _keyHost.HotKeys)
            {
                keys.Add(pair.Key,new HotkeyConfig(pair.Value));
            }
            _config.Fields.Elpis_HotKeys = keys;

            if (!_config.Fields.System_OutputDevice.Equals((string)cmbOutputDevice.SelectedValue))
            {
                _config.Fields.System_OutputDevice = (string)cmbOutputDevice.SelectedValue;
                _player.OutputDevice = (string)cmbOutputDevice.SelectedValue;
            }

            _config.SaveConfig();
        }

        private bool NeedsRestart()
        {
            bool restart =
                txtProxyAddress.Text != _config.Fields.Proxy_Address ||
                txtProxyPort.Text != _config.Fields.Proxy_Port.ToString() ||
                txtProxyUser.Text != _config.Fields.Proxy_User ||
                txtProxyPassword.Password != _config.Fields.Proxy_Password ||
                chkRemoteControlEnabled.IsChecked != _config.Fields.Elpis_RemoteControlEnabled;

            return restart;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool restart = NeedsRestart();
            SaveConfig();

            if (restart)
            {
                if (Restart != null)
                    Restart();
            }
            else
            {
                if (Close != null)
                    Close();
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            _config.Fields.Login_Email = string.Empty;
            _config.Fields.Login_Password = string.Empty;

            SaveConfig();

            if (Logout != null)
                Logout();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            btnLogout.IsEnabled = _player.LoggedIn;
            LoadConfig();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Close != null)
                Close();
        }

        private void txtProxyPort_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                Convert.ToInt32(e.Text);
                string text = txtProxyPort.Text + e.Text;
                int output = Convert.ToInt32(text);
                if (output < 0 || output > 65535)
                    e.Handled = true;
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void ShowLastFMAuthButton(bool state)
        {
            btnLastFMAuth.Visibility = state ? Visibility.Visible : Visibility.Hidden;
            btnLastFMDisable.Visibility = state ? Visibility.Hidden : Visibility.Visible;
        }

        private void UpdateLastFMControlState()
        {
            bool state = (bool)chkEnableScrobbler.IsChecked;

            ShowLastFMAuthButton(_config.Fields.LastFM_SessionKey == string.Empty);
            btnLastFMAuth.IsEnabled = state || _config.Fields.LastFM_SessionKey != string.Empty;
        }

        private void chkEnableScrobbler_Click(object sender, RoutedEventArgs e)
        {
            UpdateLastFMControlState();
        }

        private void btnLastFMAuth_Click(object sender, RoutedEventArgs e)
        {
            if (LastFMAuthRequest != null)
                LastFMAuthRequest();
        }

        private void btnLastFMDisable_Click(object sender, RoutedEventArgs e)
        {
            if (LasFMDeAuthRequest != null)
                LasFMDeAuthRequest();

            chkEnableScrobbler.IsChecked = false;
            UpdateLastFMControlState();
        }

        private void btnAddHotKey_Click(object sender, RoutedEventArgs e)
        {
            _keyHost.AddHotKey(new HotKey(PlayerCommands.PlayPause,Key.None,ModifierKeys.None));
        }

        //private void btnDelHotkey_Click(object sender, RoutedEventArgs e)
        //{
        //    KeyValuePair<int, HotKey> pair = (KeyValuePair<int, HotKey>) ((FrameworkElement)sender).DataContext;
        //    _keyHost.RemoveHotKey(pair.Value);
        //}

        private void RemoveHotkey_MouseDown(object sender, MouseButtonEventArgs e)
        {
            KeyValuePair<int, HotKey> pair = (KeyValuePair<int, HotKey>)((FrameworkElement)sender).DataContext;
            _keyHost.RemoveHotKey(pair.Value);
        }

        private void txtIPAddress_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Clipboard.SetText(txtIPAddress.SelectedItem.ToString());
            }
        }
    }

    public class HotKeyBox : TextBox
    {
        public HotKeyBox() : base() { }

        static HotKeyBox()
        {
            TextProperty.OverrideMetadata(typeof(HotKeyBox),
                                                  new FrameworkPropertyMetadata()
                                                      {
                                                          BindsTwoWayByDefault = false,
                                                          Journal = true,
                                                          DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                                      });
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            KeyValuePair<int, HotKey> pair = (KeyValuePair<int, HotKey>) DataContext;
            HotKey h = pair.Value;
            switch (e.Key)
            {
                case Key.LeftShift:
                case Key.LeftAlt:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.RightAlt:
                case Key.RightShift:
                    break;
                default:
                    try
                    {
                        h.SetKeyCombo(e.Key, Keyboard.Modifiers);
                        e.Handled = true;
                        GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                        //HACK: This is a cheap-and-nasty way to shift focus from the textbox
                        IsEnabled = false;
                        IsEnabled = true;
                    }
                    catch (HotKeyNotSupportedException es)
                    {
                        //Log.O(es.Message);
                    }
                    break;
            }
        }
    }
}