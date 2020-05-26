using System;
using SterlingDB.Database;

namespace SterlingDB
{
    /// <summary>
    ///     Wrapper for the sterling database engine
    /// </summary>
    public class SterlingEngine : IDisposable
    {
        private Lazy<SterlingDatabase> _database;

        /// <summary>
        ///     Constructor takes in the database
        /// </summary>
        public SterlingEngine(ISterlingPlatformAdapter platform)
        {
            PlatformAdapter = platform;
            _database = new Lazy<SterlingDatabase>(() => new SterlingDatabase(this));
        }

        /// <summary>
        ///     The database engine
        /// </summary>
        public ISterlingDatabase SterlingDatabase => _database.Value;

        public ISterlingPlatformAdapter PlatformAdapter { get; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _database.Value.Deactivate();
        }

        public void Reset()
        {
            _database = new Lazy<SterlingDatabase>(() => new SterlingDatabase(this));
        }

        public void Activate()
        {
            _database.Value.Activate();
        }
    }
}