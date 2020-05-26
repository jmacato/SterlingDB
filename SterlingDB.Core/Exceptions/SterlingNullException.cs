using System;

namespace SterlingDB.Exceptions
{
    public class SterlingNullException : SterlingException 
    {
        public SterlingNullException(string property, Type type) : base($"{property} {type.FullName}")
        {
            
        }
    }
}
