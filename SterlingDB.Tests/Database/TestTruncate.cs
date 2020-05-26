using System.Linq;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestTruncate : TestBase
    {
        public TestTruncate()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
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
        public void TestTruncateAction()
        {
            // save a few objects
            var sample = TestModel.MakeTestModel();
            _databaseInstance.SaveAsync(sample).Wait();
            _databaseInstance.SaveAsync(TestModel.MakeTestModel()).Wait();
            _databaseInstance.SaveAsync(TestModel.MakeTestModel()).Wait();

            _databaseInstance.TruncateAsync(typeof(TestModel)).Wait();

            // query should be empty
            Assert.False(_databaseInstance.Query<TestModel, int>().Any()); //Truncate failed: key list still exists.");

            // load should be empty
            var actual = _databaseInstance.LoadAsync<TestModel>(sample.Key).Result;

            Assert.Null(actual); //Truncate failed: was able to load item.");            
        }
    }
}