namespace SterlingDB
{
    /// <summary>
    ///     Lock mechanism
    /// </summary>
    public interface ISterlingLock
    {
        object Lock { get; }
    }
}