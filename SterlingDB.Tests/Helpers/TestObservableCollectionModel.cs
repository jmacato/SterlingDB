using System.Collections.ObjectModel;

namespace SterlingDB.Test.Helpers
{
    public class TestObservableCollectionModel
    {
        private static int _nextId;

        public int ID { get; set; }

        public ObservableCollection<TestModel> Children { get; set; }

        public static TestObservableCollectionModel MakeTestListModel()
        {
            return new TestObservableCollectionModel
            {
                ID = _nextId++,
                Children =
                    new ObservableCollection<TestModel>
                        {TestModel.MakeTestModel(), TestModel.MakeTestModel(), TestModel.MakeTestModel()}
            };
        }
    }
}