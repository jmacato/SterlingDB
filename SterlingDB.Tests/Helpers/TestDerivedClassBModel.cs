namespace SterlingDB.Test.Helpers
{
    public class TestDerivedClassBModel : TestBaseClassModel
    {
        public TestDerivedClassBModel()
        {
            PropertyB = "Property B Value";
        }

        public string PropertyB { get; set; }
    }
}