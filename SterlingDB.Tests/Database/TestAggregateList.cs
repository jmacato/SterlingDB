using SterlingDB.Core;
using SterlingDB.Server.FileSystem;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestAggregateListAltDriver : TestAggregateList
    {
        protected override ISterlingDriver GetDriver()
        {
            return new FileSystemDriver();

        }
    }

    public class TestAggregateList : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestAggregateList()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
        }

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public void TestNullList()
        {
            var expected = TestAggregateListModel.MakeTestAggregateListModel();
            expected.Children = null;
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestAggregateListModel>(key).Result;
            Assert.NotNull(actual); //, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID); //, "Save/load failed: key mismatch.");
            Assert.Null(actual.Children); //, "Save/load failed: list should be null.");
        }

        [Fact]
        public void TestEmptyList()
        {
            var expected = TestAggregateListModel.MakeTestAggregateListModel();
            expected.Children.Clear();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestAggregateListModel>(key).Result;
            Assert.NotNull(actual);//, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID);//, "Save/load failed: key mismatch.");
            Assert.NotNull(actual.Children);//, "Save/load failed: list not initialized.");
            Assert.Empty(actual.Children);//, "Save/load failed: list size mismatch.");
        }

        [Fact]
        public void TestList()
        {
            var expected = TestAggregateListModel.MakeTestAggregateListModel();
            _databaseInstance.SaveAsync(expected).Wait();
            var actual = _databaseInstance.LoadAsync<TestAggregateListModel>(expected.ID).Result;
            Assert.NotNull(actual);//, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID);//, "Save/load failed: key mismatch.");
            Assert.NotNull(actual.Children);//, "Save/load failed: list not initialized.");
            Assert.Equal(expected.Children.Count, actual.Children.Count);//, "Save/load failed: list size mismatch.");

            for (var x = 0; x < expected.Children.Count; x++)
            {
                Assert.Equal(expected.Children[x].Key, actual.Children[x].Key);//, "Save/load failed: key mismatch.");
                Assert.Equal(expected.Children[x].BaseProperty, actual.Children[x].BaseProperty);//, "Save/load failed: data mismatch.");
                Assert.Equal(expected.Children[x].GetType(), actual.Children[x].GetType());//, "Save/load failed: type mismatch.");
            }
        }
    }
}
