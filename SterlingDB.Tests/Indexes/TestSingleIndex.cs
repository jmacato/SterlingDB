
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

using System.Collections.Generic;
using System.Linq;

using SterlingDB.Core;
using SterlingDB.Core.Indexes;
using SterlingDB.Core.Serialization;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Indexes
{
    [Ignore]
    
    public class TestSingleIndexAltDriver : TestSingleIndex
    {
        protected override ISterlingDriver GetDriver()
        {
#if NETFX_CORE
            return new WindowsStorageDriver() { DatabaseInstanceName = TestContext.TestName };
#elif SILVERLIGHT
            return new IsolatedStorageDriver() { DatabaseInstanceName = TestContext.TestName };
#elif AZURE_DRIVER
            return new SterlingDB.Server.Azure.TableStorage.Driver() { DatabaseInstanceName = TestContext.TestName };
#else
            return new FileSystemDriver() { DatabaseInstanceName = TestContext.TestName };
#endif
        }
    }

    [Ignore]
    
    public class TestSingleIndex : TestBase
    {
        private IndexCollection<TestModel, string, int> _target;
        private List<TestModel> _testModels;
        protected readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();
        private int _testAccessCount;
        private ISterlingDriver _driver;

        /// <summary>
        ///     Fetcher - also flag the fetch
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The model</returns>
        private TestModel _GetTestModelByKey(int key)
        {
            _testAccessCount++;
            return (from t in _testModels where t.Key.Equals(key) select t).FirstOrDefault();
        }

        

        
        public void Init()
        {
            _driver = GetDriver();

            _testModels = new List<TestModel>(3);
            for(var x = 0; x < 3; x++)
            {
                _testModels.Add(TestModel.MakeTestModel());
            }
            
            _testAccessCount = 0;                        
            _target = new IndexCollection<TestModel, string, int>("TestIndex",_driver,
                                                      tm => tm.Data , _GetTestModelByKey);
        }

        
        public void Cleanup()
        {
            _driver.PurgeAsync().Wait();
            _driver = null;
        }

        [Fact]
        public void TestAddIndex()
        {
            Assert.False(_target.IsDirty, "Dirty flag set prematurely");
            _target.AddIndexAsync(_testModels[0], _testModels[0].Key).Wait();
            Assert.True(_target.IsDirty, "Dirty flag not set on add.");            
        }

        [Fact]
        public void TestAddDuplicateIndex()
        {
            Assert.False(_target.IsDirty, "Dirty flag set prematurely");
            _target.AddIndexAsync(_testModels[0], _testModels[0].Key).Wait();
            Assert.True(_target.IsDirty, "Dirty flag not set on add.");
            Assert.Equal(_target.Query.Count(),1, "Index count is incorrect.");
            _target.AddIndexAsync(_testModels[0], _testModels[0].Key).Wait();
            Assert.Equal(_target.Query.Count(), 1, "Index count is incorrect.");            
        }

        [Fact]
        public void TestRemoveIndex()
        {
            Assert.False(_target.IsDirty, "Dirty flag set prematurely");
            _target.AddIndexAsync(_testModels[0], _testModels[0].Key).Wait();
            Assert.True(_target.IsDirty, "Dirty flag not set on add.");
            Assert.Equal(1, _target.Query.Count(), "Index count is incorrect.");
            _target.RemoveIndexAsync(_testModels[0].Key).Wait();
            Assert.Equal(0, _target.Query.Count(), "Index was not removed.");
        }

        [Fact]
        public void TestQueryable()
        {
            _target.AddIndexAsync(_testModels[0],_testModels[0].Key).Wait();
            _target.AddIndexAsync(_testModels[1], _testModels[1].Key).Wait();
            _target.AddIndexAsync(_testModels[2], _testModels[2].Key).Wait();
            Assert.Equal(3, _target.Query.Count(), "Key count is incorrect.");
            Assert.Equal(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testIndex = (from k in _target.Query where k.Index.Equals(_testModels[1].Data) select k).FirstOrDefault();
            Assert.NotNull(testIndex, "Test key not retrieved.");
            Assert.Equal(_testModels[1].Key, testIndex.Key, "Key mismatch.");
            Assert.Equal(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testModel = testIndex.Value.Result;
            Assert.Same(_testModels[1], testModel, "Model does not match.");
            Assert.Equal(1, _testAccessCount, "Lazy loader access count is incorrect.");
        }

        [Fact]
        public void TestSerialization()
        {
            _target.AddIndexAsync(_testModels[0], _testModels[0].Key).Wait();
            _target.AddIndexAsync(_testModels[1], _testModels[1].Key).Wait();            
            Assert.True(_target.IsDirty, "Dirty flag not set.");
            _target.FlushAsync().Wait();
            Assert.False(_target.IsDirty, "Dirty flag not reset on flush.");

            var secondTarget = new IndexCollection<TestModel, string, int>("TestIndex", _driver,
                                                                 tm => tm.Data,
                                                                 _GetTestModelByKey);

            // are we able to grab things?
            Assert.Equal(2, secondTarget.Query.Count(), "Key count is incorrect.");
            Assert.Equal(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testIndex = (from k in secondTarget.Query where k.Index.Equals(_testModels[1].Data) select k).FirstOrDefault();
            Assert.NotNull(testIndex, "Test index not retrieved.");
            Assert.Equal(_testModels[1].Key, testIndex.Key, "Key mismatch.");
            Assert.Equal(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testModel = testIndex.Value.Result;
            Assert.Same(_testModels[1], testModel, "Model does not match.");
            Assert.Equal(1, _testAccessCount, "Lazy loader access count is incorrect.");

            // now let's test refresh
            secondTarget.AddIndexAsync(_testModels[2],_testModels[2].Key).Wait();
            secondTarget.FlushAsync().Wait();

            Assert.Equal(2, _target.Query.Count(), "Unexpected key count in original collection.");
            _target.RefreshAsync().Wait();
            Assert.Equal(3, _target.Query.Count(), "Refresh failed.");

        }
    }
}
