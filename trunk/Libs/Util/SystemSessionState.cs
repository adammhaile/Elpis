using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Util
{
    public class SystemSessionState
    {
        public delegate void SystemLockedEvent();
        public event SystemLockedEvent SystemLocked;

        public delegate void SystemUnlockedEvent();
        public event SystemUnlockedEvent SystemUnlocked;

        public SystemSessionState()
        {
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch(e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    if (SystemLocked != null) SystemLocked();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    if (SystemUnlocked != null) SystemUnlocked();
                    break;
            }
        }
    }
}

