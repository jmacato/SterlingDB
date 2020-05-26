using System.Collections.Generic;
using SterlingDB.Database;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestObjectField
    {
        public string Data;
        public int Key;
    }

    public class TestObjectFieldDatabase : BaseDatabaseInstance
    {
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
            {
                CreateTableDefinition<TestObjectField, int>(dataDefinition => dataDefinition.Key)
            };
        }
    }

    public class TestField : TestBase
    {
        public TestField()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestObjectFieldDatabase>(TestContext.TestName, GetDriver());
            _databaseInstance.PurgeAsync().Wait();
        }

        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public void TestData()
        {
            var testNull = new TestObjectField {Key = 1, Data = "data"};

            _databaseInstance.SaveAsync(testNull).Wait();

            var loadedTestNull = _databaseInstance.LoadAsync<TestObjectField>(1).Result;

            // The values in the deserialized class should be populated.
            Assert.NotNull(loadedTestNull);
            Assert.Equal("data", loadedTestNull.Data);
            Assert.Equal(1, loadedTestNull.Key);
        }
    }
}