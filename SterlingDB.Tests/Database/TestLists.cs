
#if NETFX_CORE
using SterlingDB.WinRT.WindowsStorage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif SILVERLIGHT
using Microsoft.Phone.Testing;
using SterlingDB.WP8.IsolatedStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using SterlingDB.Server.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

using SterlingDB.Core;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Database
{
#if SILVERLIGHT
    [Tag("List")]
#endif
    [TestClass]
    public class TestListsAltDriver : TestLists
    {
        protected override ISterlingDriver GetDriver()
        {
#if NETFX_CORE
            return new WindowsStorageDriver();
#elif SILVERLIGHT
            return new IsolatedStorageDriver();
#elif AZURE_DRIVER
            return new SterlingDB.Server.Azure.TableStorage.Driver();
#else
            return new FileSystemDriver();
#endif
        }
    }

#if SILVERLIGHT 
    [Tag("List")]
#endif
    [TestClass]
    public class TestLists : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestContext TestContext { get; set; }

        
        public void TestInit()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>( TestContext.TestName, GetDriver() );
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
            var expected = TestListModel.MakeTestListModel();
            expected.Children = null;
            var key = _databaseInstance.SaveAsync( expected ).Result;
            var actual = _databaseInstance.LoadAsync<TestListModel>(key).Result;
            Assert.NotNull(actual, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.Null(actual.Children, "Save/load failed: list should be null.");            
        }

        [Fact]
        public void TestEmptyList()
        {
            var expected = TestListModel.MakeTestListModel();
            expected.Children.Clear();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestListModel>(key).Result;
            Assert.NotNull(actual, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.NotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.Equal(0, actual.Children.Count, "Save/load failed: list size mismatch.");            
        }

        [Fact]
        public void TestList()
        {
            var expected = TestListModel.MakeTestListModel();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestListModel>(key).Result;
            Assert.NotNull(actual, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.NotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.Equal(expected.Children.Count, actual.Children.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Children.Count; x++)
            {
                Assert.Equal(expected.Children[x].Key, actual.Children[x].Key, "Save/load failed: key mismatch.");
                Assert.Equal(expected.Children[x].Data, actual.Children[x].Data, "Save/load failed: data mismatch.");                
            }
        }

        [Fact]
        public void TestModelAsList()
        {
            var expected = TestModelAsListModel.MakeTestModelAsList();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestModelAsListModel>( key ).Result;
            Assert.NotNull(actual, "Save/load failed: model is null.");
            Assert.Equal(expected.Id, actual.Id, "Save/load failed: key mismatch.");
            Assert.Equal(expected.Count, actual.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Count; x++)
            {
                Assert.Equal(expected[x].Key, actual[x].Key, "Save/load failed: key mismatch.");
                Assert.Equal(expected[x].Data, actual[x].Data, "Save/load failed: data mismatch.");
            }
        }

        [Fact]
        public void TestModelAsListWithParentReference()
        {
            var expected = TestModelAsListModel.MakeTestModelAsListWithParentReference();
            var key = _databaseInstance.SaveAsync( expected ).Result;

            var actual = _databaseInstance.LoadAsync<TestModelAsListModel>( key ).Result;
            Assert.NotNull(actual, "Save/load failed: model is null.");
            Assert.Equal(expected.Id, actual.Id, "Save/load failed: key mismatch.");
            Assert.Equal(expected.Count, actual.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Count; x++)
            {
                Assert.Equal(expected[x].Key, actual[x].Key, "Save/load failed: key mismatch.");
                Assert.Equal(expected[x].Data, actual[x].Data, "Save/load failed: data mismatch.");
                Assert.Equal(expected, expected[x].Parent, "Parent doesn't match");
            }
        }

    }
}