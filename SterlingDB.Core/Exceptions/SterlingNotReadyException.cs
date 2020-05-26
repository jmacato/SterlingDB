using System;

namespace SterlingDB.Core.Exceptions
{
    public class SterlingNotReadyException : SterlingException
    {
        public SterlingNotReadyException() : base(Exceptions.SterlingNotReadyException)
        {
            
        }
    }
}
