using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elpis
{
    public class CommandLineOptions
    {
        public bool SkipTrack = false;
        public bool TogglePlayPause = false;
        public bool DoThumbsUp = false;
        public bool DoThumbsDown = false;
        public string StationToLoad = null;
        public bool ShowHelp = false;
        public string ConfigPath = null;
    }
}
