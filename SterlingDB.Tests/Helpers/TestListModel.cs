using System.Collections.Generic;

namespace SterlingDB.Test.Helpers
{
    public class TestListModel
    {
        private static int _nextId;

        public int ID { get; set; }

        public List<TestModel> Children { get; set; }

        public static TestListModel MakeTestListModel()
        {
            return new TestListModel
            {
                ID = _nextId++,
                Children =
                    new List<TestModel>
                        {TestModel.MakeTestModel(), TestModel.MakeTestModel(), TestModel.MakeTestModel()}
            };
        }
    }
}