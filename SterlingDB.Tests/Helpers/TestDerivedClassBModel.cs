using System;

namespace SterlingDB.Test.Helpers
{
    public class TestDerivedClassBModel : TestBaseClassModel
    {
        public String PropertyB { get; set; }

        public TestDerivedClassBModel()
            : base()
        {
            PropertyB = "Property B Value";
        }
    }
}
