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
            //Type t = def.GetType();
            //if (t != typeof (string) &&
            //    t != typeof (String) &&
            //    t != typeof (int) &&
            //    t != typeof (double) &&
            //    t != typeof (bool))
            //{
            //    throw new Exception("Type must be string, int, double or bool");
            //}

            Key = key;
            Default = def;
        }

        public string Key { get; private set; }
        public object Default { get; private set; }
    }

    public class MapConfig
    {
        private readonly Dictionary<string, object> _map;

        private const string _cryptCheck = "*_a3fc756b42_*";
        private readonly string _cryptPass = SystemInfo.GetUniqueHash();

        public MapConfig(string configPath = "")
        {
            ConfigPath = configPath;
            AutoSave = false;
            AutoSetDefaults = true;

            _map = new Dictionary<string, object>();
        }

        private string _lastConfig = string.Empty;
        public string LastConfig { get { return _lastConfig; } }

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

                _lastConfig = string.Empty;
                foreach (var l in lines)
                    _lastConfig += (l + "\r\n");
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
                            //Deal with our lists of config items saved out to keys that look like Key[ID]
                            if(split[0].Contains("["))
                            {
                                string[] splitList = split[0].Split(new []{"[","]"}, StringSplitOptions.None);
                                if(!_map.ContainsKey(splitList[0]))
                                {
                                    _map[splitList[0]] = new Dictionary<int, string>();
                                }
                                ((Dictionary<int,string>)_map[splitList[0]])[int.Parse(splitList[1])] = split[1];
                            }
                            //This is the normal stype of config entry - just a string
                            else
                            {
                                if (_map.ContainsKey(split[0]))
                                    _map[split[0]] = split[1]; //cascading style. newer values override old
                                else
                                    _map.Add(split[0], split[1]);
                            }
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

            foreach (var kvp in _map)
            {
                Type t = kvp.Value.GetType();
                if (t.IsGenericType)
                {
                    foreach (var item in (Dictionary<int, string>)kvp.Value)
                    {
                        configs.Add(kvp.Key + '[' + item.Key + ']' + '|' + item.Value);
                    }
                }
                else
                {
                    configs.Add(kvp.Key + '|' + kvp.Value);
                }
            }
            

            bool result = true;
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(ConfigPath, false, Encoding.Unicode);
                _lastConfig = string.Empty;
                foreach (string line in configs.ToArray())
                {
                    sw.WriteLine(line);
                    _lastConfig += (line + "\r\n");
                }
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

        public void SetValue(string key, Dictionary<int,string> value )
        {
            if (_map.ContainsKey(key))
                _map[key] = value;
            else
                _map.Add(key, value);
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
            Type dType = entry.Default.GetType();
            if (t != dType && !(t.IsGenericType && t.GetGenericTypeDefinition() == dType.GetGenericTypeDefinition()))
                throw new Exception("Value type does not equal default value type.");

            if (t == typeof(string) || t == typeof(String)) SetValue(entry.Key, (string)value);
            else if (t == typeof(int)) SetValue(entry.Key, (int)value);
            else if (t == typeof(double)) SetValue(entry.Key, (double)value);
            else if (t == typeof(bool) || t == typeof(Boolean)) SetValue(entry.Key, (bool)value);
            else if (t == typeof(Dictionary<int,string>)) SetValue(entry.Key, (Dictionary<int,string>)value);
            else SetValue(entry.Key, value.ToString());
        }

        public void SetEncryptedString(MapConfigEntry entry, string value)
        {
            Type t = entry.Default.GetType();
            if (!(t == typeof(string) || t == typeof(String)))
                throw new Exception("SetEncryptedString only works on string entries.");

            SetValue(entry, StringCrypt.EncryptString(_cryptCheck + value, _cryptPass));
        }

        public string GetValue(string key, string defValue)
        {
            if (_map.ContainsKey(key) && _map[key] as string != null)
               return ((string) _map[key]).Replace(@"&sep;", @"|");

           if (AutoSetDefaults)
               SetValue(key, defValue);
            return defValue;
            
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

        private Dictionary<int,string> GetValue(string key, Dictionary<int, string> defValue)
        {
             if (_map.ContainsKey(key))
                    return (Dictionary<int,string>) _map[key];

            if (AutoSetDefaults)
                SetValue(key, defValue);
                
            return defValue;
        }

        public object GetValue(MapConfigEntry entry)
        {
            Type t = entry.Default.GetType();

            if (t == typeof(string) || t == typeof(String)) return GetValue(entry.Key, (string)entry.Default);
            else if (t == typeof(int)) return GetValue(entry.Key, (int)entry.Default);
            else if (t == typeof(double)) return GetValue(entry.Key, (double)entry.Default);
            else if (t == typeof(bool) || t == typeof(Boolean)) return GetValue(entry.Key, (bool)entry.Default);
            else if (t == typeof(Dictionary<int, string>))
                return GetValue(entry.Key, ((Dictionary<int, string>) entry.Default));
            else return GetValue(entry.Key, (string)entry.Default.ToString()); //On their own to parse it

            throw new Exception("Default Type must be string, int, double, bool or Dictionary<int,string>");
        }

        public string GetEncryptedString(MapConfigEntry entry)
        {
            Type t = entry.Default.GetType();
            if (!(t == typeof(string) || t == typeof(String)))
                throw new Exception("GetEncryptedString only works on string entries.");

            string result = string.Empty;
            try
            {
                result = StringCrypt.DecryptString((string)GetValue(entry), _cryptPass);
                if (result != string.Empty)
                {
                    if (result.StartsWith(_cryptCheck))
                        result = result.Replace(_cryptCheck, string.Empty);
                    else
                        result = string.Empty;
                }
            }
            catch
            {
                result = string.Empty;
            }

            return result;
        }
    }
}