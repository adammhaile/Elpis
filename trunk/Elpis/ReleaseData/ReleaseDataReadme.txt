Seeing a missing ReleaseData.cs file in this directory is completely normal.
This is a file that is conditionally compiled in only for the AppRelease configuration.
Debug and Release will still build with no problems.

ReleaseData.cs contains various data value that I wanted to keep out of the public release.
For example, the Bass.Net (audio library) registration keys, while free (from bass.radio42.com)
are not allowed to be distributed, per the license agreement.

If you would like to create your own ReleaseData.cs, follow the template below:

namespace Elpis
{
    public class ReleaseData
    {
        public const string BassRegEmail = "";
        public const string BassRegKey = "";
        public const string UpdateBaseUrl = @"";
        public const string UpdateConfigFile = "";
        public const string AnalyticsPostURL = @"";
    }
}