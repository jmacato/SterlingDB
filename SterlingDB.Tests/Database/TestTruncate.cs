
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

using System.Linq;

using SterlingDB.Test.Helpers;
using SterlingDB.Core;

namespace SterlingDB.Test.Database
{
#if SILVERLIGHT
    [Tag("Truncate")]
#endif
    [TestClass]
    public class TestTruncateAltDriver : TestTruncate
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
    [Tag("Truncate")]
#endif
    [TestClass]
    public class TestTruncate : TestBase
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
        public void TestTruncateAction()
        {
            // save a few objects
            var sample = TestModel.MakeTestModel();
            _databaseInstance.SaveAsync( sample ).Wait();
            _databaseInstance.SaveAsync( TestModel.MakeTestModel() ).Wait();
            _databaseInstance.SaveAsync( TestModel.MakeTestModel() ).Wait();

            _databaseInstance.TruncateAsync(typeof(TestModel)).Wait();

            // query should be empty
            Assert.False(_databaseInstance.Query<TestModel,int>().Any(), "Truncate failed: key list still exists.");

            // load should be empty
            var actual = _databaseInstance.LoadAsync<TestModel>( sample.Key ).Result;

            Assert.Null(actual, "Truncate failed: was able to load item.");            
        }       
    }
}