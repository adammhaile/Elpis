 /*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of PandoraSharp.
 * PandoraSharp is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * PandoraSharp is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with PandoraSharp. If not, see http://www.gnu.org/licenses/.
*/

using System.Collections.Generic;

namespace PandoraSharp
{
    public enum SongRating
    {
        ban,
        love,
        none
    }

    public static class PAudioFormat
    {
        //AAC+, 64Kbps
        public static readonly string AACPlus = "aacplus";
        //MP3, VBR 128Kbps
        public static readonly string MP3 = "mp3";
        //MP3, CBR 192Kbps - Pandora One users only.
        //Will default back to MP3 if selected without a One account. (enforced on server)
        public static readonly string MP3_HIFI = "mp3-hifi";
    }

    internal class Const
    {
        public static readonly string PROTOCOL_VERSION = "33";
        public static readonly string RPC_URL = @"www.pandora.com/radio/xmlrpc/v" + PROTOCOL_VERSION + "?";
        public static readonly string USER_AGENT = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.63 Safari/535.7";
        public static readonly int HTTP_TIMEOUT = 30;
        public static readonly string AUDIO_FORMAT = PAudioFormat.AACPlus;

        public static readonly int PLAYLIST_VALIDITY_TIME = 60*60*3;

        public static readonly string SYNC_KEY_URL = @"http://zzj.itf-inc.com/s2/getsynckey.php";

        public static readonly string SYNC_TIME_URL = @"http://ridetheclown.com/s2/synctime.php";
    }

    public class PDict : Dictionary<string, object>
    {
    }
}