﻿namespace SterlingDB
{
    /// <summary>
    ///     Byte Interceptor interface
    /// </summary>
    public interface ISterlingByteInterceptor
    {
        byte[] Save(byte[] sourceStream);
        byte[] Load(byte[] sourceStream);
    }
}