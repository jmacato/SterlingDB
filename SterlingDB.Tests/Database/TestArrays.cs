using SterlingDB;
using SterlingDB.Server.FileSystem;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestArrays : TestBase
    {
        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestArrays()
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
        public void TestNullArray()
        {
            var expected = TestClassWithArray.MakeTestClassWithArray();
            expected.BaseClassArray = null;
            expected.ClassArray = null;
            expected.ValueTypeArray = null;
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithArray>(key).Result;

            Assert.NotNull(actual);//, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID);//, "Save/load failed: key mismatch.");
            Assert.Null(actual.BaseClassArray);//, "Save/load: array should be null");
            Assert.Null(actual.ClassArray);//, "Save/load: array should be null");
            Assert.Null(actual.ValueTypeArray);//, "Save/load: array should be null");            
        }

        [Fact]
        public void TestArray()
        {
            var expected = TestClassWithArray.MakeTestClassWithArray();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithArray>(key).Result;

            Assert.NotNull(actual);//, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID);//, "Save/load failed: key mismatch.");
            Assert.NotNull(actual.BaseClassArray);//, "Save/load failed: array not initialized.");
            Assert.NotNull(actual.ClassArray);//, "Save/load failed: array not initialized.");
            Assert.NotNull(actual.ValueTypeArray);//, "Save/load failed: array not initialized.");
            Assert.Equal(expected.BaseClassArray.Length, actual.BaseClassArray.Length);//, "Save/load failed: array size mismatch.");
            Assert.Equal(expected.ClassArray.Length, actual.ClassArray.Length);//, "Save/load failed: array size mismatch.");
            Assert.Equal(expected.ValueTypeArray.Length, actual.ValueTypeArray.Length);//, "Save/load failed: array size mismatch.");

            for (var x = 0; x < expected.BaseClassArray.Length; x++)
            {
                Assert.Equal(expected.BaseClassArray[x].Key, actual.BaseClassArray[x].Key);//, "Save/load failed: key mismatch.");
                Assert.Equal(expected.BaseClassArray[x].BaseProperty, actual.BaseClassArray[x].BaseProperty);//, "Save/load failed: data mismatch.");
            }

            for (var x = 0; x < expected.ClassArray.Length; x++)
            {
                Assert.Equal(expected.ClassArray[x].Key, actual.ClassArray[x].Key);//, "Save/load failed: key mismatch.");
                Assert.Equal(expected.ClassArray[x].Data, actual.ClassArray[x].Data);//, "Save/load failed: data mismatch.");
            }

            for (var x = 0; x < expected.ValueTypeArray.Length; x++)
            {
                Assert.Equal(expected.ValueTypeArray[x], actual.ValueTypeArray[x]);//, "Save/load failed: value mismatch.");
            }
        }
    }
}
