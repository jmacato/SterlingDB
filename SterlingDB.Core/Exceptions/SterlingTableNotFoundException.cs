using System;

namespace SterlingDB.Exceptions
{
    public class SterlingTableNotFoundException : SterlingException
    {
        public SterlingTableNotFoundException(Type tableType, string databaseName) :
                    base($"{tableType.FullName} {databaseName}")
        {
        }

        public SterlingTableNotFoundException(string typeName, string databaseName) :
                    base($"{typeName} {databaseName}")
        {
        }
    }
}