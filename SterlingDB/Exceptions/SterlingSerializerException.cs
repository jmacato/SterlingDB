using System;
using SterlingDB.Serialization;

namespace SterlingDB.Exceptions
{
    public class SterlingSerializerException : SterlingException
    {
        public SterlingSerializerException(ISterlingSerializer serializer, Type targetType) :
            base($"{serializer.GetType().FullName} {targetType.FullName}")
        {
        }
    }
}