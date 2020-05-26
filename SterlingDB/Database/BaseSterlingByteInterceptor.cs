using System;

namespace SterlingDB.Database
{
    public abstract class BaseSterlingByteInterceptor : ISterlingByteInterceptor
    {
        public virtual byte[] Save(byte[] sourceStream)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] Load(byte[] sourceStream)
        {
            throw new NotImplementedException();
        }
    }
}