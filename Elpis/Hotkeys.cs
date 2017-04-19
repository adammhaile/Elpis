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
using System.Windows.Threading;
using DrWPF.Windows.Data;
using Log = Util.Log;

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

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey)
            : base(message)
        {
            HotKey = hotKey;
        }

        public HotKeyAlreadyRegisteredException(string message, HotKey hotKey, Exception inner)
            : base(message, inner)
        {
            HotKey = hotKey;
        }
    }

    public class HotKeyNotSupportedException : Exception
    {
        public HotKey HotKey { get; private set; }

        public HotKeyNotSupportedException(string message, HotKey hotKey)
            : base(message)
        {
            HotKey = hotKey;
        }

        public HotKeyNotSupportedException(string message, HotKey hotKey, Exception inner)
            : base(message, inner)
        {
            HotKey = hotKey;
        }
    }

    public class HotKey : INotifyPropertyChanged, IEquatable<HotKey>
    {
        protected HotKey()
        {
        }

        public HotKey(RoutedUICommand command, Key key, ModifierKeys modifiers, bool global, bool enabled = true)
        {
            Key = key;
            Modifiers = modifiers;
            Enabled = enabled;
            Global = global;
            Command = command;
        }

        public HotKey(RoutedUICommand command, Key key, ModifierKeys modifiers)
            : this(command, key, modifiers, false, true)
        {
        }

        private RoutedUICommand _command;

        public RoutedUICommand Command
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
                    OnPropertyChanging("Key");
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
                    OnPropertyChanging("Modifiers");
                    _modifiers = value;
                    OnPropertyChanged("Modifiers");
                }
            }
        }

        public void SetKeyCombo(Key key, ModifierKeys modifiers)
        {
            OnPropertyChanging("Key");
            _key = key;
            _modifiers = modifiers;
            OnPropertyChanged("Key");
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

        private bool _global;

        public bool Global
        {
            get { return _global; }
            set
            {
                if (value != _global)
                {
                    OnPropertyChanging("Global");
                    _global = value;
                    OnPropertyChanged("Global");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public virtual void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
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
            return (int)Modifiers + 10 * (int)Key;
        }

        public string KeysString
        {
            get { return (Modifiers == ModifierKeys.None ? "" : (Modifiers.ToString() + " + ")) + Key.ToString(); }
        }

        public override string ToString()
        {
            return string.Format("{0} + {1} ({2}Enabled), {3}",
                                 Key, Modifiers,
                                 Enabled ? "" : "Not ",
                                 Global ? "Global" : "");
        }
    }

    public static class PlayerCommands
    {
        public static RoutedUICommand getCommandByName(string name)
        {
            if (name == PlayPause.Name) return PlayPause;
            if (name == Next.Name) return Next;
            if (name == ThumbsUp.Name) return ThumbsUp;
            if (name == ThumbsDown.Name) return ThumbsDown;
            return null;
        }

        public static List<RoutedUICommand> AllCommands
        {
            get { return new List<RoutedUICommand>() { PlayPause, Next, ThumbsUp, ThumbsDown }; }

        }

        public static RoutedUICommand PlayPause = new RoutedUICommand("Pause currently playing track or Play if paused", "Play/Pause", typeof(PlayerCommands));
        public static RoutedUICommand Next = new RoutedUICommand("Skips currently playing track", "Skip Song", typeof(PlayerCommands));
        public static RoutedUICommand ThumbsUp = new RoutedUICommand("Marks this as a liked track that suits this station", "Thumbs Up", typeof(PlayerCommands));
        public static RoutedUICommand ThumbsDown = new RoutedUICommand("Marks this as a disliked track or one that doesn't suit this station", "Thumbs Down", typeof(PlayerCommands));
    }

    public sealed class HotKeyHost : IDisposable
    {
        private Window _window;
        public bool IsEnabled { get; set; }
        private ObservableDictionary<int, HotKey> hotKeys = new ObservableDictionary<int, HotKey>();

        public HotKeyHost(Window window)
        {
            _window = window;
            var hwnd = (HwndSource)HwndSource.FromVisual(window);
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
        }

        private void RegisterHotKey(int id, HotKey hotKey)
        {
            if (hotKey.Global)
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
            if (hotKey.Global)
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
            if ((int)hwndSource.Handle != 0)
            {
                RegisterHotKey(hwndSource.Handle, id, (int)hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
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
            if ((int)hwndSource.Handle != 0)
            {
                UnregisterHotKey(hwndSource.Handle, id);
                int error = Marshal.GetLastWin32Error();
                if (error != 0)
                    throw new Win32Exception(error);
            }
        }

        #endregion

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HotKey)
            {
                Log.O("HotKeys WndProc: IsEnabled - {0}", IsEnabled.ToString());
                if (IsEnabled && hotKeys.ContainsKey((int)wParam))
                {
                    HotKey h = hotKeys[(int)wParam];
                    Log.O("HotKeys WndProc: HotKey - {0}", h.KeysString);
                    if (h.Global)
                    {
                        if (h.Command != null)
                        {
                            h.Command.Execute(null, _window);
                        }
                    }
                }
            }

            return new IntPtr(0);
        }

        #region ActiveWindowHotkeyBinding

        private void RegisterActiveWindowHotkey(HotKey hotkey)
        {
            try
            {
                hotkey.Command.InputGestures.Add(new KeyGesture(hotkey.Key, hotkey.Modifiers));
            }
            catch (NotSupportedException e)
            {
                throw new HotKeyNotSupportedException("Alphanumeric Keys without modifiers are not supported as hotkeys", hotkey, e);
            }
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

        private void hotKey_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Global":
                case "Modifiers":
                case "Key":
                    var kvPair = hotKeys.FirstOrDefault(h => h.Value == sender);
                    if (kvPair.Value != null) { UnregisterHotKey(kvPair.Key); }
                    break;
            }
        }

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
                    case "Global":
                    case "Modifiers":
                    case "Key":
                        if (kvPair.Value.Enabled)
                        {
                            RegisterHotKey(kvPair.Key, kvPair.Value);
                        }
                        break;
                }
            }
        }

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

        public ObservableDictionary<int, HotKey> HotKeys
        {
            get { return hotKeys; }
        }


        private static readonly SerialCounter idGen = new SerialCounter(-1);

        public HotKey AddHotKey(HotKey hotKey)
        {
            try
            {
                if (hotKey == null)
                    throw new ArgumentNullException("value");
                /* We let em add as many null keys to the list as they want, but never register them*/
                if (hotKey.Key != Key.None && hotKeys.ContainsValue(hotKey))
                {
                    throw new HotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);
                    //Log.O("HotKey already registered!");
                }

                try
                {
                    int id = idGen.Next();
                    if (hotKey.Enabled && hotKey.Key != Key.None)
                    {
                        RegisterHotKey(id, hotKey);
                    }
                    hotKey.PropertyChanging += hotKey_PropertyChanging;
                    hotKey.PropertyChanged += hotKey_PropertyChanged;
                    hotKeys[id] = hotKey;
                    return hotKey;
                }
                catch (HotKeyNotSupportedException e)
                {
                    return null;
                }
            }
            catch (HotKeyAlreadyRegisteredException e)
            {
                Log.O("HotKey already registered!");
            }
            return null;
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
                UnregisterGlobalHotKey(i);
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


}