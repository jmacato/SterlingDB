using System;
using SterlingDB.Core.Serialization;

namespace SterlingDB.Core.Exceptions
{
    public class SterlingSerializerException : SterlingException 
    {
        public SterlingSerializerException(ISterlingSerializer serializer, Type targetType) : 
            base(string.Format(Exceptions.SterlingSerializerException, serializer.GetType().FullName, targetType.FullName))
        {
            
        }
    }
}
