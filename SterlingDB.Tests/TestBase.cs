using System;

namespace SterlingDB.Test
{
    public abstract class TestBase : IDisposable
    {
        private bool disposedValue;

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual ISterlingDriver GetDriver()
        {
            return new MemoryDriver();
        }

#pragma warning disable
        public abstract void Cleanup();
#pragma warning restore

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) Cleanup();

                disposedValue = true;
            }
        }
    }
}