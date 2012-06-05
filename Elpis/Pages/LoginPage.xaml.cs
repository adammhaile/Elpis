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
using System.Windows.Input;
using PandoraSharpPlayer;
using Util;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        #region Delegates

        public delegate void ConnectingEventHandler();

        #endregion

        private readonly Config _config;
        private readonly Player _player;
        private ErrorCodes _error = ErrorCodes.SUCCESS;
        private bool _loginFailed;

        private const string _initEmail = "enter email address";
        private const string _initPass = "enter password";

        public bool LoginFailed { get { return _loginFailed; } set { _loginFailed = value; } }
        public LoginPage(Player player, Config config)
        {
            _config = config;

            _player = player;
            _player.ConnectionEvent += _player_ConnectionEvent;

            InitializeComponent();
        }

        public event ConnectingEventHandler ConnectingEvent;

        private void ShowError()
        {
            this.BeginDispatch(() =>
                                   {
                                       lblError.Text = Errors.GetErrorMessage(_error);
                                       //WaitScreen.Visibility = Visibility.Hidden;
                                   });
        }

        private void _player_ConnectionEvent(object sender, bool state, ErrorCodes code)
        {
            if (!state)
            {
                _loginFailed = true;
                Log.O("Connection Error: {0} - {1}", code.ToString(), Errors.GetErrorMessage(code));

                _error = code;
                ShowError();
            }
            else
            {
                this.BeginDispatch(() =>
                                       {
                                           _config.Fields.Login_Email = _player.Email;
                                           _config.Fields.Login_Password = _player.Password;

                                           //In case AudioFormat was changed because user does not have subscription
                                           _config.Fields.Pandora_AudioFormat = _player.AudioFormat;

                                           _config.SaveConfig();
                                       });
            }
        }

        public void Login()
        {
            _loginFailed = false;
            //WaitScreen.Visibility = Visibility.Visible;
            _player.Connect(txtEmail.Text, txtPassword.Password);
            if (ConnectingEvent != null)
                ConnectingEvent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void txtEmail_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HidePasswordMask();
                txtPassword.Focus();
                txtPassword.SelectAll();
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtEmail.Text = _config.Fields.Login_Email;
            if (txtEmail.Text == string.Empty)
            {
                txtEmail.Foreground = this.Resources["ShadeMediumBrush"] as System.Windows.Media.Brush;
                txtEmail.Text = _initEmail;
            }

            txtPassword.Password = _config.Fields.Login_Password;
            if (txtPassword.Password == string.Empty)
            {
                txtPasswordMask.Text = _initPass;
                txtPasswordMask.Visibility = Visibility.Visible;
            }
            else
            {
                txtPasswordMask.Visibility = Visibility.Hidden;
            }

            lblError.Text = string.Empty;


            if (!_loginFailed && _config.Fields.Login_AutoLogin &&
                (!string.IsNullOrEmpty(_config.Fields.Login_Email)) &&
                (!string.IsNullOrEmpty(_config.Fields.Login_Password)))
            {
                Login();
            }

            if (_loginFailed)
                ShowError();

            txtEmail.Focus();
            txtEmail.SelectAll();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.BeginDispatch(() => WaitScreen.Visibility = Visibility.Hidden);
        }

        private void ClearEmail()
        {
            if (txtEmail.Text == _initEmail)
            {
                txtEmail.Foreground = this.Resources["MainFontBrush"] as System.Windows.Media.Brush;
                txtEmail.Text = string.Empty;
            }
        }

        private void txtEmail_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearEmail();
        }

        private void txtEmail_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ClearEmail();
        }

        private void HidePasswordMask()
        {
            if (txtPasswordMask.Visibility == Visibility.Visible) txtPasswordMask.Visibility = Visibility.Hidden;
            txtPassword.Focus();
        }
        private void txtPasswordMask_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HidePasswordMask();
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            HidePasswordMask();
        }

        private void Register_Click(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }
}