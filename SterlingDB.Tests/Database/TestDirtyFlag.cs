using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Server.FileSystem;
using SterlingDB.Test.Helpers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Database
{
    public class DirtyDatabase : BaseDatabaseInstance
    {
        public Predicate<TestModel> Predicate { get; set; }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TestListModel, int>(t=>t.ID),
                               CreateTableDefinition<TestModel, int>(t=>t.Key)
                               .WithDirtyFlag<TestModel,int>(o=>this.Predicate(o))
                           };
        }
    }

#if SILVERLIGHT
    [Tag("Dirty")]
    [Tag("Database")]
#endif
    
    public class TestDirtyFlagAltDriver : TestDirtyFlag
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
    [Tag("Dirty")]
    [Tag("Database")]
#endif
    
    public class TestDirtyFlag : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        

        public TestDirtyFlag()
        {
        }

        
        public void TestInit()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<DirtyDatabase>( TestContext.TestName, GetDriver() );
            ( (DirtyDatabase) _databaseInstance ).Predicate = model => true;
            _databaseInstance.PurgeAsync().Wait();
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [Fact]
        public void TestDirtyFlagFalse()
        {
            var expected = TestListModel.MakeTestListModel();

            // first save is to generate the keys
            var key = _databaseInstance.SaveAsync( expected ).Result;

            var actual = _databaseInstance.LoadAsync<TestListModel>( key ).Result;

            foreach(var model in actual.Children)
            {
                model.ResetAccess();
            }

            ( (DirtyDatabase) _databaseInstance ).Predicate = model => true;

            // now check that all were accessed
            _databaseInstance.SaveAsync( actual ).Wait();

            var accessed = (from t in actual.Children where !t.Accessed select 1).Any();

            Assert.False(accessed, "Dirty flag on save failed: some children were not accessed.");
        }

        [Fact]
        public void TestDirtyFlagTrue()
        {
            var expected = TestListModel.MakeTestListModel();

            // first save is to generate the keys
            var key = _databaseInstance.SaveAsync( expected ).Result;

            var actual = _databaseInstance.LoadAsync<TestListModel>( key ).Result;

            foreach (var model in actual.Children)
            {
                model.ResetAccess();
            }

            ( (DirtyDatabase) _databaseInstance ).Predicate = model => false;

            // now check that none were accessed
            _databaseInstance.SaveAsync( actual ).Wait();

            var accessed = (from t in actual.Children where t.Accessed select 1).Any();

            Assert.False(accessed, "Dirty flag on save failed: some children were accessed.");
        }

    }
}