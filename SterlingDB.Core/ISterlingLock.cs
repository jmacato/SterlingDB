namespace SterlingDB.Core
{
    /// <summary>
    ///     Lock mechanism
    /// </summary>
    public interface ISterlingLock
    {
        object Lock { get; }
    }
}
