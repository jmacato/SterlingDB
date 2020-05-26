using System;

namespace SterlingDB.Exceptions
{
    public class SterlingDuplicateTypeException : SterlingException
    {
        public SterlingDuplicateTypeException(Type type, string databaseName) :
            base($"{type.FullName}, {databaseName}")
        {
        }
    }
}