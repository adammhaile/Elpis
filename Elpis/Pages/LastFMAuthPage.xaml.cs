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

namespace Elpis
{
    /// <summary>
    /// Interaction logic for RestartPage.xaml
    /// </summary>
    public partial class LastFMAuthPage : UserControl
    {
        #region Delegates

        public delegate void ContinueEventHandler();
        public delegate void CancelEventHandler();

        #endregion

        public LastFMAuthPage()
        {
            InitializeComponent();
        }

        public void SetAuthURL(string url)
        {
            txtUrl.Text = url;
        }

        public event ContinueEventHandler ContinueEvent;

        private void SendContinue()
        {
            if (ContinueEvent != null)
                ContinueEvent();
        }

        public event CancelEventHandler CancelEvent;

        private void SendCancel()
        {
            if (CancelEvent != null)
                CancelEvent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SendCancel();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            SendContinue();
        }
    }
}