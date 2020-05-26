using System.IO;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestBackupRestore : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public void TestBackupAndRestore()
        {
            var driver = GetDriver();

            // activate the engine and store the data
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, driver);

            // test saving and reloading
            var expected = TestModel.MakeTestModel();

            _databaseInstance.SaveAsync(expected).Wait();

            // now back it up
            var memStream = new MemoryStream();

            byte[] databaseBuffer;

            using (var binaryWriter = new BinaryWriter(memStream))
            {
                _engine.SterlingDatabase.BackupAsync<TestDatabaseInstance>(binaryWriter).Wait();
                binaryWriter.Flush();
                databaseBuffer = memStream.ToArray();
            }

            // now purge the database
            _databaseInstance.PurgeAsync().Wait();

            var actual = _databaseInstance.LoadAsync<TestModel>(expected.Key).Result;

            // confirm the database is gone
            Assert.Null(actual); //, "Purge failed, was able to load the test model.");

            _databaseInstance = null;

            // shut it all down
            _engine.Dispose();
            _engine = null;

            // get a new engine
            _engine = Factory.NewEngine();

            // activate it and grab the database again
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, driver);

            // restore it
            _engine.SterlingDatabase
                .RestoreAsync<TestDatabaseInstance>(new BinaryReader(new MemoryStream(databaseBuffer))).Wait();

            _engine.Dispose();
            _engine = null;

            // get a new engine
            _engine = Factory.NewEngine();

            // activate it and grab the database again
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, driver);

            actual = _databaseInstance.LoadAsync<TestModel>(expected.Key).Result;

            Assert.NotNull(actual); //, "Load failed.");
            Assert.Equal(expected.Key, actual.Key); //, "Load failed: key mismatch.");
            Assert.Equal(expected.Data, actual.Data); //, "Load failed: data mismatch.");
            Assert.Null(actual.Data2); //, "Load failed: suppressed data property not valid on de-serialize.");
            Assert.NotNull(actual.SubClass); //, "Load failed: sub class is null.");
            Assert.Null(actual.SubClass2); //, "Load failed: supressed sub class should be null.");
            Assert.Equal(expected.SubClass.NestedText,
                actual.SubClass.NestedText); //,"Load failed: sub class text mismtach.");
            Assert.Equal(expected.SubStruct.NestedId,
                actual.SubStruct.NestedId); //"Load failed: sub struct id mismtach.");
            Assert.Equal(expected.SubStruct.NestedString,
                actual.SubStruct.NestedString); //,"Load failed: sub class string mismtach.");
        }
    }
}