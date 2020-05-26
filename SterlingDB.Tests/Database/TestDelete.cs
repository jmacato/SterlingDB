using SterlingDB.Core;
using SterlingDB.Test.Helpers;
using Xunit;
using System;
using System.Linq;

namespace SterlingDB.Test.Database
{
    public class TestDelete : TestBase
    {
        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestDelete()
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
        public void TestDatabaseDeleteByInstance()
        {
            var testModel = TestModel.MakeTestModel();

            Func<int> countKeys = () => _databaseInstance.Query<TestModel, int>().Count();
            Func<int> countIndex =
                () => _databaseInstance.Query<TestModel, string, int>(TestDatabaseInstance.DATAINDEX).Count();

            Assert.Equal(0, countKeys()); //,"Database initialized with invalid key count.");
            Assert.Equal(0, countIndex()); //Database initialized with invalid index count.");

            var key = _databaseInstance.SaveAsync(testModel).Result;

            Assert.Equal(1, countKeys()); //Keys not updated with save.");
            Assert.Equal(1, countIndex()); //Index count not updated with save.");

            var actual = _databaseInstance.LoadAsync<TestModel>(key).Result;

            Assert.NotNull(actual); //Test model did not re-load.");

            _databaseInstance.DeleteAsync(actual).Wait();

            Assert.Equal(0, countKeys()); //Database updated with invalid key count after delete.");
            Assert.Equal(0, countIndex()); //Database updated with invalid index count after delete.");

            actual = _databaseInstance.LoadAsync<TestModel>(key).Result;

            Assert.Null(actual); //Delete failed: loaded actual value after delete.");
        }

        [Fact]
        public void TestDatabaseDeleteByKey()
        {
            var testModel = TestModel.MakeTestModel();

            Func<int> countKeys = () => _databaseInstance.Query<TestModel, int>().Count();
            Func<int> countIndex =
                () => _databaseInstance.Query<TestModel, string, int>(TestDatabaseInstance.DATAINDEX).Count();

            Assert.Equal(0, countKeys()); //Database initialized with invalid key count.");
            Assert.Equal(0, countIndex()); //Database initialized with invalid index count.");

            var key = _databaseInstance.SaveAsync(testModel).Result;

            Assert.Equal(1, countKeys()); //Keys not updated with save.");
            Assert.Equal(1, countIndex()); //Index count not updated with save.");

            var actual = _databaseInstance.LoadAsync<TestModel>(key).Result;

            Assert.NotNull(actual); //Test model did not re-load.");

            _databaseInstance.DeleteAsync(typeof(TestModel), key).Wait();

            Assert.Equal(0, countKeys()); //Database updated with invalid key count after delete.");
            Assert.Equal(0, countIndex()); //Database updated with invalid index count after delete.");

            actual = _databaseInstance.LoadAsync<TestModel>(key).Result;

            Assert.Null(actual); //Delete failed: loaded actual value after delete.");
        }
    }
}
