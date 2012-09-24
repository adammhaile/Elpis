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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows;

namespace Elpis.Hotkeys
{
    public class HotKeyEventArgs : EventArgs
    {
        public HotKey HotKey { get; private set; }

        public HotKeyEventArgs(HotKey hotKey)
        {
            HotKey = hotKey;
        }
    }

    public class HotKeyAlreadyRegisteredException : Exception
    {
        public HotKey HotKey { get; private set; }

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey) : base(message)
        {
            HotKey = hotKey;
        }

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey, Exception inner) : base(message, inner)
        {
            HotKey = hotKey;
        }
    }

    public class HotKey : INotifyPropertyChanged, IEquatable<HotKey>
    {
        public HotKey(Key key, ModifierKeys modifiers, bool activeOnly, bool enabled = true, RoutedCommand command = null)
        {
            Key = key;
            Modifiers = modifiers;
            Enabled = enabled;
            ActiveOnly = activeOnly;
            Command = command;
        }

        public HotKey(Key key, ModifierKeys modifiers, RoutedCommand command ) : this(key, modifiers, false, true, command)
        {
        }

        public HotKey(Key key, ModifierKeys modifiers) : this(key, modifiers, false, true, null)
        {
        }

        private RoutedCommand _command;

        public RoutedCommand Command
        {
            get { return _command; }
            set
            {
                if (_command != value)
                {
                    _command = value;
                    OnPropertyChanged("Command");
                }
            }
        }

        private Key _key;

        public Key Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        private ModifierKeys _modifiers;

        public ModifierKeys Modifiers
        {
            get { return _modifiers; }
            set
            {
                if (_modifiers != value)
                {
                    _modifiers = value;
                    OnPropertyChanged("Modifiers");
                }
            }
        }

        private bool _enabled;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        private bool _activeOnly;

        public bool ActiveOnly
        {
            get { return _activeOnly; }
            set
            {
                if (value != _activeOnly)
                {
                    _activeOnly = value;
                    OnPropertyChanged("ActiveOnly");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public override bool Equals(object obj)
        {
            HotKey hotKey = obj as HotKey;
            if (hotKey != null)
                return Equals(hotKey);
            else
                return false;
        }

        public bool Equals(HotKey other)
        {
            return (Key == other.Key && Modifiers == other.Modifiers);
        }

        public override int GetHashCode()
        {
            return (int) Modifiers + 10*(int) Key;
        }

        public override string ToString()
        {
            return string.Format("{0} + {1} ({2}Enabled), {3}",
                                 Key, Modifiers,
                                 Enabled ? "" : "Not ",
                                 ActiveOnly ? "Active Window Only" : "");
        }

        public event EventHandler<HotKeyEventArgs> HotKeyPressed;

        protected virtual void OnHotKeyPress()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this, new HotKeyEventArgs(this));

            if (Command != null)
            {
                Command.Execute(null,null);
            }
        }

        internal void RaiseOnHotKeyPressed()
        {
            OnHotKeyPress();
        }
    }

    public sealed class HotKeyHost : IDisposable
    {
        public HotKeyHost(Window window)
        {
            _window = window;
            var hwnd = (HwndSource) HwndSource.FromVisual(window);
            Init(hwnd);
        }

        private void Init(HwndSource hwndSource)
        {
            if (hwndSource == null)
                throw new ArgumentNullException("hwndSource");

            this.hook = new HwndSourceHook(WndProc);
            this.hwndSource = hwndSource;
            hwndSource.AddHook(hook);

            IsEnabled = true;
            GlobalEnabled = true;
        }

        public bool IsEnabled { get; set; }
        public bool GlobalEnabled { get; set; }

        private void RegisterHotKey(int id, HotKey hotKey)
        {
            if (!hotKey.ActiveOnly && GlobalEnabled)
            {
                RegisterGlobalHotKey(id, hotKey);
            }
            else
            {
                RegisterActiveWindowHotkey(hotKey);
            }
        }

        private void UnregisterHotKey(int id)
        {
            HotKey hotKey = hotKeys[id];
            if (!hotKey.ActiveOnly && GlobalEnabled)
            {
                UnregisterGlobalHotKey(id);
            }
            else
            {
                UnregisterActiveWindowHotkey(hotKey);
            }
        }

        #region HotKey Interop

        private const int WM_HotKey = 786;

        private Window _window;

        [DllImport("user32", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        private static extern int RegisterHotKey(IntPtr hwnd,
                                                 int id, int modifiers, int key);

        [DllImport("user32", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);

        #endregion

        #region Interop-Encapsulation

        private HwndSourceHook hook;
        private HwndSource hwndSource;

        private void RegisterGlobalHotKey(int id, HotKey hotKey)
        {
            if ((int) hwndSource.Handle != 0)
            {
                RegisterHotKey(hwndSource.Handle, id, (int) hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
                int error = Marshal.GetLastWin32Error();
                if (error != 0)
                {
                    Exception e = new Win32Exception(error);

                    if (error == 1409)
                        throw new HotKeyAlreadyRegisteredException(e.Message, hotKey, e);
                    else
                        throw e;
                }
            }
            else
                throw new InvalidOperationException("Handle is invalid");
        }

        private void UnregisterGlobalHotKey(int id)
        {
            if ((int) hwndSource.Handle != 0)
            {
                UnregisterHotKey(hwndSource.Handle, id);
                int error = Marshal.GetLastWin32Error();
                if (error != 0)
                    throw new Win32Exception(error);
            }
        }

        #endregion

        public event EventHandler<HotKeyEventArgs> GlobalHotKeyPressed;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HotKey)
            {
                if (IsEnabled && hotKeys.ContainsKey((int) wParam))
                {
                    HotKey h = hotKeys[(int) wParam];
                    if (!h.ActiveOnly && GlobalEnabled)
                    {
                        h.RaiseOnHotKeyPressed();
                        if (GlobalHotKeyPressed != null)
                            GlobalHotKeyPressed(this, new HotKeyEventArgs(h));
                    }
                }
            }

            return new IntPtr(0);
        }

        #region ActiveWindowHotkeyBinding

        private void RegisterActiveWindowHotkey(HotKey hotkey)
        {
            hotkey.Command.InputGestures.Add(new KeyGesture(hotkey.Key, hotkey.Modifiers));
        }

        private void UnregisterActiveWindowHotkey(HotKey hotkey)
        {
            foreach (KeyGesture keygesture in hotkey.Command.InputGestures)
            {
                if (keygesture.Key == hotkey.Key && keygesture.Modifiers == hotkey.Modifiers)
                {
                    hotkey.Command.InputGestures.Remove(keygesture);
                    break;
                }
            }
        }

        #endregion

        private void hotKey_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var kvPair = hotKeys.FirstOrDefault(h => h.Value == sender);
            if (kvPair.Value != null)
            {
                switch (e.PropertyName)
                {
                    case "Enabled":
                        if (kvPair.Value.Enabled)
                            RegisterHotKey(kvPair.Key, kvPair.Value);
                        else
                            UnregisterHotKey(kvPair.Key);
                        break;
                    case "Modifiers":
                    case "Key":
                        if (kvPair.Value.Enabled)
                        {
                            UnregisterHotKey(kvPair.Key);
                            RegisterHotKey(kvPair.Key, kvPair.Value);
                        }
                        break;
                }
            }
        }


        private Dictionary<int, HotKey> hotKeys = new Dictionary<int, HotKey>();


        public class SerialCounter
        {
            public SerialCounter(int start)
            {
                Current = start;
            }

            public int Current { get; private set; }

            public int Next()
            {
                return ++Current;
            }
        }

        public IEnumerable<HotKey> HotKeys
        {
            get { return hotKeys.Values; }
        }


        private static readonly SerialCounter idGen = new SerialCounter(1);
                                              //Annotation: Can be replaced with "Random"-class

        public static readonly RoutedCommand ActiveWindowCommand = new RoutedCommand();

        public void AddHotKey(HotKey hotKey)
        {
            if (hotKey == null)
                throw new ArgumentNullException("value");
            if (hotKey.Key == 0)
                throw new ArgumentNullException("value.Key");
            if (hotKeys.ContainsValue(hotKey))
                throw new HotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);

            int id = idGen.Next();
            if (hotKey.Enabled)
            {
                RegisterHotKey(id, hotKey);
            }
            hotKey.PropertyChanged += hotKey_PropertyChanged;
            hotKeys[id] = hotKey;
        }


        public bool RemoveHotKey(HotKey hotKey)
        {
            var kvPair = hotKeys.FirstOrDefault(h => h.Value == hotKey);
            if (kvPair.Value != null)
            {
                kvPair.Value.PropertyChanged -= hotKey_PropertyChanged;
                if (kvPair.Value.Enabled)
                    UnregisterHotKey(kvPair.Key);
                return hotKeys.Remove(kvPair.Key);
            }
            return false;
        }


        #region Destructor

        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                hwndSource.RemoveHook(hook);
            }

            for (int i = hotKeys.Count - 1; i >= 0; i--)
            {
                RemoveHotKey(hotKeys.Values.ElementAt(i));
            }


            disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~HotKeyHost()
        {
            this.Dispose(false);
        }

        #endregion
    }

    public static class CustomCommands
    {
        public static RoutedCommand PlayPause = MediaCommands.TogglePlayPause;
        public static RoutedCommand Next = MediaCommands.NextTrack;
        public static RoutedCommand ThumbsUp = new RoutedCommand();
        public static RoutedCommand ThumbsDown = new RoutedCommand();
    }
}