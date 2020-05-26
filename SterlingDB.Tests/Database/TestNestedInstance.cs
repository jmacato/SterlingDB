using SterlingDB;
using SterlingDB.Database;
using SterlingDB.Server.FileSystem;
using Xunit;
using System;
using System.Linq;

using System.Collections.Generic;

namespace SterlingDB.Test.Database
{
    public abstract class BaseNested
    {
        public Guid Id { get; set; }
    }

    public class Bill : BaseNested
    {
        public Bill()
        {
            Partakers = new List<Partaker>();
        }

        public string Name { get; set; }
        public List<Partaker> Partakers { get; set; }
        public double Total { get; set; }
    }

    public class Person : BaseNested
    {
        public string Name { get; set; }
    }

    public class Partaker : BaseNested
    {
        public double Paid { get; set; }
        public Person Person { get; set; }
    }

    public class NestedInstancesDatabase : BaseDatabaseInstance
    {       
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
        {
            CreateTableDefinition<Bill, Guid>( b => b.Id ),
            CreateTableDefinition<Person, Guid>( p => p.Id )
        };
        }
    }
 
    public class TestNestedInstance : TestBase
    {
        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _database;
        
        public TestNestedInstance()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _database = _engine.SterlingDatabase.RegisterDatabase<NestedInstancesDatabase>(TestContext.TestName, GetDriver());
        }
        
        public override void Cleanup()
        {
            if (_engine == null) return;

            _engine.Dispose();
            _database = null;
        }

        [Fact]
        public void TestAddBill()
        {
            _database.PurgeAsync().Wait();

            var bill = new Bill
                           {
                Id = Guid.NewGuid(),
                Name = "Test"
            };

            _database.SaveAsync( bill ).Wait();
            
            var person1 = new Person
                              {
                Id = Guid.NewGuid(),
                Name = "Martin"
            };

            _database.SaveAsync( person1 ).Wait();

            var partaker1 = new Partaker
                                {
                Id = Guid.NewGuid(),
                Paid = 42,
                Person = person1
            };

            bill.Partakers.Add(partaker1);

            _database.SaveAsync( bill ).Wait();

            var person2 = new Person
                              {
                Id = Guid.NewGuid(),
                Name = "Jeremy"
            };

            _database.SaveAsync( person2 ).Wait();
            
            var partaker2 = new Partaker
                                {
                Id = Guid.NewGuid(),
                Paid = 0,
                Person = person2
            };

            bill.Partakers.Add(partaker2);

            _database.SaveAsync( bill ).Wait();

            var partaker3 = new Partaker()
                                {
                                    Id = Guid.NewGuid(),
                                    Paid = 1,
                                    Person = person1
                                };

            bill.Partakers.Add(partaker3);

            _database.SaveAsync( bill ).Wait();

            _database.FlushAsync().Wait();
            
            var billKeys = _database.Query<Bill, Guid>();

            Assert.True(billKeys.Count == 1);
            Assert.Equal(billKeys[0].Key, bill.Id);

            var freshBill = billKeys[0].LazyValue.Value;

            Assert.True(freshBill.Partakers.Count == 3); //Bill should have exactly 3 partakers.");            

            var personKeys = _database.Query<Person, Guid>();

            Assert.True(personKeys.Count == 2); //Failed to save exactly 2 persons.");            
            
            // Compare loaded instances and verify they are equal 
            var persons = (from p in freshBill.Partakers where p.Person.Id.Equals(person1.Id) select p.Person).ToList();

            // should be two of these
            Assert.Equal(2, persons.Count); //Failed to grab two instances of the same person.");
            Assert.Equal(persons[0], persons[1]); //Instances were not equal.");
        }
    }
}
