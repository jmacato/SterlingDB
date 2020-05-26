using System;

namespace SterlingDB.Events
{
    /// <summary>
    ///     Notify arguments when changes happen
    /// </summary>
    public class SterlingOperationArgs : EventArgs
    {
        public SterlingOperationArgs(SterlingOperation operation, Type targetType, object key)
        {
            TargetType = targetType;
            Operation = operation;
            Key = key;
        }

        public Type TargetType { get; }

        public object Key { get; }

        public SterlingOperation Operation { get; }
    }
}