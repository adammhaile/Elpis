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
using System.Windows.Input;
using PandoraSharp;
using PandoraSharpPlayer;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : UserControl
    {
        #region Delegates

        public delegate void CancelHandler(object sender);

        public delegate void StationCreatedHandler(object sender, Station station);

        #endregion

        private readonly Player _player;
        private const string initialSearchText = "Enter Artist, Track or Composer";

        public Search(Player player)
        {
            _player = player;
            InitializeComponent();

            _player.SearchResult += _player_SearchResult;
            //_player.StationCreated += _player_StationCreated;
            _player.ExceptionEvent += _player_ExceptionEvent;
        }

        public event CancelHandler Cancel;
        public event StationCreatedHandler StationCreated;

        void _player_ExceptionEvent(object sender, string code, System.Exception ex)
        {
            ShowWait(false);
        }

        private void RunSearch(string query)
        {
            if (query != string.Empty && !string.IsNullOrWhiteSpace(query))
            {
                lblNoResults.Visibility = Visibility.Collapsed;
                ShowWait(true);
                _player.StationSearchNew(query);
            }
        }

        private void ShowWait(bool state)
        {
            this.BeginDispatch(() =>
                                   {
                                       WaitScreen.Visibility = state
                                                                   ? Visibility.Visible
                                                                   : Visibility.Collapsed;
                                   });
        }

        private void _player_SearchResult(object sender, List<SearchResult> result)
        {
            this.BeginDispatch(() =>
                                   {
                                       if (result.Count == 0)
                                           lblNoResults.Visibility = Visibility.Visible;

                                       ResultItems.ItemsSource = result;
                                       ResultScroller.ScrollToHome();
                                       ShowWait(false);
                                   });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RunSearch(txtSearch.Text);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel != null)
                Cancel(this);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var result = (SearchResult) ((Grid) sender).DataContext;
            ShowWait(true);
            _player.CreateStation(result);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RunSearch(txtSearch.Text);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lblNoResults.Visibility = Visibility.Collapsed;
            txtSearch.Text = initialSearchText;
            ResultScroller.ScrollToHome();
            ShowWait(false);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ShowWait(false);
        }

        private void ClearSearchBox()
        {
            if (txtSearch.Text == initialSearchText)
                txtSearch.Text = string.Empty;
        }

        private void txtSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearSearchBox();
        }
    }
}