using SterlingDB.Test.Helpers;
using Xunit;
using System;
using SterlingDB.Exceptions;
using SterlingDB.Serialization;
using SterlingDB.Server;
using System.Text;
using System.IO;

namespace SterlingDB.Test.Serializer
{
    public class TestAggregateSerializer
    {
        /// <summary>
        ///     The target default serializer
        /// </summary>
        private readonly ISterlingSerializer _target;

        // test data
        const int FIVE = 5;
        const double PI = 3.14;
        const string TEST_STRING = "This string";
        private DateTime _date = DateTime.Now;

        private TestStruct _testStruct = new TestStruct { Value = 5, Date = DateTime.Now };


        public TestAggregateSerializer()
        {
            var serializer = new AggregateSerializer(new PlatformAdapter());
            serializer.AddSerializer(new DefaultSerializer());
            serializer.AddSerializer(new TestSerializer());
            _target = serializer;
        }

        /// <summary>
        ///     Check that serialization checks are working
        /// </summary>
        [Fact]
        public void TestSerializationChecks()
        {
            Assert.True(_target.CanSerialize<int>()); //Failed to recognize integer.");
            Assert.True(_target.CanSerialize<double>()); //Failed to recognize double.");
            Assert.True(_target.CanSerialize<string>()); //Failed to recognize string (generic).");
            Assert.True(_target.CanSerialize(typeof(string))); //Failed to recognize string.");
            Assert.True(_target.CanSerialize(typeof(TestStruct))); //Failed to recognize test structure.");                       
            Assert.False(_target.CanSerialize(_date.GetType())); //Accepted data time.");
        }

        /// <summary>
        ///     Check it throws an exception when trying to serialize the wrong thing
        /// </summary>
        [Fact]
        public void TestSerializerException()
        {
            var exception = false;

            using var mem = new MemoryStream();
            using var bw = new BinaryWriter(mem);

            try
            {
                _target.Serialize(_date, bw);
            }
            catch (SterlingSerializerException)
            {
                exception = true;
            }

            Assert.True(exception); //Sterling did not throw an exception when attemping to serialize the date.");
        }

        /// <summary>
        ///     Test the serialization and deserialization
        /// </summary>
        [Fact]
        public void TestSerialization()
        {
            var charArray = TEST_STRING.ToCharArray();
            var byteArray = Encoding.UTF8.GetBytes(TEST_STRING);

            int targetFive;
            double targetPi;
            string targetTestString;
            char[] targetCharArray;
            byte[] targetByteArray;
            TestStruct targetTestStruct;

            using (var mem = new MemoryStream())
            using (var bw = new BinaryWriter(mem))
            {
                _target.Serialize(FIVE, bw);
                _target.Serialize(PI, bw);
                _target.Serialize(TEST_STRING, bw);
                _target.Serialize(charArray, bw);
                _target.Serialize(byteArray, bw);
                _target.Serialize(_testStruct, bw);

                mem.Seek(0, SeekOrigin.Begin);

                using (var br = new BinaryReader(mem))
                {
                    targetFive = _target.Deserialize<int>(br);
                    targetPi = _target.Deserialize<double>(br);
                    targetTestString = _target.Deserialize<string>(br);
                    targetCharArray = _target.Deserialize<char[]>(br);
                    targetByteArray = (byte[])_target.Deserialize(typeof(byte[]), br);
                    targetTestStruct = _target.Deserialize<TestStruct>(br);
                }
            }

            Assert.Equal(FIVE, targetFive); //Integer did not deserialize correctly.");
            Assert.Equal(PI, targetPi); //Double did not deserialize correctly.");
            Assert.Equal(TEST_STRING, targetTestString); //String did not deserialize correctly.");

            Assert.Equal(charArray.Length, targetCharArray.Length); //Character array length mismatch.");
            if (charArray.Length == targetCharArray.Length)
            {
                for (var idx = 0; idx < charArray.Length; idx++)
                {
                    Assert.Equal(charArray[idx], targetCharArray[idx]);//"Character array did not deserialize correctly.");
                }
            }

            Assert.Equal(byteArray.Length, targetByteArray.Length); //Byte array length mismatch.");
            if (byteArray.Length == targetByteArray.Length)
            {
                for (var idx = 0; idx < byteArray.Length; idx++)
                {
                    Assert.Equal(byteArray[idx], targetByteArray[idx]); //Byte array did not deserialize correctly.");
                }
            }

            Assert.Equal(_testStruct.Value, targetTestStruct.Value); //Test structure did not deserialize.");
            Assert.Equal(_testStruct.Date, targetTestStruct.Date); //Test structure did not deserialize correctly.");
        }
    }
}
