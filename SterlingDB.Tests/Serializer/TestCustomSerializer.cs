using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SterlingDB.Database;
using SterlingDB.Serialization;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Serializer
{
    /// <summary>
    ///     Example list normally not supported (IEnumerable)
    /// </summary>
    public class NotSupportedList : IEnumerable<TestModel>
    {
        private readonly List<TestModel> _list = new List<TestModel>();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TestModel> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IEnumerable<TestModel> newItems)
        {
            _list.AddRange(newItems);
        }
    }

    /// <summary>
    ///     Class typically not supported because the property is IEnumerable
    /// </summary>
    public class NotSupportedClass
    {
        public NotSupportedClass()
        {
            InnerList = new NotSupportedList();
        }

        public int Id { get; set; }

        public NotSupportedList InnerList { get; set; }
    }

    /// <summary>
    ///     Custom database to host the test
    /// </summary>
    public class CustomSerializerDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
            {
                CreateTableDefinition<NotSupportedClass, int>(t => t.Id),
                CreateTableDefinition<TestModel, int>(t => t.Key)
            };
        }
    }

    /// <summary>
    ///     Serializer to create support for the non-supported property
    /// </summary>
    public class SupportSerializer : BaseSerializer
    {
        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            // only support the "non-supported" list
            return targetType.Equals(typeof(NotSupportedList));
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            // turn it into a list and save it 
            var list = new List<TestModel>((NotSupportedList) target);

            // this takes advantage of the special save wrapper for injecting into the stream
            TestCustomSerializer.DatabaseInstance.Helper.Save(list, writer);
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            // grab it as a list - again, unwrapped from a node and returned
            var list = TestCustomSerializer.DatabaseInstance.Helper.Load<List<TestModel>>(reader);
            return new NotSupportedList {list};
        }
    }


    public class TestCustomSerializer : TestBase
    {
        /// <summary>
        ///     Initialize the test
        /// </summary>
        public TestCustomSerializer()
        {
            _engine = Factory.NewEngine();
            _engine.SterlingDatabase.RegisterSerializer<SupportSerializer>();
            _engine.Activate();
            DatabaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<CustomSerializerDatabase>(TestContext.TestName, GetDriver());
            DatabaseInstance.PurgeAsync().Wait();
        }

        private readonly SterlingEngine _engine;
        public static ISterlingDatabaseInstance DatabaseInstance;

        /// <summary>
        ///     Clean up when done
        /// </summary>
        public override void Cleanup()
        {
            DatabaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            DatabaseInstance = null;
        }

        /// <summary>
        ///     Test the serializer by creating a typically non-supported class and processing with
        ///     the custom serializer
        /// </summary>
        [Fact]
        public void TestCustomSaveAndLoad()
        {
            var expectedList = new[] {TestModel.MakeTestModel(), TestModel.MakeTestModel(), TestModel.MakeTestModel()};
            var expected = new NotSupportedClass {Id = 1};
            expected.InnerList.Add(expectedList);

            var key = DatabaseInstance.SaveAsync(expected).Result;

            // confirm the test models were saved as "foreign keys" 
            var count = DatabaseInstance.Query<TestModel, int>().Count();

            Assert.Equal(expectedList.Length, count); //Load failed: test models were not saved independently.");

            var actual = DatabaseInstance.LoadAsync<NotSupportedClass>(key).Result;
            Assert.NotNull(actual); //Load failed: instance is null.");
            Assert.Equal(expected.Id, actual.Id); //Load failed: key mismatch.");

            // cast to list
            var actualList = new List<TestModel>(actual.InnerList);

            Assert.Equal(expectedList.Length, actualList.Count); //Load failed: mismatch in list.");

            foreach (var matchingItem in
                expectedList
                    .Select(item => (from i in actualList where i.Key.Equals(item.Key) select i.Key).FirstOrDefault())
                    .Where(matchingItem => matchingItem < 1))
                Assert.True(false, "Test failed: matching models not loaded.");
        }
    }
}