
using System;

using SterlingDB;

namespace SterlingDB.Test
{
    internal static class Factory
    {
        public static SterlingEngine NewEngine()
        {
            return new SterlingEngine( NewPlatformAdapter() );
        }

        public static ISterlingPlatformAdapter NewPlatformAdapter()
        {
            return new SterlingDB.Server.PlatformAdapter();
        }
    }
}
