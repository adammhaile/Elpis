/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of BassPlayer.
 * BassPlayer is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * BassPlayer is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with BassPlayer. If not, see http://www.gnu.org/licenses/.
*/

using System;
using SD = System.Diagnostics;

namespace BassPlayer
{
    public enum LogType
    {
        Log,
        Recorder,
        Error,
        EPG,
        VMR9,
        Config,
        MusicShareWatcher,
        WebEPG
    }

    public class Log
    {
        public static void Info(string format, params object[] arg)
        {
            Util.Log.O("Info: " + format, arg);
        }

        public static void Info(LogType type, string format, params object[] arg)
        {
            Util.Log.O("Info: " + format, arg);
        }

        public static void Error(string format, params object[] arg)
        {
            Util.Log.O("Error: " + format, arg);
        }

        public static void Error(Exception ex)
        {
            Util.Log.O("Error: " + ex);
        }

        public static void Error(LogType type, string format, params object[] arg)
        {
            Util.Log.O("Error: " + format, arg);
        }

        public static void Warn(string format, params object[] arg)
        {
            Util.Log.O("Warn: " + format, arg);
        }

        public static void Warn(LogType type, string format, params object[] arg)
        {
            Util.Log.O("Warn: " + format, arg);
        }

        public static void Debug(string format, params object[] arg)
        {
            Util.Log.O("Debug: " + format, arg);
        }

        public static void Debug(LogType type, string format, params object[] arg)
        {
            Util.Log.O("Debug: " + format, arg);
        }
    }
}