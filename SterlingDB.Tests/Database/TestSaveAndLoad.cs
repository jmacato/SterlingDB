
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Core.Exceptions;
using SterlingDB.Test.Helpers;

namespace SterlingDB.Test.Database
{
#if SILVERLIGHT
    [Tag("SaveAndLoad")]
#endif
    
    public class TestSaveAndLoadAltDriver : TestSaveAndLoad
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
    [Tag("SaveAndLoad")]
#endif
    
    public class TestSaveAndLoad : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        

        public class TestLateBoundTable
        {
            public int Id { get; set; }
            public string Data { get; set; }
        }

        public class TestSecondLateBoundTable : TestLateBoundTable
        {        
        }

        
        public void TestInit()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
            _databaseInstance.PurgeAsync().Wait();
        }

        
        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public async Task TestSaveExceptions()
        {
            var raiseException = false;
            try
            {
                await _databaseInstance.SaveAsync( this );
            }
            catch ( SterlingTableNotFoundException )
            {
                raiseException = true;
            }

            Assert.True(raiseException, "Sterling did not raise exception for unknown type.");
        }

        [Fact]
        public void TestSave()
        {
            // test saving and reloading
            var expected = TestModel.MakeTestModel();

            _databaseInstance.SaveAsync( expected ).Wait();

            var actual = _databaseInstance.LoadAsync<TestModel>( expected.Key ).Result;

            Assert.NotNull(actual, "Load failed.");

            Assert.Equal(expected.Key, actual.Key, "Load failed: key mismatch.");
            Assert.Equal(expected.Data, actual.Data, "Load failed: data mismatch.");
            Assert.Null(actual.Data2, "Load failed: suppressed data property not valid on de-serialize.");
            Assert.NotNull(actual.SubClass, "Load failed: sub class is null.");
            Assert.Null(actual.SubClass2, "Load failed: supressed sub class should be null.");           
            Assert.Equal(expected.SubClass.NestedText, actual.SubClass.NestedText, "Load failed: sub class text mismtach.");
            Assert.Equal(expected.SubStruct.NestedId, actual.SubStruct.NestedId, "Load failed: sub struct id mismtach.");
            Assert.Equal(expected.SubStruct.NestedString, actual.SubStruct.NestedString, "Load failed: sub class string mismtach.");
        }

        [Fact]
        [Ignore]
        public void TestSaveLateBoundTable()
        {
            // test saving and reloading
            var expected = new TestLateBoundTable {Id = 1, Data = Guid.NewGuid().ToString()};

            _databaseInstance.RegisterTableDefinition(_databaseInstance.CreateTableDefinition<TestLateBoundTable,int>(t=>t.Id));

            _databaseInstance.SaveAsync( expected ).Wait();

            var actual = _databaseInstance.LoadAsync<TestLateBoundTable>( expected.Id ).Result;

            Assert.NotNull(actual, "Load failed.");

            Assert.Equal(expected.Id, actual.Id, "Load failed: key mismatch.");
            Assert.Equal(expected.Data, actual.Data, "Load failed: data mismatch.");

            _databaseInstance.FlushAsync().Wait();

            _engine.Dispose();
            var driver = _databaseInstance.Driver;
            _databaseInstance = null;

            // bring it back up
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, driver);

            // do this in a different order
            _databaseInstance.RegisterTableDefinition(_databaseInstance.CreateTableDefinition<TestSecondLateBoundTable,int>(t=>t.Id));

            _databaseInstance.RegisterTableDefinition(_databaseInstance.CreateTableDefinition<TestLateBoundTable, int>(t => t.Id));

            actual = _databaseInstance.LoadAsync<TestLateBoundTable>( expected.Id ).Result;

            Assert.NotNull(actual, "Load failed after restart.");

            Assert.Equal(expected.Id, actual.Id, "Load failed: key mismatch after restart.");
            Assert.Equal(expected.Data, actual.Data, "Load failed: data mismatch after restart.");
        }

        [Fact]
        public void TestSaveShutdownReInitialize()
        {
            _databaseInstance.PurgeAsync().Wait();

            // test saving and reloading
            var expected1 = TestModel.MakeTestModel();
            var expected2 = TestModel.MakeTestModel();

            expected2.GuidNullable = null;

            var expectedComplex = new TestComplexModel
                                      {
                                          Id = 5,
                                          Dict = new Dictionary<string, string>(),
                                          Models = new ObservableCollection<TestModel>()
                                      };
            for (var x = 0; x < 10; x++)
            {
                expectedComplex.Dict.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                expectedComplex.Models.Add(TestModel.MakeTestModel());
            }

            _databaseInstance.SaveAsync( expected1 ).Wait();
            _databaseInstance.SaveAsync( expected2 ).Wait();
            _databaseInstance.SaveAsync( expectedComplex ).Wait();

            _databaseInstance.FlushAsync().Wait();
            
            // shut it down

            _engine.Dispose();
            var driver = _databaseInstance.Driver; 
            _databaseInstance = null;

            // bring it back up
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, driver);

            var actual1 = _databaseInstance.LoadAsync<TestModel>( expected1.Key ).Result;
            var actual2 = _databaseInstance.LoadAsync<TestModel>( expected2.Key ).Result;
            
            Assert.NotNull(actual1, "Load failed for 1.");
            Assert.Equal(expected1.Key, actual1.Key, "Load failed (1): key mismatch.");
            Assert.Equal(expected1.Data, actual1.Data, "Load failed(1): data mismatch.");
            Assert.NotNull(actual1.SubClass, "Load failed (1): sub class is null.");
            Assert.Equal(expected1.SubClass.NestedText, actual1.SubClass.NestedText, "Load failed (1): sub class text mismtach.");
            Assert.Equal(expected1.GuidNullable, actual1.GuidNullable, "Load failed (1): nullable Guid mismtach.");

            Assert.NotNull(actual2, "Load failed for 2.");
            Assert.Equal(expected2.Key, actual2.Key, "Load failed (2): key mismatch.");
            Assert.Equal(expected2.Data, actual2.Data, "Load failed (2): data mismatch.");
            Assert.NotNull(actual2.SubClass, "Load failed (2): sub class is null.");
            Assert.Equal(expected2.SubClass.NestedText, actual2.SubClass.NestedText, "Load failed (2): sub class text mismatch.");
            Assert.Null(expected2.GuidNullable, "Load failed (2): nullable Guid was not loaded as null.");

            //insert a third 
            var expected3 = TestModel.MakeTestModel();
            _databaseInstance.SaveAsync( expected3 ).Wait();

            actual1 = _databaseInstance.LoadAsync<TestModel>( expected1.Key ).Result;
            actual2 = _databaseInstance.LoadAsync<TestModel>( expected2.Key ).Result;
            var actual3 = _databaseInstance.LoadAsync<TestModel>( expected3.Key ).Result;

            Assert.NotNull(actual1, "Load failed for 1.");
            Assert.Equal(expected1.Key, actual1.Key, "Load failed (1): key mismatch.");
            Assert.Equal(expected1.Data, actual1.Data, "Load failed(1): data mismatch.");
            Assert.NotNull(actual1.SubClass, "Load failed (1): sub class is null.");
            Assert.Equal(expected1.SubClass.NestedText, actual1.SubClass.NestedText, "Load failed (1): sub class text mismtach.");

            Assert.NotNull(actual2, "Load failed for 2.");
            Assert.Equal(expected2.Key, actual2.Key, "Load failed (2): key mismatch.");
            Assert.Equal(expected2.Data, actual2.Data, "Load failed (2): data mismatch.");
            Assert.NotNull(actual2.SubClass, "Load failed (2): sub class is null.");
            Assert.Equal(expected2.SubClass.NestedText, actual2.SubClass.NestedText, "Load failed (2): sub class text mismtach.");

            Assert.NotNull(actual3, "Load failed for 3.");
            Assert.Equal(expected3.Key, actual3.Key, "Load failed (3): key mismatch.");
            Assert.Equal(expected3.Data, actual3.Data, "Load failed (3): data mismatch.");
            Assert.NotNull(actual3.SubClass, "Load failed (3): sub class is null.");
            Assert.Equal(expected3.SubClass.NestedText, actual3.SubClass.NestedText, "Load failed (3): sub class text mismtach.");

            // load the complex 
            var actualComplex = _databaseInstance.LoadAsync<TestComplexModel>( 5 ).Result;
            Assert.NotNull(actualComplex, "Load failed (complex): object is null.");
            Assert.Equal(5, actualComplex.Id, "Load failed: id mismatch.");
            Assert.NotNull(actualComplex.Dict, "Load failed: dictionary is null.");
            foreach(var key in expectedComplex.Dict.Keys)
            {
                var value = expectedComplex.Dict[key];
                Assert.True(actualComplex.Dict.Contains(key), "Load failed: dictionary is missing key.");
                Assert.Equal(value, actualComplex.Dict[key], "Load failed: dictionary has invalid value.");
            }

            Assert.NotNull(actualComplex.Models, "Load failed: complex missing the model collection.");

            foreach(var model in expectedComplex.Models)
            {
                var targetModel = actualComplex.Models.Where(m => m.Key.Equals(model.Key)).FirstOrDefault();
                Assert.NotNull(targetModel, "Load failed for nested model.");
                Assert.Equal(model.Key, targetModel.Key, "Load failed for nested model: key mismatch.");
                Assert.Equal(model.Data, targetModel.Data, "Load failed for nested model: data mismatch.");
                Assert.NotNull(targetModel.SubClass, "Load failed for nested model: sub class is null.");
                Assert.Equal(model.SubClass.NestedText, targetModel.SubClass.NestedText, "Load failed for nested model: sub class text mismtach.");
            }

        }
        
        [Fact]
        public void TestSaveForeign()
        {
            var expected = TestAggregateModel.MakeAggregateModel();

            _databaseInstance.SaveAsync( expected ).Wait();

            var actual = _databaseInstance.LoadAsync<TestAggregateModel>( expected.Key ).Result;
            var actualTestModel = _databaseInstance.LoadAsync<TestModel>( expected.TestModelInstance.Key ).Result;
            var actualForeignModel = _databaseInstance.LoadAsync<TestForeignModel>( expected.TestForeignInstance.Key ).Result;
            var actualDerivedModel = _databaseInstance.LoadAsync<TestDerivedClassAModel>( expected.TestBaseClassInstance.Key ).Result;

            Assert.Equal(expected.Key, actual.Key, "Load with foreign key failed: key mismatch.");
            Assert.Equal(expected.TestForeignInstance.Key, actual.TestForeignInstance.Key, "Load failed: foreign key mismatch.");
            Assert.Equal(expected.TestForeignInstance.Data, actual.TestForeignInstance.Data, "Load failed: foreign data mismatch.");
            Assert.Equal(expected.TestModelInstance.Key, actual.TestModelInstance.Key, "Load failed: test model key mismatch.");
            Assert.Equal(expected.TestModelInstance.Data, actual.TestModelInstance.Data, "Load failed: test model data mismatch.");
            Assert.Equal(expected.TestForeignInstance.Key, actualForeignModel.Key, "Load failed: foreign key mismatch on direct load.");
            Assert.Equal(expected.TestForeignInstance.Data, actualForeignModel.Data, "Load failed: foreign data mismatch on direct load.");
            Assert.Equal(expected.TestModelInstance.Key, actualTestModel.Key, "Load failed: test model key mismatch on direct load.");
            Assert.Equal(expected.TestModelInstance.Data, actualTestModel.Data, "Load failed: test model data mismatch on direct load.");

            Assert.Equal(expected.TestBaseClassInstance.Key, actual.TestBaseClassInstance.Key, "Load failed: base class key mismatch.");
            Assert.Equal(expected.TestBaseClassInstance.BaseProperty, actual.TestBaseClassInstance.BaseProperty, "Load failed: base class data mismatch.");
            Assert.Equal(expected.TestBaseClassInstance.GetType(), actual.TestBaseClassInstance.GetType(), "Load failed: base class type mismatch.");
        }

        [Fact]
        public void TestSaveForeignNull()
        {
            var expected = TestAggregateModel.MakeAggregateModel();
            expected.TestForeignInstance = null;

            _databaseInstance.SaveAsync( expected ).Wait();

            var actual = _databaseInstance.LoadAsync<TestAggregateModel>( expected.Key ).Result;
            var actualTestModel = _databaseInstance.LoadAsync<TestModel>( expected.TestModelInstance.Key ).Result;
            
            Assert.Equal(expected.Key, actual.Key, "Load with foreign key failed: key mismatch.");
            Assert.Null(actual.TestForeignInstance, "Load failed: foreign key not set to null.");
            Assert.Equal(expected.TestModelInstance.Key, actual.TestModelInstance.Key, "Load failed: test model key mismatch.");
            Assert.Equal(expected.TestModelInstance.Data, actual.TestModelInstance.Data, "Load failed: test model data mismatch.");
            Assert.Equal(expected.TestModelInstance.Key, actualTestModel.Key, "Load failed: test model key mismatch on direct load.");
            Assert.Equal(expected.TestModelInstance.Data, actualTestModel.Data, "Load failed: test model data mismatch on direct load.");
        }

        [Fact]
        public void TestSaveAsWithBase()
        {
            var expected = new TestIndexedSubclassBase();
            expected.BaseProperty = "This is base";
            expected.Id = 1;
            _databaseInstance.SaveAsAsync<TestIndexedSubclassBase>( expected ).Wait();

            var actual = _databaseInstance.LoadAsync<TestIndexedSubclassBase>( expected.Id ).Result;

            Assert.Equal(expected.Id, actual.Id, "Save As failed: key mismatch. ");
            Assert.Equal(expected.BaseProperty, actual.BaseProperty, "Save As failed: base property mismatch. ");
        }

        [Fact]
        public void TestSaveAsWithSubclass()
        {
            var expected = new TestIndexedSubclassModel();
            expected.BaseProperty = "This is base";
            expected.SubclassProperty = "This is subclass";
            expected.Id = 2;
            _databaseInstance.SaveAsAsync<TestIndexedSubclassBase>( expected ).Wait();

            var actual = _databaseInstance.LoadAsync<TestIndexedSubclassBase>( expected.Id ).Result;
            var actualSubclass = actual as TestIndexedSubclassModel;

            Assert.Equal(expected.Id, actual.Id, "Save As failed: key mismatch. ");
            Assert.Equal(expected.BaseProperty, actual.BaseProperty, "Save As failed: base property mismatch. ");
            Assert.NotNull(actualSubclass, "Save As failed: Subclass not honoured on deserialization. ");
            Assert.Equal(expected.SubclassProperty, actualSubclass.SubclassProperty, "Save As failed: Subclass property mismatch. ");
        }

        [Fact]
        public void TestSaveAsWithInvalidSubclass()
        {
            SterlingException expectedException = null;
            var expected = new TestIndexedSubclassFake();

            var expectedErrorMessage = string.Format("{0} is not of type {1}", expected.GetType().Name, typeof(TestIndexedSubclassBase).Name);
            
            expected.BaseProperty = "This is base";
            expected.SubclassProperty = "This is subclass";
            expected.Id = 2;

            try
            {
                _databaseInstance.SaveAsAsync(typeof(TestIndexedSubclassBase),expected).Wait();
            }
            catch (SterlingException ex)
            {
                expectedException = ex;
            }

            Assert.NotNull(expectedException, "Save As failed: succeeded with inaccurate subclass");
            Assert.InstanceOfType(expectedException, typeof(SterlingException));
            Assert.Equal(expectedErrorMessage,expectedException.Message);
        }
    }
}
