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
        public delegate void EditQuickMixEventHandler();
        public event EditQuickMixEventHandler EditQuickMixEvent;

        public delegate void AddVarietyEventHandler(Station station);
        public event AddVarietyEventHandler AddVarietyEvent;

        private readonly Player _player;

        private ContextMenu _stationMenu;
        private MenuItem _mnuRename;
        private MenuItem _mnuDelete;
        private MenuItem _mnuEditQuickMix;
        private MenuItem _mnuAddVariety;
        private MenuItem _mnuInfo;
        private Station _currMenuStation = null;
        private Control _currStationItem = null;

        private Pandora.SortOrder _currSort = Pandora.SortOrder.DateDesc;
        private bool _waiting;

        public StationList(Player player)
        {
            _player = player;
            _player.StationLoading += _player_StationLoading;
            _player.ExceptionEvent += _player_ExceptionEvent;
            InitializeComponent();

            _stationMenu = this.Resources["StationMenu"] as ContextMenu;
            _mnuRename = _stationMenu.Items[0] as MenuItem; //mnuRename
            _mnuDelete = _stationMenu.Items[1] as MenuItem; //mnuDelete
            _mnuEditQuickMix = _stationMenu.Items[2] as MenuItem; //mnuEditQuickMix
            _mnuAddVariety = _stationMenu.Items[3] as MenuItem; //mnuAddVariety
            _mnuInfo = _stationMenu.Items[4] as MenuItem; //mnuInfo
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
        }

        private void StationItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var station = (Station) ((Grid) sender).DataContext;

            _player.PlayStation(station);
        }

        private void DoRename()
        {
            if (_currMenuStation == null || _currStationItem == null) return;

            string name = _currStationItem.FindChildByName<TextBox>("txtRename").Text;

            _player.StationRename(_currMenuStation, name);

            var btnSaveRename = _currStationItem.FindChildByName<Button>("btnSaveRename");
            var txtStationName = _currStationItem.FindChildByName<TextBlock>("txtStationName");
            var txtRename = _currStationItem.FindChildByName<TextBox>("txtRename");

            txtStationName.Text = name;
            txtStationName.Visibility = Visibility.Visible;
            txtRename.Visibility = Visibility.Hidden;
            btnSaveRename.Visibility = Visibility.Hidden;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DoRename();
        }

        private void btnSaveRename_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            DoRename();
        }

        private void ShowMenu(object sender)
        {
            if (_currMenuStation != null)
            {
                _mnuRename.Visibility = _currMenuStation.IsQuickMix ? Visibility.Collapsed : Visibility.Visible;
                _mnuDelete.Visibility = _currMenuStation.IsQuickMix ? Visibility.Collapsed : Visibility.Visible;
                _mnuAddVariety.Visibility = _currMenuStation.IsQuickMix ? Visibility.Collapsed : Visibility.Visible;
                _mnuEditQuickMix.Visibility = _currMenuStation.IsQuickMix ? Visibility.Visible : Visibility.Collapsed;
            }

            _stationMenu.PlacementTarget = sender as UIElement;
            _stationMenu.IsOpen = true;
        }

        private Control GetStationItem(object sender)
        {
            return (Control)(((ImageButton)sender).FindParentByName<ContentControl>("StationItem"));
        }

        private Station GetItemStation(object sender)
        {
            return GetStationItem(sender).DataContext as Station;
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            _currMenuStation = GetItemStation(sender);
            _currStationItem = GetStationItem(sender);

            ShowMenu(sender);
        }

        private void StationMenu_Closed(object sender, RoutedEventArgs e)
        {
            //_currMenuStation = null;
            //_currStationItem = null;
        }

        private void mnuRename_Click(object sender, RoutedEventArgs e)
        {
            if (_currStationItem == null) return;

            var textStation = _currStationItem.FindChildByName<TextBlock>("txtStationName");
            var textBox = _currStationItem.FindChildByName<TextBox>("txtRename");
            var saverename = _currStationItem.FindChildByName<Button>("btnSaveRename");

            textBox.Text = textStation.Text;
            textStation.Visibility = Visibility.Hidden;
            textBox.Visibility = Visibility.Visible;
            saverename.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuStation != null)
            {
                _waiting = true;
                ShowWait(true);
                _player.StationDelete(_currMenuStation);
            }
        }

        private void mnuEditQuickMix_Click(object sender, RoutedEventArgs e)
        {
            if(EditQuickMixEvent != null)
                EditQuickMixEvent();
        }

        private void mnuInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuStation != null && _currMenuStation.InfoUrl.StartsWith("http"))
                Process.Start(_currMenuStation.InfoUrl);
        }

        private void mnuAddVariety_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuStation != null)
            {
                if (AddVarietyEvent != null)
                    AddVarietyEvent(_currMenuStation);
            }
        }
    }
}