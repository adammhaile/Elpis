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

using System.Windows;
using System.Windows.Controls;
using PandoraSharpPlayer;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        #region Delegates

        public delegate void CloseEvent();

        public delegate void LogoutEvent();

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

        public event LogoutEvent Logout;

        private void SaveConfig()
        {
            //_config.Fields = (ConfigFields)DataContext;

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
            _config.Fields.Misc_ForceSSL = (bool)chkForceSSL.IsChecked;

            _player.AudioFormat = _config.Fields.Pandora_AudioFormat;
            //In case MP3-HiFi was rejected
            _config.Fields.Pandora_AudioFormat = _player.AudioFormat;

            _player.SetStationSortOrder(_config.Fields.Pandora_StationSortOrder);
            _config.Fields.Pandora_StationSortOrder = _player.StationSortOrder.ToString();

            _player.ForceSSL = _config.Fields.Misc_ForceSSL;

            _config.SaveConfig();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            if (Close != null)
                Close();
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
            DataContext = _config.Fields;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Close != null)
                Close();
        }
    }
}