
using System;
using System.Collections;
using System.Collections.Generic;
using SterlingDB.Core;

namespace SterlingDB.Server.FileSystem
{
    internal static class PathLock
    {
        private static readonly Dictionary<int, AsyncLock> _pathLocks = new Dictionary<int, AsyncLock>();

        public static AsyncLock GetLock( string path )
        {
            var hash = path.GetHashCode();

            lock ( _pathLocks )
            {

                if (_pathLocks.TryGetValue(hash, out AsyncLock aLock) == false)
                {
                    aLock = _pathLocks[hash] = new AsyncLock();
                }

                return aLock;
            }
        }
    }
}
