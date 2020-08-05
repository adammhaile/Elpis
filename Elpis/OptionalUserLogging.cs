/*******************************************************************************************************
 * Purpose:
 * --------
 * This file contains optional functions to be used with configurable logging (in Pages\Settings.xaml)
 *******************************************************************************************************/
using PandoraSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elpis
{
    public static class OptionalUserLogging
    {
        public static void TryLogSongLoved(Config _config, Song song)
        {
            if (!_config.Fields.Logging_LogOnSongLike)
                return;

            using(StreamWriter writeFile = new StreamWriter(_config.Fields.Logging_LogOnSongLikeFile, append: true))
            {
                writeFile.WriteLine($"[{DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm")}] Liked song \"{song.Artist} - {song.SongTitle}\"");
            }
        }
    }
}
