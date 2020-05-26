
#if NETFX_CORE
using SterlingDB.WinRT.WindowsStorage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif SILVERLIGHT
using Microsoft.Phone.Testing;
using SterlingDB.WP8.IsolatedStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using SterlingDB.Server.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

using System;
using System.Collections.Generic;

using SterlingDB.Core;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Keys
{
#if SILVERLIGHT
    [Tag("CompositeKey")]
#endif
    
    public class TestCompositeKeyWithKeyAltDriver : TestCompositeKey
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
    [Tag("CompositeKey")]
#endif
    
    public class TestCompositeKeyWithKeyClass : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        

        
        public void TestInit()
        {           
            _engine = Factory.NewEngine();
            _engine.SterlingDatabase.RegisterSerializer<TestCompositeSerializer>();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstanceComposite>( TestContext.TestName, GetDriver() );
            _databaseInstance.PurgeAsync().Wait();
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;            
        }       

        [Fact]
        public void TestSave()
        {
            var random = new Random();
            // test saving and reloading
            var list = new List<TestCompositeClass>();
            for (var x = 0; x < 100; x++)
            {
                var testClass = new TestCompositeClass
                {
                    Key1 = random.Next(),
                    Key2 = random.Next().ToString(),
                    Key3 = Guid.NewGuid(),
                    Key4 = DateTime.Now.AddMinutes(-1 * random.Next(100)),
                    Data = Guid.NewGuid().ToString()
                };
                list.Add(testClass);
                _databaseInstance.SaveAsync( testClass ).Wait();
            }

            for (var x = 0; x < 100; x++)
            {
                var actual = _databaseInstance.LoadAsync<TestCompositeClass>( new TestCompositeKeyClass( list[ x ].Key1,
                    list[x].Key2,list[x].Key3,list[x].Key4)).Result;
                Assert.NotNull(actual, "Load failed.");
                Assert.Equal(list[x].Data, actual.Data, "Load failed: data mismatch.");
            }
        }
    }
}