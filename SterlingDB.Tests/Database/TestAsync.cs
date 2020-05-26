
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SterlingDB.Core;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestAsync : TestBase
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private List<TestModel> _modelList;
        //private DateTime _startTime;

        private const int MODELS = 500;

        public void TestInit()
        {
            //_startTime = DateTime.Now;
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>("async");
            _modelList = new List<TestModel>();
            for (var i = 0; i < MODELS; i++)
            {
                _modelList.Add(TestModel.MakeTestModel());
            }
        }

        /// <summary>
        ///     Clean up
        /// </summary>
        /// <remarks>
        ///     Uncomment the top block to display the time for each operation
        /// </remarks>

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;

        }
 
        [Fact]
        public void TestConcurrentSaveAndLoad()
        {
            var saveEvent = new ManualResetEvent(false);
            var loadEvent = new ManualResetEvent(false);

            // Initialize the DB with some data.
            foreach (var item in _modelList)
            {
                _databaseInstance.SaveAsync(item).Wait();
            }

            var savedCount = 0;

            var errorMsg = string.Empty;

            var save = new Task(() =>
                              {
                                  try
                                  {
                                      foreach (var item in _modelList)
                                      {
                                          _databaseInstance.SaveAsync(item).Wait();
                                          savedCount++;
                                      }

                                      if (MODELS != savedCount)
                                      {
                                          throw new Exception("Save failed: Not all models were saved.");
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      errorMsg = ex.AsExceptionString();
                                  }
                                  finally
                                  {
                                      saveEvent.Set();
                                  }
                              });

            var load = new Task(() =>
                              {
                                  try
                                  {
                                      var query = from key in _databaseInstance.Query<TestModel, int>()
                                                  select key.LazyValue.Value;
                                      query.Count();

                                      var list = new List<TestModel>(query);

                                      if (list.Count < 1)
                                      {
                                          throw new Exception("Test failed: did not load any items.");
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      errorMsg = ex.AsExceptionString();
                                  }
                                  finally
                                  {
                                      loadEvent.Set();
                                  }
                              });

            save.Start();
            load.Start();

            saveEvent.WaitOne(60000);
            loadEvent.WaitOne(60000);

            Assert.True(string.IsNullOrEmpty(errorMsg), string.Format("Failed concurrent load: {0}", errorMsg));
 
        }

        [Fact]
        public void TestConcurrentSaveAndLoadWithIndex()
        {
            var saveEvent = new ManualResetEvent(false);
            var loadEvent = new ManualResetEvent(false);

            // Initialize the DB with some data.
            foreach (var item in _modelList)
            {
                _databaseInstance.SaveAsync(item).Wait();
            }

            var savedCount = 0;

            var errorMsg = string.Empty;

            var save = new Task(() =>
                              {
                                  try
                                  {
                                      foreach (var item in _modelList)
                                      {
                                          _databaseInstance.SaveAsync(item).Wait();
                                          savedCount++;
                                      }

                                      if (MODELS != savedCount)
                                      {
                                          throw new Exception("Save failed: Not all models were saved.");
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      errorMsg = ex.AsExceptionString();
                                  }
                                  finally
                                  {
                                      saveEvent.Set();
                                  }
                              });

            var load = new Task(() =>
                              {
                                  try
                                  {
                                      var now = DateTime.Now;
                                      var query =
                                          from key in
                                              _databaseInstance.Query<TestModel, DateTime, string, int>("IndexDateData")
                                          where key.Index.Item1.Month == now.Month
                                          select key.Value.Result;

                                      var list = new List<TestModel>(query);

                                      if (list.Count < 1)
                                      {
                                          throw new Exception("Test failed: did not load any models.");
                                      }
                                  }
                                  catch (Exception ex)
                                  {
                                      errorMsg = ex.AsExceptionString();
                                  }
                                  finally
                                  {
                                      loadEvent.Set();
                                  }
                              });

            save.Start();
            load.Start();

            saveEvent.WaitOne(60000);
            loadEvent.WaitOne(60000);

            Assert.True(string.IsNullOrEmpty(errorMsg), string.Format("Concurrent test failed: {0}", errorMsg));
        }
    }
}
