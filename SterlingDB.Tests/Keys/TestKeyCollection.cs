using System.Linq;
using SterlingDB.Keys;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Keys
{
    public class TestKeyCollection : TestBase
    {
        public TestKeyCollection()
        {
            _driver = GetDriver();
            _testAccessCount = 0;
            _target = new KeyCollection<TestModel, int>(_driver,
                GetTestModelByKey);
        }

        private readonly TestModel[] _models =
        {
            TestModel.MakeTestModel(), TestModel.MakeTestModel(),
            TestModel.MakeTestModel()
        };

        private ISterlingDriver _driver;
        private readonly KeyCollection<TestModel, int> _target;
        protected readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();
        private int _testAccessCount;


        /// <summary>
        ///     Fetcher - also flag the fetch
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The model</returns>
        private TestModel GetTestModelByKey(int key)
        {
            _testAccessCount++;
            return (from t in _models where t.Key.Equals(key) select t).FirstOrDefault();
        }


        public override void Cleanup()
        {
            _driver.PurgeAsync().Wait();
            _driver = null;
        }

        [Fact]
        public void TestAddDuplicateKey()
        {
            Assert.False(_target.IsDirty); //Dirty flag set prematurely");
            Assert.Equal(0, _target.NextKey); //Next key is incorrect initialized.");
            _target.AddKeyAsync(_models[0].Key).Wait();
            Assert.True(_target.IsDirty); //Dirty flag not set on add.");
            Assert.Equal(1, _target.NextKey); //Next key not advanced.");
            _target.AddKeyAsync(_models[0].Key).Wait();
            Assert.Equal(1, _target.NextKey); //Next key advanced on duplicate add."); 
            Assert.Single(_target.Query); //Key list count is incorrect.");
        }

        [Fact]
        public void TestAddKey()
        {
            Assert.False(_target.IsDirty); //Dirty flag set prematurely");
            Assert.Equal(0, _target.NextKey); //Next key is incorrect.");
            _target.AddKeyAsync(_models[0].Key).Wait();
            Assert.True(_target.IsDirty); //Dirty flag not set on add.");
            Assert.Equal(1, _target.NextKey); //Next key not advanced.");
        }

        [Fact]
        public void TestQueryable()
        {
            _target.AddKeyAsync(_models[0].Key).Wait();
            _target.AddKeyAsync(_models[1].Key).Wait();
            _target.AddKeyAsync(_models[2].Key).Wait();
            Assert.Equal(3, _target.Query.Count()); //Key count is incorrect.");
            Assert.Equal(0, _testAccessCount); //Lazy loader was accessed prematurely.");
            var testKey = (from k in _target.Query where k.Key.Equals(_models[1].Key) select k).FirstOrDefault();
            Assert.NotNull(testKey); //Test key not retrieved.");
            Assert.Equal(_models[1].Key, testKey.Key); //Key mismatch.");
            Assert.Equal(0, _testAccessCount); //Lazy loader was accessed prematurely.");
            var testModel = testKey.LazyValue.Value;
            Assert.Same(_models[1], testModel); //Model does not match.");
            Assert.Equal(1, _testAccessCount); //Lazy loader access count is incorrect.");
        }

        [Fact]
        public void TestRemoveKey()
        {
            Assert.False(_target.IsDirty); //Dirty flag set prematurely");
            Assert.Equal(0, _target.NextKey); //Next key is incorrect.");
            _target.AddKeyAsync(_models[0].Key).Wait();
            Assert.True(_target.IsDirty); //Dirty flag not set on add.");
            Assert.Equal(1, _target.NextKey); //Next key not advanced.");       
            _target.RemoveKeyAsync(_models[0].Key).Wait();
            Assert.Empty(_target.Query); //Key was not removed.");
        }

        [Fact]
        public void TestSerialization()
        {
            _target.AddKeyAsync(_models[0].Key).Wait();
            _target.AddKeyAsync(_models[1].Key).Wait();
            Assert.True(_target.IsDirty); //Dirty flag not set.");
            _target.FlushAsync().Wait();
            Assert.False(_target.IsDirty); //Dirty flag not reset on flush.");

            var secondTarget = new KeyCollection<TestModel, int>(_driver,
                GetTestModelByKey);

            // are we able to grab things?
            Assert.Equal(2, secondTarget.Query.Count()); //Key count is incorrect.");
            Assert.Equal(0, _testAccessCount); //Lazy loader was accessed prematurely.");
            var testKey = (from k in secondTarget.Query where k.Key.Equals(_models[1].Key) select k).FirstOrDefault();
            Assert.NotNull(testKey); //Test key not retrieved.");
            Assert.Equal(_models[1].Key, testKey.Key); //Key mismatch.");
            Assert.Equal(0, _testAccessCount); //Lazy loader was accessed prematurely.");
            var testModel = testKey.LazyValue.Value;
            Assert.Same(_models[1], testModel); //Model does not match.");
            Assert.Equal(1, _testAccessCount); //Lazy loader access count is incorrect.");

            // now let's test refresh
            secondTarget.AddKeyAsync(_models[2].Key).Wait();
            secondTarget.FlushAsync().Wait();

            Assert.Equal(2, _target.Query.Count()); //Unexpected key count in original collection.");
            _target.RefreshAsync().Wait();
            Assert.Equal(3, _target.Query.Count()); //Refresh failed.");
        }
    }
}