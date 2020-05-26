using SterlingDB;
using SterlingDB.Database;
using Xunit;
using System.Collections.Generic;

namespace SterlingDB.Test.Database
{
    public enum TestEnums : short
    {
        Value1,
        Value2,
        Value3
    }

    public enum TestEnumsLong : long
    {
        LongValue = 500,
        LongerValue = 1000
    }

    public class EnumClass
    {
        public int Id { get; set; }
        public TestEnums Value { get; set; }
        public TestEnumsLong ValueLong { get; set; }
    }

    public class EnumDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<EnumClass, int>(e => e.Id)
                           };
        }
    }
    
    public class TestEnum : TestBase
    {
        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        
        public TestEnum()
        {           
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<EnumDatabase>( TestContext.TestName, GetDriver() );
            _databaseInstance.PurgeAsync().Wait();
        }

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [Fact]
        public void TestEnumSaveAndLoad()
        {
            var test = new EnumClass() { Id = 1, Value = TestEnums.Value2, ValueLong = TestEnumsLong.LongerValue };
            _databaseInstance.SaveAsync( test ).Wait();
            var actual = _databaseInstance.LoadAsync<EnumClass>( 1 ).Result;
            Assert.Equal(test.Id, actual.Id); //Failed to load enum: key mismatch.");
            Assert.Equal(test.Value, actual.Value); //Failed to load enum: value mismatch.");
            Assert.Equal(test.ValueLong, actual.ValueLong); //Failed to load enum: value mismatch.");
        }

        [Fact]
        public void TestMultipleEnumSaveAndLoad()
        {
            var test1 = new EnumClass { Id = 1, Value = TestEnums.Value1, ValueLong = TestEnumsLong.LongValue };
            var test2 = new EnumClass { Id = 2, Value = TestEnums.Value2, ValueLong = TestEnumsLong.LongerValue };

            _databaseInstance.SaveAsync( test1 ).Wait();
            _databaseInstance.SaveAsync( test2 ).Wait();

            var actual1 = _databaseInstance.LoadAsync<EnumClass>( 1 ).Result;
            var actual2 = _databaseInstance.LoadAsync<EnumClass>( 2 ).Result;

            Assert.Equal(test1.Id, actual1.Id); //Failed to load enum: key 1 mismatch.");
            Assert.Equal(test1.Value, actual1.Value); //Failed to load enum: value 1 mismatch.");
            Assert.Equal(test1.ValueLong, actual1.ValueLong); //Failed to load enum: value 1 mismatch.");

            Assert.Equal(test2.Id, actual2.Id); //Failed to load enum: key 2 mismatch.");
            Assert.Equal(test2.Value, actual2.Value); //Failed to load enum: value 2 mismatch.");
            Assert.Equal(test2.ValueLong, actual2.ValueLong); //Failed to load enum: value 2 mismatch.");
        }
    }
}