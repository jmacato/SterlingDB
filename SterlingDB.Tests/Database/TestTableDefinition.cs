using SterlingDB.Test.Helpers;
using Xunit;
using System.Linq;
using SterlingDB.Serialization;
using SterlingDB.Server;
using SterlingDB.Database;

namespace SterlingDB.Test.Database
{

    public class TestTableDefinition : TestBase
    {
        protected virtual ISterlingDriver GetDriver(string test, ISterlingSerializer serializer)
        {
            return new MemoryDriver() { DatabaseInstanceName = test, DatabaseSerializer = serializer };
        }

#pragma warning disable
        private readonly TestModel[] _models = new[]
                                          {
                                              TestModel.MakeTestModel(), TestModel.MakeTestModel(),
                                              TestModel.MakeTestModel()
                                          };

        private TableDefinition<TestModel, int> _target;
        private readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();
        private int _testAccessCount;

#pragma warning restore

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

        public TestTableDefinition()
        {
            var serializer = new AggregateSerializer(new PlatformAdapter());
            serializer.AddSerializer(new DefaultSerializer());
            serializer.AddSerializer(new ExtendedSerializer(new PlatformAdapter()));
            _testAccessCount = 0;
            _target = new TableDefinition<TestModel, int>(GetDriver(TestContext.TestName, serializer),
                                                        GetTestModelByKey, t => t.Key);
        }

        [Fact]
        public void TestConstruction()
        {
            Assert.Equal(typeof(TestModel), _target.TableType); //Table type mismatch.");
            Assert.Equal(typeof(int), _target.KeyType); //Key type mismatch.");
            var key = _target.FetchKey(_models[1]);
            Assert.Equal(_models[1].Key, key); //Key mismatch after fetch key invoked.");
        }

        public override void Cleanup()
        {

        }
    }
}
