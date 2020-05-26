using System.Collections.Generic;
using SterlingDB.Database;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class ByteStreamData
    {
        public string Id { get; set; }

        public string Data { get; set; }
    }

    public class TestByteStreamInterceptorDatabase : BaseDatabaseInstance
    {
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
            {
                CreateTableDefinition<ByteStreamData, string>(dataDefinition => dataDefinition.Id)
            };
        }
    }

    public class ByteInterceptor : BaseSterlingByteInterceptor
    {
        public override byte[] Save(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++) retVal[x] = (byte) (sourceStream[x] ^ 0x80); // xor
            return retVal;
        }

        public override byte[] Load(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++) retVal[x] = (byte) (sourceStream[x] ^ 0x80); // xor
            return retVal;
        }
    }

    public class ByteInterceptor2 : BaseSterlingByteInterceptor
    {
        public override byte[] Save(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++) retVal[x] = (byte) (sourceStream[x] ^ 0x22); // xor
            return retVal;
        }

        public override byte[] Load(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++) retVal[x] = (byte) (sourceStream[x] ^ 0x22); // xor
            return retVal;
        }
    }

    public class TestByteStreamInterceptor : TestBase
    {
        public TestByteStreamInterceptor()
        {
            _engine = Factory.NewEngine();
            _engine.Activate();
            _databaseInstance =
                _engine.SterlingDatabase.RegisterDatabase<TestByteStreamInterceptorDatabase>(TestContext.TestName,
                    GetDriver());
            _databaseInstance.PurgeAsync().Wait();
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
        public void TestData()
        {
            const string DATA = "Data to be intercepted";

            var byteStreamData = new ByteStreamData {Id = "data", Data = DATA};

            _databaseInstance.RegisterInterceptor<ByteInterceptor>();
            _databaseInstance.RegisterInterceptor<ByteInterceptor2>();

            _databaseInstance.SaveAsync(byteStreamData).Wait();

            var loadedByteStreamData = _databaseInstance.LoadAsync<ByteStreamData>("data").Result;

            Assert.Equal(DATA, loadedByteStreamData.Data); //, "Byte interceptor test failed: data does not match");

            _databaseInstance.UnRegisterInterceptor<ByteInterceptor2>();

            try
            {
                loadedByteStreamData = _databaseInstance.LoadAsync<ByteStreamData>("data").Result;
            }
            catch
            {
                loadedByteStreamData = null;
            }

            Assert.True(loadedByteStreamData == null || !DATA.Equals(loadedByteStreamData.Data),
                "Byte interceptor test failed: Sterling deserialized intercepted data without interceptor.");

            _databaseInstance.RegisterInterceptor<ByteInterceptor2>();

            loadedByteStreamData = _databaseInstance.LoadAsync<ByteStreamData>("data").Result;

            Assert.Equal(DATA, loadedByteStreamData.Data); //, "Byte interceptor test failed: data does not match");
        }
    }
}