﻿/*
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

using System.Windows;
using System.Windows.Controls;
using PandoraSharpPlayer;
using System;
using Util;

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

        public Settings(Player player, Config config)
        {
            InitializeComponent();

            _config = config;
            _player = player;
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
            chkGlobalMediaKeys.IsChecked = _config.Fields.Elpis_GlobalMediaKeys;
            chkTrayMinimize.IsChecked = _config.Fields.Elpis_MinimizeToTray;
            chkShowNotify.IsChecked = _config.Fields.Elpis_ShowTrayNotifications;
            chkPauseOnLock.IsChecked = _config.Fields.Elpis_PauseOnLock;

            _config.Fields.Pandora_AudioFormat = _player.AudioFormat;

            _config.Fields.Pandora_StationSortOrder = _config.Fields.Pandora_StationSortOrder;

            txtProxyAddress.Text = _config.Fields.Proxy_Address;
            txtProxyPort.Text = _config.Fields.Proxy_Port.ToString();
            txtProxyUser.Text = _config.Fields.Proxy_User;
            txtProxyPassword.Password = _config.Fields.Proxy_Password;

            chkEnableScrobbler.IsChecked = _config.Fields.LastFM_Scrobble;

            _config.SaveConfig();

            UpdateLastFMControlState();
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
            _config.Fields.Elpis_GlobalMediaKeys = (bool) chkGlobalMediaKeys.IsChecked;
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

            _config.SaveConfig();
        }

        private bool NeedsRestart()
        {
            bool restart = 
                txtProxyAddress.Text != _config.Fields.Proxy_Address ||
                txtProxyPort.Text != _config.Fields.Proxy_Port.ToString() ||
                txtProxyUser.Text != _config.Fields.Proxy_User ||
                txtProxyPassword.Password != _config.Fields.Proxy_Password;

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
    }
}