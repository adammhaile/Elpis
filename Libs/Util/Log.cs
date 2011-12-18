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
using System.Diagnostics;
using System.IO;

namespace Util
{
    public class Log
    {
        #region Delegates

        public delegate void LogMessageEventHandler(string msg);

        #endregion

        private static string _logPath;
        private static StreamWriter _sw;
        private static readonly object _swLock = new object();

        private static bool _timestamp = true;

        public static bool WriteTimestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public static event LogMessageEventHandler LogMessage;

        public static void SetLogPath(string path, bool append = false)
        {
            _logPath = path;

            lock (_swLock)
            {
                if (_sw != null)
                {
                    _sw.Flush();
                    _sw.Close();
                }

                try
                {
                    _sw = new StreamWriter(_logPath, append);
                }
                catch (Exception e)
                {
                    _sw = null;
                    throw;
                }
            }
        }

        public static void O(string msg, params object[] arg)
        {
            try
            {
                Debug.WriteLine(msg, arg);
            }
            catch
            {
                Debug.WriteLine(msg);
            }

            if (_sw == null) return;

            lock (_swLock)
            {
                string timestamp = "";
                if (WriteTimestamp) timestamp = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] ";

                try
                {
                    _sw.WriteLine(timestamp + msg, arg);
                    _sw.Flush();
                }
                catch (FormatException fex)
                {
                    try
                    {
                        _sw.WriteLine(timestamp + msg);
                        _sw.Flush();
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }
            }

            OnLog(String.Format(msg, arg));
        }

        private static void OnLog(string msg)
        {
            if (LogMessage != null)
                LogMessage(msg);
        }
    }
}