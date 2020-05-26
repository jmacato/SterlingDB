
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

using SterlingDB.Core;
using SterlingDB.Core.Database;

namespace SterlingDB.Test.Database
{
    public interface IInterface
    {
        int Id { get; }
        int Value { get; }
    }

    public class InterfaceClass : IInterface
    {
        public int Id { get; set; }
        public int Value { get; set; }        
    }

    public class TargetClass
    {
        public int Id { get; set; }
        public IInterface SubInterface { get; set; }
    }

    public class InterfaceDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TargetClass, int>(n => n.Id)
                           };
        }
    }

#if SILVERLIGHT
    [Tag("Interface")]
    [Tag("Database")]
#endif
    [TestClass]
    public class TestInterfacePropertyAltDriver : TestInterfaceProperty
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
    [Tag("Interface")]
    [Tag("Database")]
#endif
    [TestClass]
    public class TestInterfaceProperty : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestContext TestContext { get; set; }

        
        public void TestInit()
        {            
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<InterfaceDatabase>( TestContext.TestName, GetDriver() );
            _databaseInstance.PurgeAsync().Wait();
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [Fact]
        public void TestInterface()
        {
            var test = new TargetClass { Id = 1, SubInterface = new InterfaceClass { Id = 5, Value = 6 }};

            _databaseInstance.SaveAsync( test ).Wait();

            var actual = _databaseInstance.LoadAsync<TargetClass>( 1 ).Result;
            
            Assert.Equal(test.Id, actual.Id, "Failed to load class with interface property: key mismatch.");
            Assert.NotNull(test.SubInterface, "Failed to load class with interface property: interface property is null.");
            Assert.Equal(test.SubInterface.Id, actual.SubInterface.Id, "Failed to load class with interface property: interface id mismatch.");
            Assert.Equal(test.SubInterface.Value, actual.SubInterface.Value, "Failed to load class with interface property: value mismatch.");            
        }       
    }
}