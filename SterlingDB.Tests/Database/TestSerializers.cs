using SterlingDB;
using SterlingDB.Server.FileSystem;
using SterlingDB.Test.Helpers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SterlingDB.Exceptions;

namespace SterlingDB.Test.Database
{
    public class TestSerializers : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestSerializers()
        {
            _engine = Factory.NewEngine();
            _engine.SterlingDatabase.RegisterSerializer<TestSerializer>();
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
            var expected = TestClassWithStruct.MakeTestClassWithStruct();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithStruct>(key).Result;
            Assert.NotNull(actual); //Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID); //Save/load failed: key mismatch.");
            Assert.NotNull(actual.Structs); //Save/load failed: list not initialized.");
            Assert.Equal(expected.Structs.Count, actual.Structs.Count); //Save/load failed: list size mismatch.");
            Assert.Equal(expected.Structs[0].Date, actual.Structs[0].Date); //Save/load failed: date mismatch.");
            Assert.Equal(expected.Structs[0].Value, actual.Structs[0].Value); //Save/load failed: value mismatch.");
        }
    }
}
