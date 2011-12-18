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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Elpis.Controls;
using PandoraSharp;
using PandoraSharpPlayer;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for StationList.xaml
    /// </summary>
    public partial class StationList : UserControl
    {
        private readonly Player _player;

        private ContentControl _currMenu;

        private Pandora.SortOrder _currSort = Pandora.SortOrder.DateDesc;
        private bool _waiting;

        public StationList(Player player)
        {
            _player = player;
            _player.StationLoading += _player_StationLoading;
            _player.ExceptionEvent += _player_ExceptionEvent;
            InitializeComponent();
        }

        public List<Station> Stations
        {
            get { return (List<Station>) StationItems.ItemsSource; }
            set
            {
                this.BeginDispatch(() =>
                                       {
                                           lblNoStations.Visibility = (value.Count > 0) ? Visibility.Hidden : Visibility.Visible;
                                           StationItems.ItemsSource = value;
                                           _currSort = _player.StationSortOrder;
                                           scrollMain.ScrollToHome();
                                           if (_waiting)
                                           {
                                               ShowWait(false);
                                               _waiting = false;
                                           }
                                       });
            }
        }

        public void SetStationsRefreshing()
        {
            this.BeginDispatch(() =>
                                   {
                                       _waiting = true;
                                       ShowWait(true);
                                   });
        }

        public void ShowWait(bool state)
        {
            this.BeginDispatch(() =>
                                   {
                                       WaitScreen.Visibility = state
                                                                   ? Visibility.Visible
                                                                   : Visibility.Collapsed;
                                   });
        }

        private void _player_StationLoading(object sender, Station station)
        {
            this.BeginDispatch(() =>
                                   {
                                       if(this.IsLoaded)
                                            ShowWait(true);

                                       ToggleMenu();
                                   });
        }

        void _player_ExceptionEvent(object sender, string code, System.Exception ex)
        {
            ShowWait(false);
        }

        private void StationList_Loaded(object sender, RoutedEventArgs e)
        {
            ShowWait(false);
            if (_player.StationSortOrder != _currSort)
                _player.RefreshStations();
        }

        private void StationList_Unloaded(object sender, RoutedEventArgs e)
        {
            ShowWait(false);
            ToggleMenu();
        }

        private void StationItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var station = (Station) ((Grid) sender).DataContext;

            _player.PlayStation(station);
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var btnRename = ((ImageButton) sender);
            var menugrid = ((Grid) ((Grid) btnRename.Parent).Parent);

            var textStation = menugrid.FindChildByName<TextBlock>("txtStationNameMenu");
            var textBox = menugrid.FindChildByName<TextBox>("txtRename");
            var saverename = menugrid.FindChildByName<Button>("btnSaveRename");

            textBox.Text = textStation.Text;
            textStation.Visibility = Visibility.Hidden;
            textBox.Visibility = Visibility.Visible;
            saverename.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var station = (Station) ((ImageButton) sender).Tag;
            _waiting = true;
            ToggleMenu();
            ShowWait(true);
            _player.StationDelete(station);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string name = ((TextBox) sender).Text;
                var station = (Station) (((TextBox) sender).FindParentByName<ContentControl>("StationItem")).DataContext;
                _player.StationRename(station, name);

                var stationMenu = ((TextBox) sender).FindParentByName<ContentControl>("StationMenu");
                var textStationName = stationMenu.FindSiblingByName<TextBlock>("txtStationName");
                var textStationNameMenu = ((TextBox) sender).FindSiblingByName<TextBlock>("txtStationNameMenu");
                var textBox = (TextBox) sender;

                textStationName.Text = textStationNameMenu.Text = name;
                textStationNameMenu.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Hidden;

                ToggleMenu();
            }
        }

        private void btnSaveRename_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            string name = (((Button) sender).FindSiblingByName<TextBox>("txtRename")).Text;

            var station = (Station) (((Button) sender).FindParentByName<ContentControl>("StationItem")).DataContext;
            _player.StationRename(station, name);

            var stationMenu = ((Button) sender).FindParentByName<ContentControl>("StationMenu");
            var textStationName = stationMenu.FindSiblingByName<TextBlock>("txtStationName");
            var textStationNameMenu = ((Button) sender).FindSiblingByName<TextBlock>("txtStationNameMenu");
            var self = (Button) sender;

            textStationName.Text = textStationNameMenu.Text = name;
            textStationNameMenu.Visibility = Visibility.Visible;
            self.Visibility = Visibility.Hidden;

            ToggleMenu();
        }

        private void MenuGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                var station = (Station) ((Grid) sender).Tag;
                var textStation = ((Grid) sender).FindChildByName<TextBlock>("txtStationNameMenu");
                var textBox = ((Grid) sender).FindChildByName<TextBox>("txtRename");
                var saverename = ((Grid) sender).FindChildByName<Button>("btnSaveRename");
                var rename = ((Grid) sender).FindChildByName<Button>("btnRename");
                var delete = ((Grid) sender).FindChildByName<Button>("btnDelete");

                if (station.IsQuickMix || !station.IsCreator)
                {
                    rename.Visibility = Visibility.Collapsed;
                    delete.Visibility = Visibility.Collapsed;
                }
                else
                {
                    rename.Visibility = Visibility.Visible;
                    delete.Visibility = Visibility.Visible;
                }
                textBox.Text = textStation.Text;
                textStation.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Hidden;
                saverename.Visibility = Visibility.Hidden;
            }
        }

        private void ToggleMenu()
        {
            if (_currMenu == null) return;

            if (_currMenu.Visibility != Visibility.Hidden)
            {
                _currMenu.Visibility = Visibility.Hidden;
                _currMenu = null;
            }
            else
                _currMenu.Visibility = Visibility.Visible;
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            //var grid = ((ImageButton) sender).FindSiblingByName<Grid>("CenterGrid");
            //var btnMenuClose = ((ImageButton) sender).FindSiblingByName<ImageButton>("btnMenuClose");

            var newMenu = ((ImageButton)sender).FindSiblingByName<ContentControl>("StationMenu");

            if (_currMenu != null && _currMenu != newMenu)
                ToggleMenu();

            if (newMenu != null)
            {
                _currMenu = newMenu;
                
                ToggleMenu();
            }
        }

        private void btnMenuClose_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start((string) ((ImageButton) sender).Tag);
        }
    }
}