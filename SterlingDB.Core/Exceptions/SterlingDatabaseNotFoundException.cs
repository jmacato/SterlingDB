using System;

namespace SterlingDB.Exceptions
{
    public class SterlingDatabaseNotFoundException : SterlingException
    {
        public SterlingDatabaseNotFoundException(string databaseName)
            : base(databaseName)
        {
        }
    }
}