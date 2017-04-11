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
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using BassPlayer;
using Elpis.Controls;
using PandoraSharp;
using PandoraSharpPlayer;
using Util;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for PlaylistPage.xaml
    /// </summary>
    public partial class PlaylistPage : UserControl
    {
        private readonly object _currSongLock = new object();
        private readonly Dictionary<Song, ImageButton[]> _feedbackMap;
        private readonly Player _player;
        private Song _currSong;
        private Song _currMenuSong;

        private ContextMenu _songMenu;
        private MenuItem _purchaseMenu;
        private MenuItem _purchaseAmazonAlbum;
        private MenuItem _purchaseAmazonTrack;

        public PlaylistPage(Player player)
        {
            _player = player;
            _player.SongStarted += _player_SongStarted;
            _player.PlaybackStateChanged += _player_PlaybackStateChanged;
            _player.PlayedSongAdded += _player_PlayedSongAdded;
            _player.PlayedSongRemoved += _player_PlayedSongRemoved;
            _player.LoadingNextSong += _player_LoadingNextSong;
            _player.FeedbackUpdateEvent += _player_FeedbackUpdateEvent;
            _player.PlaybackProgress += _player_PlaybackProgress;
            _player.StationLoaded += _player_StationLoaded;
            _player.StationLoading += _player_StationLoading;
            _player.ExceptionEvent += _player_ExceptionEvent;
            _player.LogoutEvent += _player_LogoutEvent;
            InitializeComponent();

            _feedbackMap = new Dictionary<Song, ImageButton[]>();

            _songMenu = this.Resources["SongMenu"] as ContextMenu;
            //This would need to be changed if the menu order is ever changed
            _purchaseMenu = _songMenu.Items[0] as MenuItem;
            _purchaseAmazonAlbum = _purchaseMenu.Items[0] as MenuItem;
            _purchaseAmazonTrack = _purchaseMenu.Items[1] as MenuItem;
        }

        private void ShowWait(bool state)
        {
            this.BeginDispatch(() => StationWaitScreen.Visibility = state ? Visibility.Visible : Visibility.Hidden);
        }

        void _player_LogoutEvent(object sender)
        {
            lstOldSongs.Items.Clear();
        }

        void _player_ExceptionEvent(object sender, ErrorCodes code, Exception ex)
        {
            ShowWait(false);
        }

        private void _player_PlaybackStateChanged(object sender, BassAudioEngine.PlayState oldState,
                                                  BassAudioEngine.PlayState newState)
        {
            lock (_currSongLock)
            {
                if (newState == BassAudioEngine.PlayState.Playing && _currSong != null)
                    this.BeginDispatch(() => SetSong(_currSong));
            }
        }

        private void _player_StationLoaded(object sender, Station station)
        {
            this.BeginDispatch(() =>
                                   {
                                       txtStationName.Text = station.Name;
                                       ShowWait(false);
                                   });
        }

        private void _player_StationLoading(object sender, Station station)
        {
            this.BeginDispatch(() =>
                                   {
                                       if (IsLoaded)
                                       {
                                           ShowWait(true);
                                       }
                                   });
        }

        private void _player_PlaybackProgress(object sender, BassAudioEngine.Progress prog)
        {
            UpdateProgress(prog);
        }

        private void UpdateProgress(BassAudioEngine.Progress prog)
        {
            this.BeginDispatch(() =>
                                   {
                                       lblCurrTime.Content = prog.ElapsedTime.ToString(@"mm\:ss");
                                       lblRemainTime.Content = prog.RemainingTime.ToString(@"mm\:ss");
                                       progPlayTime.Value = prog.Percent;
                                   });
        }

        private void _player_FeedbackUpdateEvent(object sender, Song song, bool success)
        {
            this.BeginDispatch(() =>
                                   {
                                       if (_feedbackMap.ContainsKey(song))
                                       {
                                           //bit of a hack, but avoids putting INotify in lower level classes or making wrappers
                                           foreach (ImageButton button in _feedbackMap[song])
                                           {
                                               var spinner = button.FindParent<ContentSpinner>();
                                               BindingExpression bind =
                                                   button.GetBindingExpression(ImageButton.IsActiveProperty);
                                               if (bind != null) bind.UpdateTarget();
                                               spinner.StopAnimation();
                                           }
                                           _feedbackMap.Remove(song);

                                           if (song.Banned && song == _player.CurrentSong) _player.Next();
                                       }
                                   });
        }

        private void _player_LoadingNextSong(object sender)
        {
            UpdateProgress(new BassAudioEngine.Progress {ElapsedTime = new TimeSpan(0), TotalTime = new TimeSpan(0)});
            this.BeginDispatch(() =>
                                   {
                                       CurrentSong.Visibility = Visibility.Hidden;
                                       WaitScreen.Visibility = Visibility.Visible;
                                   });
        }

        private void _player_PlayedSongRemoved(object sender, Song song)
        {
            this.BeginDispatch(() => PlayedSongRemove(song));
        }

        private void PlayedSongRemove(Song song)
        {
            ContentControl result = null;
            foreach (ContentControl sc in lstOldSongs.Items)
            {
                if (song == sc.Content)
                {
                    result = sc;
                    break;
                }
            }

            if (result != null)
            {
                bool last = lstOldSongs.Items.IndexOf(result) == (lstOldSongs.Items.Count - 1);
                Dispatcher.Invoke(AnimateListRemove(result, last));
            }
        }

        private Action AnimateListRemove(ContentControl item, bool last)
        {
            return () =>
                       {
                           Storyboard remSB;
                           if (last)
                           {
                               remSB = ((Storyboard) Resources["ListBoxRemoveLast"]).Clone();
                           }
                           else
                           {
                               remSB = ((Storyboard) Resources["ListBoxRemove"]).Clone();
                               ((DoubleAnimation) remSB.Children[1]).From = (item).ActualHeight;
                           }

                           remSB.Completed += ((o, e) => lstOldSongs.Items.Remove(item));
                           remSB.Begin(item);
                       };
        }

        private void _player_PlayedSongAdded(object sender, Song song)
        {
            this.BeginDispatch(() => PlayedSongAdd(song));
        }

        private void PlayedSongAdd(Song song)
        {
            var songControl = new ContentControl();
            RoutedEventHandler loadEvent = AnimateListAdd(lstOldSongs.Items.Count == 0);
            songControl.Loaded += loadEvent;
            songControl.Tag = loadEvent;
            songControl.ContentTemplate = (DataTemplate) Resources["SongTemplate"];
            songControl.Content = song;

            lstOldSongs.Items.Insert(0, songControl);
        }

        private RoutedEventHandler AnimateListAdd(bool first)
        {
            return (o1, e1) =>
                       {
                           Storyboard addSB;
                           if (first)
                           {
                               addSB = ((Storyboard) Resources["ListBoxAddFirst"]).Clone();
                           }
                           else
                           {
                               addSB = ((Storyboard) Resources["ListBoxAdd"]).Clone();
                               //((DoubleAnimation) addSB.Children[0]).To = 96;//((ContentControl)o1).ActualHeight;
                           }
                           addSB.Begin((ContentControl) o1);

                           var song = (ContentControl) o1;
                           song.Loaded -= (RoutedEventHandler) song.Tag;
                       };
        }

        private void SetSong(Song song)
        {
            CurrentSong.Content = song;
            CurrentSong.Visibility = Visibility.Visible;
            WaitScreen.Visibility = Visibility.Collapsed;

            this.BeginDispatch(() =>
            {
                String[] stat = txtStationName.Text.Split('-');
                if (stat[0].Equals("Quick Mix"))
                {
                    txtStationName.Text = stat[0]+"-"+song.Station.Name;
                }
            });
        }

        private void _player_SongStarted(object sender, Song song)
        {
            lock (_currSongLock)
            {
                _currSong = song;
            }
        }

        private void RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        public void ThumbDownCurrent()
        {
            var thumbDown = CurrentSong.FindChildByName<ImageButton>("btnThumbDown");
            ThumbDownHandle(thumbDown);
        }

        private void ThumbDownHandle(ImageButton button)
        {
            var song = (Song)((button).FindParentByName<Grid>("SongItem")).DataContext;
            var spinner = button.FindParent<ContentSpinner>();
            if (_feedbackMap.ContainsKey(song)) return;

            var otherButton =
                spinner.FindSiblingByName<ContentSpinner>("SpinUp").FindChildByName<ImageButton>("btnThumbUp");
            _feedbackMap.Add(song, new[] { button, otherButton });

            if (song.Banned)
                _player.SongDeleteFeedback(song);
            else
                _player.SongThumbDown(song);

            spinner.StartAnimation();
        }

        private void btnThumbDown_Click(object sender, RoutedEventArgs e)
        {
            ThumbDownHandle((ImageButton)sender);
        }

        public void ThumbUpCurrent()
        {
            var thumbUp = CurrentSong.FindChildByName<ImageButton>("btnThumbUp");
            ThumbUpHandle(thumbUp);
        }

        private void ThumbUpHandle(ImageButton button)
        {
            var song = (Song)((button).FindParentByName<Grid>("SongItem")).DataContext;
            var spinner = button.FindParent<ContentSpinner>();
            if (_feedbackMap.ContainsKey(song)) return;

            var otherButton =
                spinner.FindSiblingByName<ContentSpinner>("SpinDown").FindChildByName<ImageButton>("btnThumbDown");
            _feedbackMap.Add(song, new[] { button, otherButton });
            if (song.Loved)
                _player.SongDeleteFeedback(song);
            else
                _player.SongThumbUp(song);

            spinner.StartAnimation();
        }


        private void btnThumbUp_Click(object sender, RoutedEventArgs e)
        {
            ThumbUpHandle((ImageButton)sender);
        }

        private Song GetItemSong(object sender)
        {
            return (Song) (((ImageButton) sender).FindParentByName<Grid>("SongItem")).DataContext;
        }

        private void ShowMenu(object sender)
        {
            _songMenu.PlacementTarget = sender as UIElement;
            bool showAmazonAlbum = (_currMenuSong.AmazonAlbumID != string.Empty);
            bool showAmazonTrack = (_currMenuSong.AmazonTrackID != string.Empty);
            bool showPurchase = (showAmazonAlbum || showAmazonTrack);

            _purchaseAmazonAlbum.Visibility = showAmazonAlbum ? Visibility.Visible : Visibility.Hidden;
            _purchaseAmazonTrack.Visibility = showAmazonTrack ? Visibility.Visible : Visibility.Hidden;

            _purchaseMenu.Visibility = showPurchase ? Visibility.Visible : Visibility.Hidden;

            _songMenu.IsOpen = true;
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            _currMenuSong = GetItemSong(sender);
            ShowMenu(sender);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ShowWait(false);
        }

        private void mnuTired_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                _player.SongTired(_currMenuSong);
                if (_currMenuSong == _player.CurrentSong)
                    _player.Next();
            }
        }

        public void TiredOfCurrentSongFromSystemTray()
        {
            _player.SongTired(_player.CurrentSong);
            _player.Next();
        }

        private void mnuBookArtist_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                _player.SongBookmarkArtist(_currMenuSong);
            }
        }

        private void mnuBookSong_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                _player.SongBookmark(_currMenuSong);
            }
        }

        private void SongMenu_Closed(object sender, RoutedEventArgs e)
        {
            _currMenuSong = null;
        }

        private void mnuCreateArtist_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                _player.CreateStationFromArtist(_currMenuSong);
            }
        }

        private void mnuCreateSong_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                _player.CreateStationFromSong(_currMenuSong);
            }
        }

        private void LaunchAmazonURL(string ID)
        {
            if (ID != string.Empty)
            {
                string url = @"http://www.amazon.com/dp/" + ID;
#if APP_RELEASE
                if (ReleaseData.AmazonTag != string.Empty)
                {
                    url += (@"/?tag=" + ReleaseData.AmazonTag);
                }
#endif

                Process.Start(url);
            }
        }

        private void mnuPurchaseAmazonAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                if (_currMenuSong.AmazonAlbumID != null)
                {
                    LaunchAmazonURL(_currMenuSong.AmazonAlbumID);
                }
                else
                {
                    if(_currMenuSong.AmazonAlbumUrl != null)
                    {
                        string url = _currMenuSong.AmazonAlbumUrl;

#if APP_RELEASE
                        if (ReleaseData.AmazonTag != string.Empty)
                        {
                            string oldTag = url.Substring(url.IndexOf("tag="));
                            url = url.Replace(oldTag, ReleaseData.AmazonTag);
                        }
#endif
                        Process.Start(url);
                    }
                }
            }
        }

        private void mnuPurchaeAmazonTrack_Click(object sender, RoutedEventArgs e)
        {
            if (_currMenuSong != null)
            {
                LaunchAmazonURL(_currMenuSong.AmazonTrackID);
            }
        }

        private void seekToPosition(object sender, MouseEventArgs e)
        {
            int percentClicked = (int)Math.Round(e.GetPosition((ProgressBar)sender).X / this.progPlayTime.ActualWidth * 100);
            //Util.Log.O("Bar percentage, numerator, denominator: {0} {1} {2}", new object[] {percentClicked, e.GetPosition((ProgressBar)sender).X, this.progPlayTime.ActualWidth});
            _player.SeekToTime(percentClicked);
        }

        private void progPlayTime_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            seekToPosition(sender, e);
        }

        private void progPlayTime_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                seekToPosition(sender, e);
            }
        }
    }
}