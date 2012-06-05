using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Util;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for QuickMixPage.xaml
    /// </summary>
    public partial class QuickMixPage : UserControl
    {
        public delegate void CancelHandler();
        public delegate void CloseHandler();

        public event CancelHandler CancelEvent;
        public event CloseHandler CloseEvent;

        private PandoraSharpPlayer.Player _player = null;

        public QuickMixPage(PandoraSharpPlayer.Player player)
        {
            _player = player;
            _player.QuickMixSavedEvent += _player_QuickMixSavedEvent;
            _player.ExceptionEvent += _player_ExceptionEvent;
            InitializeComponent();
        }

        void _player_ExceptionEvent(object sender, ErrorCodes code, Exception ex)
        {
            ShowWait(false);
        }

        void _player_QuickMixSavedEvent(object sender)
        {
            ShowWait(false);
            if (CloseEvent != null)
                CloseEvent();
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null) CancelEvent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowWait(false);

            var subList = from s in _player.Stations
                          where !s.IsQuickMix
                          select s;

            StationItems.ItemsSource = subList;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ShowWait(true);
            _player.SaveQuickMix();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ShowWait(false);
        }
    }
}
