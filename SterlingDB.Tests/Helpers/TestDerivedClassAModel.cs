namespace SterlingDB.Test.Helpers
{
    public class TestDerivedClassAModel : TestBaseClassModel
    {
        public TestDerivedClassAModel()
        {
            PropertyA = "Property A value";
        }

        public string PropertyA { get; set; }
    }
}