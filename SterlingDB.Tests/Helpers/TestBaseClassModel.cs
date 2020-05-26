using System;

namespace SterlingDB.Test.Helpers
{
    public abstract class TestBaseClassModel
    {
        public TestBaseClassModel()
        {
            Key = Guid.NewGuid();
            BaseProperty = "Base Property Value";
        }

        public Guid Key { get; set; }
        public string BaseProperty { get; set; }
    }
}