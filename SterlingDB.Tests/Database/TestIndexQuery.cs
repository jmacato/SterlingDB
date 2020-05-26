
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
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Database
{
#if SILVERLIGHT
    [Tag("IndexQuery")]
#endif
    [TestClass]
    public class TestIndexQueryAltDriver : TestIndexQuery
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
    [Tag("IndexQuery")]
#endif
    [TestClass]
    public class TestIndexQuery : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private List<TestModel> _modelList;

        public TestContext TestContext { get; set; }

        
        public void TestInit()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>( TestContext.TestName, GetDriver() );
            _modelList = new List<TestModel>();
            for (var i = 0; i < 10; i++)
            {
                _modelList.Add(TestModel.MakeTestModel());
                _databaseInstance.SaveAsync(_modelList[i]).Wait();
            }
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [Fact]
        public void TestSequentialQuery()
        {
            // set up queries
            var sequential = from k in _databaseInstance.Query<TestModel, string, int>(TestDatabaseInstance.DATAINDEX) orderby k.Index select k.Key;

            _modelList.Sort((m1,m2)=>m1.Data.CompareTo(m2.Data));

            var idx = 0;
            foreach (var key in sequential)
            {
                Assert.Equal(_modelList[idx++].Key, key, "Sequential query failed: key mismatch.");
            }
            Assert.Equal(idx, _modelList.Count, "Error in query: wrong number of rows.");
        }

        [Fact]
        public void TestDescendingQuery()
        {
            var descending = from k in _databaseInstance.Query<TestModel, string, int>(TestDatabaseInstance.DATAINDEX) orderby k.Index descending select k.Key;

            _modelList.Sort((m1,m2)=>m2.Data.CompareTo(m1.Data));

            var idx = 0;
            foreach (var key in descending)
            {
                Assert.Equal(_modelList[idx++].Key, key, "Descending query failed: key mismatch.");
            }
            Assert.Equal(idx, _modelList.Count, "Error in query: wrong number of rows.");
        }        

        [Fact]
        public void TestUnrolledQuery()
        {
            _modelList.Sort((m1, m2) => m1.Date.CompareTo(m2.Date));
            var unrolled = from k in _databaseInstance.Query<TestModel, string, int>(TestDatabaseInstance.DATAINDEX) 
                           orderby k.Value.Result.Date select k.Value.Result;

            var idx = 0;

            foreach (var model in unrolled)
            {
                Assert.Equal(_modelList[idx].Key, model.Key, "Unrolled query failed: key mismatch.");
                Assert.Equal(_modelList[idx].Date, model.Date, "Unrolled query failed: date mismatch.");
                Assert.Equal(_modelList[idx].Data, model.Data, "Unrolled query failed: data mismatch.");
                idx++;
            }
        }
    }
}
