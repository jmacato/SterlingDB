
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
using SterlingDB.Core.Database;

namespace SterlingDB.Test.Database
{
    public class TestObjectField
    {
        public int Key;
        public string Data;
    }

    public class TestObjectFieldDatabase : BaseDatabaseInstance
    {
        protected override System.Collections.Generic.List<ITableDefinition> RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<TestObjectField,int>(dataDefinition => dataDefinition.Key)
            };
        }
    }

#if SILVERLIGHT
    [Tag("Field")]
    [Tag("Database")]
#endif
    [TestClass]
    public class TestFieldAltDriver : TestField
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
    [Tag("Field")]
    [Tag("Database")]
#endif
    [TestClass]
    public class TestField : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestContext TestContext { get; set; }

        
        public void TestInit()
        {            
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestObjectFieldDatabase>( TestContext.TestName, GetDriver() );
            _databaseInstance.PurgeAsync().Wait();
        }

        [Fact]
        public void TestData()
        {
            var testNull = new TestObjectField {Key = 1, Data = "data"};

            _databaseInstance.SaveAsync( testNull ).Wait();

            var loadedTestNull = _databaseInstance.LoadAsync<TestObjectField>( 1 ).Result;

            // The values in the deserialized class should be populated.
            Assert.NotNull(loadedTestNull);
            Assert.NotNull(loadedTestNull.Data);
            Assert.NotNull(loadedTestNull.Key);
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }

    }

}