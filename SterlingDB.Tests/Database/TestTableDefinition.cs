﻿
#if NETFX_CORE
using SterlingDB.WinRT.WindowsStorage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SterlingDB.WinRT;
#elif SILVERLIGHT
using Microsoft.Phone.Testing;
using SterlingDB.WP8.IsolatedStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SterlingDB.WP8;
#else
using SterlingDB.Server;
using SterlingDB.Server.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SterlingDB.Server;
#endif

using System;
using System.Linq;

using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Core.Serialization;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Database
{
#if SILVERLIGHT
    [Tag("TableDefinition")]
#endif
    [TestClass]
    public class TestTableDefinitionAltDriver : TestTableDefinition
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

        protected override ISterlingDriver GetDriver( string test, ISterlingSerializer serializer )
        {
#if NETFX_CORE
            return new WindowsStorageDriver() { DatabaseInstanceName = test, DatabaseSerializer = serializer };
#elif SILVERLIGHT
            return new IsolatedStorageDriver()  { DatabaseInstanceName = test, DatabaseSerializer = serializer };
#elif AZURE_DRIVER
            return new SterlingDB.Server.Azure.TableStorage.Driver() { DatabaseInstanceName = test, DatabaseSerializer = serializer };
#else
            return new FileSystemDriver()  { DatabaseInstanceName = test, DatabaseSerializer = serializer };
#endif
        }
    }

#if SILVERLIGHT
    [Tag("TableDefinition")]
#endif
    [TestClass]
    public class TestTableDefinition : TestBase
    {
        protected virtual ISterlingDriver GetDriver( string test, ISterlingSerializer serializer )
        {
            return new MemoryDriver() { DatabaseInstanceName = test, DatabaseSerializer = serializer };
        }

        private readonly TestModel[] _models = new[]
                                          {
                                              TestModel.MakeTestModel(), TestModel.MakeTestModel(),
                                              TestModel.MakeTestModel()
                                          };

        private TableDefinition<TestModel, int> _target;
        private readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();
        private int _testAccessCount;

        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Fetcher - also flag the fetch
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The model</returns>
        private TestModel _GetTestModelByKey(int key)
        {
            _testAccessCount++;
            return (from t in _models where t.Key.Equals(key) select t).FirstOrDefault();
        }

        
        public void TestInit()
        {
            var serializer = new AggregateSerializer( new PlatformAdapter() );
            serializer.AddSerializer(new DefaultSerializer());
            serializer.AddSerializer(new ExtendedSerializer( new PlatformAdapter() ));
            _testAccessCount = 0;
            _target = new TableDefinition<TestModel, int>(GetDriver(TestContext.TestName, serializer),
                                                        _GetTestModelByKey, t => t.Key);
        }        

        [Fact]
        public void TestConstruction()
        {
            Assert.Equal(typeof(TestModel), _target.TableType, "Table type mismatch.");
            Assert.Equal(typeof(int), _target.KeyType, "Key type mismatch.");
            var key = _target.FetchKey(_models[1]);
            Assert.Equal(_models[1].Key, key, "Key mismatch after fetch key invoked.");
        }
    }
}
