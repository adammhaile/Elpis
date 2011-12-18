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
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Util
{
    public class HashHelper
    {
        public static string HashArrayToString(byte[] hash)
        {
            var sb = new StringBuilder();
            foreach (byte b in hash) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        public static string GetStringHash(string data)
        {
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(data));
            return HashArrayToString(hash);
        }
    }

    public class SystemInfo
    {
        public static string GetUniqueHash()
        {
            ManagementObjectCollection cpus = (new ManagementObjectSearcher("SELECT * FROM Win32_Processor")).Get();
            string cpu_serial = "";
            foreach (ManagementBaseObject c in cpus)
            {
                cpu_serial = (string) c["ProcessorId"];
                break;
            }

            ManagementObjectCollection mobos = (new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard")).Get();
            string mobo = "";
            foreach (ManagementBaseObject m in mobos)
            {
                mobo = (string) m["SerialNumber"];
                break;
            }

            return (cpu_serial + mobo).MD5Hash();
        }

        public static string GetWindowsVersion()
        {
            OperatingSystem os = Environment.OSVersion;
            Version vs = os.Version;

            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        operatingSystem = "Future";
                        break;
                }
            }
            if (operatingSystem != "")
            {
                operatingSystem = "Windows " + operatingSystem;

                if (os.ServicePack != "")
                {
                    operatingSystem += " " + os.ServicePack;
                }

                string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE",
                                                               EnvironmentVariableTarget.Machine);
                int osArch = ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);

                operatingSystem += " " + osArch + "-bit";
            }

            return operatingSystem;
        }
    }
}