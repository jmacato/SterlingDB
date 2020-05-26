
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
    [Tag("Serializers")]
#endif
    [TestClass]
    public class TestSerializersAltDriver : TestSerializers
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
    [Tag("Serializers")]
#endif 
    [TestClass]
    public class TestSerializers : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestContext TestContext { get; set; }

        
        public void TestInit()
        {
            _engine = Factory.NewEngine();
            _engine.SterlingDatabase.RegisterSerializer<TestSerializer>();
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
            var expected = TestClassWithStruct.MakeTestClassWithStruct();
            var key = _databaseInstance.SaveAsync( expected ).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithStruct>(key).Result;
            Assert.NotNull(actual, "Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.NotNull(actual.Structs, "Save/load failed: list not initialized.");
            Assert.Equal(expected.Structs.Count, actual.Structs.Count, "Save/load failed: list size mismatch.");
            Assert.Equal(expected.Structs[0].Date, actual.Structs[0].Date, "Save/load failed: date mismatch.");
            Assert.Equal(expected.Structs[0].Value, actual.Structs[0].Value, "Save/load failed: value mismatch.");
        }
    }
}
