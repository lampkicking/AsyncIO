﻿namespace AsyncIO
{
    public static class ForceDotNet
    {
        internal static bool Forced { get; private set; }

        public static void Force()
        {
            Forced = true;
        }

        public static void Unforce()
        {
            Forced = false;
        }
    }
}
