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
        }

        public event UpdateSelectionEventHandler UpdateSelectionEvent;

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
            SendUpdateSelection(true);
        }
    }
}