﻿using System.Collections.Generic;
using System.Linq;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestQuery : TestBase
    {
        public TestQuery()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
            _modelList = new List<TestModel>();
            for (var i = 0; i < 10; i++)
            {
                _modelList.Add(TestModel.MakeTestModel());
                _databaseInstance.SaveAsync(_modelList[i]).Wait();
            }
        }

        private readonly SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private readonly List<TestModel> _modelList;

        public override void Cleanup()
        {
            _databaseInstance.PurgeAsync().Wait();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [Fact]
        public void TestDescendingQuery()
        {
            var descending = from k in _databaseInstance.Query<TestModel, int>() orderby k.Key descending select k.Key;

            var idx = _modelList.Count - 1;
            foreach (var key in descending)
                Assert.Equal(_modelList[idx--].Key, key); //Descending query failed: key mismatch.");
        }

        [Fact]
        public void TestRangeQuery()
        {
            var range = from k in _databaseInstance.Query<TestModel, int>()
                where k.Key > _modelList[2].Key && k.Key < _modelList[5].Key
                orderby k.Key
                select k.Key;

            var idx = 3;
            foreach (var key in range) Assert.Equal(_modelList[idx++].Key, key); //Range query failed: key mismatch.");
        }

        [Fact]
        public void TestSequentialQuery()
        {
            // set up queries
            var sequential = from k in _databaseInstance.Query<TestModel, int>() orderby k.Key select k.Key;

            var idx = 0;
            foreach (var key in sequential)
                Assert.Equal(_modelList[idx++].Key, key); //Sequential query failed: key mismatch.");
        }

        [Fact]
        public void TestUnrolledQuery()
        {
            _modelList.Sort((m1, m2) => m1.Data.CompareTo(m2.Data));
            var unrolled = from k in _databaseInstance.Query<TestModel, int>()
                orderby k.LazyValue.Value.Data
                select k.LazyValue.Value;

            var idx = 0;

            foreach (var model in unrolled)
            {
                Assert.Equal(_modelList[idx].Key, model.Key); //Unrolled query failed: key mismatch.");
                Assert.Equal(_modelList[idx].Date, model.Date); //Unrolled query failed: date mismatch.");
                Assert.Equal(_modelList[idx].Data, model.Data); //Unrolled query failed: data mismatch.");
                idx++;
            }
        }
    }
}