using System;

namespace SterlingDB.Exceptions
{
    public class SterlingLoggerNotFoundException : SterlingException 
    {
        public SterlingLoggerNotFoundException(Guid guid) : base(guid.ToString())
        {
            
        }
    }
}
