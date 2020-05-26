
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
using System.Linq;

using SterlingDB.Core;
using SterlingDB.Core.Keys;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Keys
{
#if SILVERLIGHT
    [Tag("TableKey")]
#endif
    
    public class TestTableKey
    {
        [Fact]
        public void TestTableKeyFunctionality()
        {
            var list = new[] {TestModel.MakeTestModel(), TestModel.MakeTestModel()};

            Func<int, TestModel> getter = i => (from t in list where t.Key.Equals(i) select t).FirstOrDefault();

            var key1 = new TableKey<TestModel, int>(list[0].Key, getter);
            var key2 = new TableKey<TestModel, int>(list[1].Key, getter);

            Assert.Equal(key1.Key, list[0].Key, "Key mismatch.");
            Assert.Equal(key2.Key, list[1].Key, "Key mismatch.");
            
            Assert.False(key1.LazyValue.IsValueCreated, "Lazy model already created.");
            var testModel1 = key1.LazyValue.Value;
            Assert.True(key1.LazyValue.IsValueCreated, "Lazy value created was not set.");
            Assert.Same(list[0], testModel1, "First key returned invalid instance.");
            Assert.Same(list[1], key2.LazyValue.Value, "Second key return invalid instance.");
        }

    }
}
