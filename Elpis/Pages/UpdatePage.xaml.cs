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
using Elpis.UpdateSystem;
using System.IO;
using Util;
using System;
using System.Diagnostics;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl
    {
        #region Delegates

        public delegate void UpdateSelectionEventHandler(bool status);

        #endregion

        private UpdateCheck _update;

        public UpdatePage(UpdateCheck update)
        {
            InitializeComponent();

            _update = update;

            lblCurrVer.Content = _update.CurrentVersion.ToString();
            lblNewVer.Content = _update.NewVersion.ToString();
            txtReleaseNotes.Text = _update.ReleaseNotes;

            _update.DownloadProgress += _update_DownloadProgress;
            _update.DownloadComplete += _update_DownloadComplete;
        }

        public event UpdateSelectionEventHandler UpdateSelectionEvent;

        public void DownloadUpdate()
        {
            string downloadDir = Path.Combine(Config.ElpisAppData, "Updates");
            if (!Directory.Exists(downloadDir))
            {
                try
                {
                    Directory.CreateDirectory(downloadDir);
                }
                catch(Exception ex)
                {
                    Log.O("Trouble creating update directory! " + ex.ToString());
                    return;
                }
            }

            string downloadFile = Path.Combine(downloadDir, "ElpisUpdate.exe");
            if (File.Exists(downloadFile))
                File.Delete(downloadFile);

            _update.DownloadUpdateAsync(downloadFile);
        }

        void _update_DownloadComplete(bool error, Exception ex)
        {
            this.BeginDispatch(() =>
            {
                if (error)
                {
                    Log.O("Error Downloading Update!");
                    lblDownloadStatus.Text = "Error downloading update. Please try again later.";
                    btnUpdate.Visibility = System.Windows.Visibility.Hidden;
                    btnLater.Content = "Close";
                }
                else
                {
                    Process.Start(_update.UpdatePath);
                    SendUpdateSelection(true);
                }
            });
        }

        void _update_DownloadProgress(int prog)
        {
            this.BeginDispatch(() =>
            {
                lblProgress.Content = prog.ToString() + "%";
                progDownload.Value = prog;
            });
        }

        private void SendUpdateSelection(bool status)
        {
            if (UpdateSelectionEvent != null)
                UpdateSelectionEvent(status);
        }

        private void btnLater_Click(object sender, RoutedEventArgs e)
        {
            SendUpdateSelection(false);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //SendUpdateSelection(true);
            gridReleaseNotes.Visibility = System.Windows.Visibility.Hidden;
            gridDownload.Visibility = System.Windows.Visibility.Visible;
            DownloadUpdate();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            gridReleaseNotes.Visibility = System.Windows.Visibility.Visible;
            gridDownload.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}