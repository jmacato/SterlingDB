﻿using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestDictionary : TestBase
    {
        public TestDictionary()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
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
        public void TestDictionarySaveAndLoad()
        {
            var expected = TestClassWithDictionary.MakeTestClassWithDictionary();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithDictionary>(key).Result;

            Assert.NotNull(actual); //Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID); //Save/load failed: key mismatch.");
            Assert.NotNull(actual.DictionaryWithBaseClassAsValue); //Save/load failed: dictionary not initialized.");
            Assert.NotNull(actual.DictionaryWithClassAsValue); //Save/load failed: dictionary not initialized.");
            Assert.NotNull(actual.DictionaryWithListAsValue); //Save/load failed: dictionary not initialized.");
            Assert.NotNull(actual.BaseDictionary); //Save/load failed: dictionary not initialized.");

            foreach (var v in expected.BaseDictionary)
            {
                Assert.True(actual.BaseDictionary.ContainsKey(v.Key)); //Save/load failed: key not found.");
                Assert.Equal(expected.BaseDictionary[v.Key],
                    actual.BaseDictionary[v.Key]); //Save/load failed: key mismatch.");
            }

            foreach (var v in expected.DictionaryWithBaseClassAsValue)
            {
                Assert.True(actual.DictionaryWithBaseClassAsValue
                    .ContainsKey(v.Key)); //Save/load failed: key not found.");
                Assert.Equal(expected.DictionaryWithBaseClassAsValue[v.Key].Key,
                    actual.DictionaryWithBaseClassAsValue[v.Key].Key); //Save/load failed: key mismatch.");
                Assert.Equal(expected.DictionaryWithBaseClassAsValue[v.Key].BaseProperty,
                    actual.DictionaryWithBaseClassAsValue[v.Key].BaseProperty); //Save/load failed: data mismatch.");
                Assert.Equal(expected.DictionaryWithBaseClassAsValue[v.Key].GetType(),
                    actual.DictionaryWithBaseClassAsValue[v.Key].GetType()); //Save/load failed: type mismatch.");
            }

            foreach (var v in expected.DictionaryWithClassAsValue)
            {
                Assert.True(actual.DictionaryWithClassAsValue.ContainsKey(v.Key)); //Save/load failed: key not found.");
                Assert.Equal(expected.DictionaryWithClassAsValue[v.Key].Key,
                    actual.DictionaryWithClassAsValue[v.Key].Key); //Save/load failed: key mismatch.");
                Assert.Equal(expected.DictionaryWithClassAsValue[v.Key].Data,
                    actual.DictionaryWithClassAsValue[v.Key].Data); //Save/load failed: data mismatch.");
                Assert.Equal(expected.DictionaryWithClassAsValue[v.Key].Date,
                    actual.DictionaryWithClassAsValue[v.Key].Date); //Save/load failed: date mismatch.");
                Assert.Equal(expected.DictionaryWithClassAsValue[v.Key].GetType(),
                    actual.DictionaryWithClassAsValue[v.Key].GetType()); //Save/load failed: type mismatch.");
            }

            foreach (var v in expected.DictionaryWithListAsValue)
            {
                Assert.True(actual.DictionaryWithListAsValue.ContainsKey(v.Key)); //Save/load failed: key not found.");
                Assert.NotNull(actual.DictionaryWithListAsValue[v.Key]); //Save/load failed: list not initialized.");
                Assert.Equal(expected.DictionaryWithListAsValue[v.Key].Count,
                    actual.DictionaryWithListAsValue[v.Key].Count); //Save/load failed: list size mismatch.");

                for (var x = 0; x < expected.DictionaryWithListAsValue[v.Key].Count; x++)
                {
                    Assert.Equal(expected.DictionaryWithListAsValue[v.Key][x].Key,
                        actual.DictionaryWithListAsValue[v.Key][x].Key); //Save/load failed: key mismatch.");
                    Assert.Equal(expected.DictionaryWithListAsValue[v.Key][x].Data,
                        actual.DictionaryWithListAsValue[v.Key][x].Data); //Save/load failed: data mismatch.");
                    Assert.Equal(expected.DictionaryWithListAsValue[v.Key][x].Date,
                        actual.DictionaryWithListAsValue[v.Key][x].Date); //Save/load failed: date mismatch.");
                }
            }
        }

        [Fact]
        public void TestEmptyDictionary()
        {
            var expected = TestClassWithDictionary.MakeTestClassWithDictionary();
            expected.DictionaryWithBaseClassAsValue.Clear();
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithDictionary>(key).Result;

            Assert.NotNull(actual); //Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID); //Save/load failed: key mismatch.");
            Assert.NotNull(actual.DictionaryWithBaseClassAsValue); //Save/load failed: dictionary not initialized.");
            Assert.Empty(actual.DictionaryWithBaseClassAsValue); //Save/load failed: dictionary size mismatch.");
        }

        [Fact]
        public void TestNullDictionary()
        {
            var expected = TestClassWithDictionary.MakeTestClassWithDictionary();
            expected.DictionaryWithBaseClassAsValue = null;
            var key = _databaseInstance.SaveAsync(expected).Result;
            var actual = _databaseInstance.LoadAsync<TestClassWithDictionary>(key).Result;

            Assert.NotNull(actual); //Save/load failed: model is null.");
            Assert.Equal(expected.ID, actual.ID); //Save/load failed: key mismatch.");
            Assert.Null(actual.DictionaryWithBaseClassAsValue); //Save/load failed: dictionary is not null.");
        }
    }
}