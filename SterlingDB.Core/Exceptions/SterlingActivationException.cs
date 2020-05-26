namespace SterlingDB.Exceptions
{
    public class SterlingActivationException : SterlingException
    {
        public SterlingActivationException(string operation) : base(operation)
        {
        }
    }
}