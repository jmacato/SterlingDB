using System;
using System.Collections.Generic;
using System.Linq;
using SterlingDB.Database;
using SterlingDB.Exceptions;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TriggerClass
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public bool IsDirty { get; set; }
    }

    public class TriggerClassTestTrigger : BaseSterlingTrigger<TriggerClass, int>
    {
        public const int BADSAVE = 5;
        public const int BADDELETE = 99;

        private int _nextKey;

        public TriggerClassTestTrigger(int nextKey)
        {
            _nextKey = nextKey;
        }

        public override bool BeforeSave(TriggerClass instance)
        {
            if (instance.Id == BADSAVE) return false;

            if (instance.Id > 0) return true;

            instance.Id = _nextKey++;
            return true;
        }

        public override void AfterSave(TriggerClass instance)
        {
            instance.IsDirty = false;
        }

        public override bool BeforeDelete(int key)
        {
            return key != BADDELETE;
        }
    }

    public class TriggerListTestTrigger : BaseSterlingTrigger<TestModel, int>
    {
        private int _nextKey;

        public TriggerListTestTrigger(int nextKey)
        {
            _nextKey = nextKey;
        }

        public override bool BeforeSave(TestModel instance)
        {
            if (instance.Key > 0) return true;

            instance.Key = _nextKey++;
            return true;
        }

        public override void AfterSave(TestModel instance)
        {
        }

        public override bool BeforeDelete(int key)
        {
            return true;
        }
    }

    public class TriggerDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
            {
                CreateTableDefinition<TriggerClass, int>(e => e.Id),
                CreateTableDefinition<TestListModel, int>(t => t.ID),
                CreateTableDefinition<TestModel, int>(t => t.Key)
            };
        }
    }

    public class TestTriggers : TestBase
    {
        public TestTriggers()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TriggerDatabase>(TestContext.TestName, GetDriver());
            _databaseInstance.PurgeAsync().Wait();

            // get the next key in the database for auto-assignment
            var nextKey = _databaseInstance.Query<TriggerClass, int>().Any()
                ? (from keys in _databaseInstance.Query<TriggerClass, int>()
                    select keys.Key).Max() + 1
                : 1;

            _databaseInstance.RegisterTrigger(new TriggerClassTestTrigger(nextKey));
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
        public void TestTriggerAfterSave()
        {
            var target = new TriggerClass {Data = Guid.NewGuid().ToString(), IsDirty = true};
            _databaseInstance.SaveAsync<TriggerClass, int>(target).Wait();
            Assert.False(target.IsDirty); //Trigger failed: is dirty flag was not reset.");
        }

        [Fact]
        public void TestTriggerBeforeDelete()
        {
            var instance1 = new TriggerClass {Data = Guid.NewGuid().ToString()};
            _databaseInstance.SaveAsync<TriggerClass, int>(instance1).Wait();
            var key2 = _databaseInstance.SaveAsync<TriggerClass, int>(new TriggerClass
                {Id = TriggerClassTestTrigger.BADDELETE, Data = Guid.NewGuid().ToString()}).Result;

            _databaseInstance.DeleteAsync(instance1).Wait(); // should be no problem

            var handled = false;

            try
            {
                _databaseInstance.DeleteAsync(typeof(TriggerClass), key2).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Single() is SterlingTriggerException) handled = true;
            }

            Assert.True(handled); //Trigger failed to throw exception for delete operation on key = 5.");
        }

        [Fact]
        public void TestTriggerBeforeSaveWithFailure()
        {
            var handled = false;
            try
            {
                _databaseInstance.SaveAsync<TriggerClass, int>(new TriggerClass
                    {Id = TriggerClassTestTrigger.BADSAVE, Data = Guid.NewGuid().ToString()}).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Single() is SterlingTriggerException) handled = true;
            }

            Assert.True(handled); //Save failed: trigger did not throw exception");

            var actual = _databaseInstance.LoadAsync<TriggerClass>(TriggerClassTestTrigger.BADSAVE).Result;

            Assert.Null(actual); //Trigger failed: instance was saved.");
        }

        [Fact]
        public void TestTriggerBeforeSaveWithSuccess()
        {
            var key1 = _databaseInstance
                .SaveAsync<TriggerClass, int>(new TriggerClass {Data = Guid.NewGuid().ToString()}).Result;
            var key2 = _databaseInstance
                .SaveAsync<TriggerClass, int>(new TriggerClass {Data = Guid.NewGuid().ToString()}).Result;
            Assert.True(key1 > 0); //Trigger failed: key is not greater than 0.");
            Assert.True(key2 > 0); //Save failed: second key is not greater than 0.");
            Assert.True(key2 - key1 == 1); //Save failed: second key isn't one greater than first key.");
        }

        [Fact]
        public void TestTriggerOnChildren()
        {
            var trigger = new TriggerListTestTrigger(100);
            _databaseInstance.RegisterTrigger(trigger);
            var expected = TestListModel.MakeTestListModel();

            // set all the keys to something negative so the trigger can generate the key
            foreach (var subModel in expected.Children) subModel.Key = -1 * subModel.Key;

            var key = _databaseInstance.SaveAsync(expected).Result;

            var actual = _databaseInstance.LoadAsync<TestListModel>(key).Result;

            Assert.NotNull(actual.Children); //Trigger failed: child list is null.");
            Assert.Equal(expected.Children.Count,
                actual.Children.Count); //Trigger failed: actual child count different.");

            var noKey = (from m in actual.Children where m == null || m.Key < 1 select m).Any();

            Assert.False(noKey); //Trigger failed: children found without a key.");            
            _databaseInstance.UnregisterTrigger(trigger);
        }
    }
}