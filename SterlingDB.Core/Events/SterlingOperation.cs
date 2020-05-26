namespace SterlingDB.Events
{
    /// <summary>
    ///     Operation in STerling
    /// </summary>
    public enum SterlingOperation
    {
        Save,
        Load,
        Delete,
        Flush,
        Purge,
        Truncate
    }
}
