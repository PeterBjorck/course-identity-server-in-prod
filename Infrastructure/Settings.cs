using System;

namespace Infrastructure
{
    public static class Settings
    {
        /// <summary>
        /// The date and time when this service was started
        /// </summary>
        public static DateTime StartupTime;

#if DEBUG
        /// <summary>
        /// True if we are in a debug build
        /// </summary>
        public static bool IsReleaseBuild = false;
#else
        /// <summary>
        /// True if we are in a debug build
        /// </summary>
        public static bool IsReleaseBuild = true;
#endif
    }
}
