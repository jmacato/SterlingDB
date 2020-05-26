using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Server.FileSystem;
using SterlingDB.Test.Helpers;
using Xunit;
using System.Collections.Generic;

using SterlingDB.Core;
using SterlingDB.Core.Database;

namespace SterlingDB.Test.Database
{
    public class NullableClass
    {
        public int Id { get; set; }
        public int? Value { get; set; }
    }

    public class NullableDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<NullableClass, int>(n => n.Id)
                           };
        }
    }
 
    public class TestNullable : TestBase
    {                
        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public void TestInit()
        {            
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<NullableDatabase>( TestContext.TestName, GetDriver() );
            _databaseInstance.PurgeAsync().Wait();
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [Fact]
        public void TestNotNull()
        {
            var test = new NullableClass {Id = 1, Value = 1};
            _databaseInstance.SaveAsync( test ).Wait();
            var actual = _databaseInstance.LoadAsync<NullableClass>( 1 ).Result;
            Assert.Equal(test.Id, actual.Id, "Failed to load nullable with nullable set: key mismatch.");
            Assert.Equal(test.Value, actual.Value, "Failed to load nullable with nullable set: value mismatch.");
        }

        [Fact]
        public void TestNull()
        {
            var test = new NullableClass { Id = 1, Value = null };
            _databaseInstance.SaveAsync( test ).Wait();
            var actual = _databaseInstance.LoadAsync<NullableClass>( 1 ).Result;
            Assert.Equal(test.Id, actual.Id, "Failed to load nullable with nullable set: key mismatch.");
            Assert.Null(actual.Value, "Failed to load nullable with nullable set: value mismatch.");
        }
    }
}