using System.Collections.Generic;
using SterlingDB.Database;
using Xunit;

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


    public class TestInterfaceProperty : TestBase
    {
        public TestInterfaceProperty()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<InterfaceDatabase>(TestContext.TestName, GetDriver());
            _databaseInstance.PurgeAsync().Wait();
        }

        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public void TestInterface()
        {
            var test = new TargetClass {Id = 1, SubInterface = new InterfaceClass {Id = 5, Value = 6}};

            _databaseInstance.SaveAsync(test).Wait();

            var actual = _databaseInstance.LoadAsync<TargetClass>(1).Result;

            Assert.Equal(test.Id, actual.Id); //Failed to load class with interface property: key mismatch.");
            Assert.NotNull(test
                .SubInterface); //Failed to load class with interface property: interface property is null.");
            Assert.Equal(test.SubInterface.Id,
                actual.SubInterface.Id); //Failed to load class with interface property: interface id mismatch.");
            Assert.Equal(test.SubInterface.Value,
                actual.SubInterface
                    .Value); //Failed to load class with interface property: value mismatch.");            
        }
    }
}