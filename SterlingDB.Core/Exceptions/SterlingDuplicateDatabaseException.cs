using System;

namespace SterlingDB.Exceptions
{
    public class SterlingDuplicateDatabaseException : SterlingException
    {
        public SterlingDuplicateDatabaseException(ISterlingDatabaseInstance instance) : base(
            instance.GetType().FullName)
        {
        }

        public SterlingDuplicateDatabaseException(Type type)
            : base(type.FullName)
        {
        }
    }
}