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
using System.IO;
using System.Text;

namespace Util
{
    public class MapConfigEntry
    {
        public MapConfigEntry(string key, object def)
        {
            Type t = def.GetType();
            if (t != typeof (string) &&
                t != typeof (String) &&
                t != typeof (int) &&
                t != typeof (double) &&
                t != typeof (bool))
            {
                throw new Exception("Type must be string, int, double or bool");
            }

            Key = key;
            Default = def;
        }

        public string Key { get; private set; }
        public object Default { get; private set; }
    }

    public class MapConfig
    {
        private readonly Dictionary<string, string> _map;

        public MapConfig(string configPath = "")
        {
            ConfigPath = configPath;
            AutoSave = false;
            AutoSetDefaults = true;

            _map = new Dictionary<string, string>();
        }

        public string ConfigPath { get; private set; }

        public bool AutoSave { get; set; }

        public bool AutoSetDefaults { get; set; }

        public bool LoadConfig(string configData = "")
        {
            var lines = new List<string>();

            bool result = true;
            TextReader tr = null;
            try
            {
                if (configData == "")
                {
                    if (ConfigPath == "") return false;

                    if (!File.Exists(ConfigPath))
                    {
                        File.Create(ConfigPath).Close();
                        return true;
                    }

                    tr = new StreamReader(ConfigPath, Encoding.Unicode);
                }
                else
                {
                    tr = new StringReader(configData);
                }
                string line = "";
                while ((line = tr.ReadLine()) != null)
                    lines.Add(line);
            }
            catch (Exception ex)
            {
                Log.O(ex.ToString());
                result = false;
            }
            finally
            {
                if (tr != null) tr.Close();
            }

            if (result)
            {
                foreach (string line in lines)
                {
                    try
                    {
                        string[] split = line.Split('|');

                        if (split.Length == 2)
                        {
                            if (_map.ContainsKey(split[0]))
                                _map[split[0]] = split[1]; //cascading style. newer values override old
                            else
                                _map.Add(split[0], split[1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.O(ex.ToString());
                    }
                }
            }

            return result;
        }

        public bool SaveConfig()
        {
            var configs = new List<string>();

            foreach (var kvp in _map) configs.Add(kvp.Key + '|' + kvp.Value);

            bool result = true;
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(ConfigPath, false, Encoding.Unicode);
                foreach (string line in configs.ToArray())
                    sw.WriteLine(line);
            }
            catch (Exception ex)
            {
                Log.O(ex.ToString());
                result = false;
            }
            finally
            {
                if (sw != null) sw.Close();
            }

            return result;
        }

        public void SetValue(string key, string value)
        {
            value = value.Replace(@"|", @"&sep;");
            if (_map.ContainsKey(key))
                _map[key] = value;
            else
                _map.Add(key, value);

            if (AutoSave)
                SaveConfig();
        }

        public void SetValue(string key, int value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, double value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(string key, bool value)
        {
            SetValue(key, value.ToString());
        }

        public void SetValue(MapConfigEntry entry, object value)
        {
            Type t = value.GetType();
            if (t != entry.Default.GetType())
                throw new Exception("Value type does not equal default value type.");

            if (t == typeof (string) || t == typeof (String)) SetValue(entry.Key, (string) value);
            else if (t == typeof (int)) SetValue(entry.Key, (int) value);
            else if (t == typeof (double)) SetValue(entry.Key, (double) value);
            else if (t == typeof (bool) || t == typeof (Boolean)) SetValue(entry.Key, (bool) value);
        }

        public string GetValue(string key, string defValue)
        {
            if (_map.ContainsKey(key))
                return _map[key].Replace(@"&sep;", @"|");
            else
            {
                if (AutoSetDefaults)
                    SetValue(key, defValue);

                return defValue;
            }
        }

        public int GetValue(string key, int defValue)
        {
            string value = GetValue(key, defValue.ToString());
            int result = defValue;
            int.TryParse(value, out result);
            return result;
        }

        public double GetValue(string key, double defValue)
        {
            string value = GetValue(key, defValue.ToString());
            double result = defValue;
            double.TryParse(value, out result);
            return result;
        }

        public bool GetValue(string key, bool defValue)
        {
            string value = GetValue(key, defValue.ToString());
            bool result = defValue;
            bool.TryParse(value, out result);
            return result;
        }

        public object GetValue(MapConfigEntry entry)
        {
            Type t = entry.Default.GetType();

            if (t == typeof (string) || t == typeof (String)) return GetValue(entry.Key, (string) entry.Default);
            if (t == typeof (int)) return GetValue(entry.Key, (int) entry.Default);
            if (t == typeof (double)) return GetValue(entry.Key, (double) entry.Default);
            if (t == typeof (bool) || t == typeof (Boolean)) return GetValue(entry.Key, (bool) entry.Default);

            throw new Exception("Default Type must be string, int, double or bool");
        }
    }
}