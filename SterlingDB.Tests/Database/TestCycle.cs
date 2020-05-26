using SterlingDB;
using SterlingDB.Database;
using Xunit;
using System.Collections.Generic;

namespace SterlingDB.Test.Database
{
    public class CycleClass
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public CycleClass ChildCycle { get; set; }
    }

    public class CycleDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<CycleClass, int>(n => n.Id)
                           };
        }
    }

    public class TestCycle : TestBase
    {
        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public TestCycle()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<CycleDatabase>(TestContext.TestName, GetDriver());
            _databaseInstance.PurgeAsync().Wait();
        }

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public void TestCycleNegativeCase()
        {
            var test = new CycleClass { Id = 1, Value = 1 };
            var child = new CycleClass { Id = 2, Value = 5 };
            test.ChildCycle = child;

            _databaseInstance.SaveAsync(test).Wait();
            var actual = _databaseInstance.LoadAsync<CycleClass>(1).Result;
            Assert.Equal(test.Id, actual.Id); //Failed to load cycle with non-null child: key mismatch.");
            Assert.Equal(test.Value, actual.Value); //Failed to load cycle with non-null child: value mismatch.");
            Assert.NotNull(test.ChildCycle); //Failed to load cycle with non-null child: child is null.");
            Assert.Equal(child.Id, actual.ChildCycle.Id); //Failed to load cycle with non-null child: child key mismatch.");
            Assert.Equal(child.Value, actual.ChildCycle.Value); //Failed to load cycle with non-null child: value mismatch.");

            actual = _databaseInstance.LoadAsync<CycleClass>(2).Result;
            Assert.Equal(child.Id, actual.Id); //Failed to load cycle with non-null child: key mismatch on direct child load.");
            Assert.Equal(child.Value, actual.Value); //Failed to load cycle with non-null child: value mismatch on direct child load.");
        }

        [Fact]
        public void TestCyclePositiveCase()
        {
            var test = new CycleClass { Id = 1, Value = 1 };
            var child = new CycleClass { Id = 2, Value = 5 };
            test.ChildCycle = child;
            child.ChildCycle = test; // this creates our cycle condition

            _databaseInstance.SaveAsync(test).Wait();
            var actual = _databaseInstance.LoadAsync<CycleClass>(1).Result;
            Assert.Equal(test.Id, actual.Id); //Failed to load cycle with non-null child: key mismatch.");
            Assert.Equal(test.Value, actual.Value); //Failed to load cycle with non-null child: value mismatch.");
            Assert.NotNull(test.ChildCycle); //Failed to load cycle with non-null child: child is null.");
            Assert.Equal(child.Id, actual.ChildCycle.Id); //Failed to load cycle with non-null child: child key mismatch.");
            Assert.Equal(child.Value, actual.ChildCycle.Value); //Failed to load cycle with non-null child: value mismatch.");

            actual = _databaseInstance.LoadAsync<CycleClass>(2).Result;
            Assert.Equal(child.Id, actual.Id); //Failed to load cycle with non-null child: key mismatch on direct child load.");
            Assert.Equal(child.Value, actual.Value); //Failed to load cycle with non-null child: value mismatch on direct child load.");
        }

    }
}