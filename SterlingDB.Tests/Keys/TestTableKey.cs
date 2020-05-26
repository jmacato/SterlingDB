﻿
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
    [TestClass]
    public class TestTableKey
    {
        [TestMethod]
        public void TestTableKeyFunctionality()
        {
            var list = new[] {TestModel.MakeTestModel(), TestModel.MakeTestModel()};

            Func<int, TestModel> getter = i => (from t in list where t.Key.Equals(i) select t).FirstOrDefault();

            var key1 = new TableKey<TestModel, int>(list[0].Key, getter);
            var key2 = new TableKey<TestModel, int>(list[1].Key, getter);

            Assert.AreEqual(key1.Key, list[0].Key, "Key mismatch.");
            Assert.AreEqual(key2.Key, list[1].Key, "Key mismatch.");
            
            Assert.IsFalse(key1.LazyValue.IsValueCreated, "Lazy model already created.");
            var testModel1 = key1.LazyValue.Value;
            Assert.IsTrue(key1.LazyValue.IsValueCreated, "Lazy value created was not set.");
            Assert.AreSame(list[0], testModel1, "First key returned invalid instance.");
            Assert.AreSame(list[1], key2.LazyValue.Value, "Second key return invalid instance.");
        }

    }
}