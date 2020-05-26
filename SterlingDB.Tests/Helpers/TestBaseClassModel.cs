﻿using System;

namespace SterlingDB.Test.Helpers
{
    public abstract class TestBaseClassModel
    {
        public Guid Key { get; set; }
        public String BaseProperty { get; set; }

        public TestBaseClassModel()
        {
            Key = Guid.NewGuid();
            BaseProperty = "Base Property Value";
        }
    }
}
