using System;

namespace SterlingDB.Exceptions
{
    public class SterlingIndexNotFoundException : SterlingException 
    {
        public SterlingIndexNotFoundException(string indexName, Type type) : 
            base($"{indexName} {type.FullName}")
        {
            
        }
    }
}
