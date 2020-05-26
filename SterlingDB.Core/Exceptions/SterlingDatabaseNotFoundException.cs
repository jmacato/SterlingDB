﻿using System;

namespace SterlingDB.Core.Exceptions
{
    public class SterlingDatabaseNotFoundException : SterlingException
    {
        public SterlingDatabaseNotFoundException(string databaseName)
            : base(string.Format(Exceptions.SterlingDatabaseNotFoundException, databaseName))
        {
        }
    }
}