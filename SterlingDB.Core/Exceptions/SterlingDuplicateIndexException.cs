using System;

namespace SterlingDB.Exceptions
{
    public class SterlingDuplicateIndexException : SterlingException 
    {
        public SterlingDuplicateIndexException(string indexName, Type type, string databaseName) : 
        base ($"{indexName} {type.FullName} {databaseName}")
        {
            
        }        
    }
}
