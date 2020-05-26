using System;

namespace SterlingDB.Exceptions
{
    public class SterlingTriggerException : SterlingException
    {
        public SterlingTriggerException(string message, Type triggerType) :
                            base($"{triggerType.FullName} {message}")

        {

        }
    }
}