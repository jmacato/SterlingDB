using System;

namespace SterlingDB.Exceptions
{
    public class SterlingIsolatedStorageException : SterlingException
    {
        public SterlingIsolatedStorageException(Exception ex) : base(ex.Message)
        {
            
        }
    }
}
