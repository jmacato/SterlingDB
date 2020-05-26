
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SterlingDB.Core;

namespace SterlingDB.Test
{
    public abstract class TestBase : IDisposable
    {
        private bool disposedValue;

        protected virtual ISterlingDriver GetDriver()
        {
            return new MemoryDriver();
        }

        public abstract void Cleanup();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Cleanup();
                }

                disposedValue = true;
            }
        }
 
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
