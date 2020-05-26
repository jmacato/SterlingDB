using System.Collections.Generic;

namespace SterlingDB.Server.FileSystem
{
    internal static class PathLock
    {
        private static readonly Dictionary<int, AsyncLock> _pathLocks = new Dictionary<int, AsyncLock>();

        public static AsyncLock GetLock(string path)
        {
            var hash = path.GetHashCode();

            lock (_pathLocks)
            {
                if (_pathLocks.TryGetValue(hash, out var aLock) == false) aLock = _pathLocks[hash] = new AsyncLock();

                return aLock;
            }
        }
    }
}