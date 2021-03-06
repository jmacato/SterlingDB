﻿using System.IO;
using System.Text;
using SterlingDB.Serialization;
using Xunit;

namespace SterlingDB.Test.Serializer
{
    /// <summary>
    ///     Default serializer test
    /// </summary>
    public class TestDefaultSerializer : TestBase
    {
        public TestDefaultSerializer()
        {
            _target = new DefaultSerializer();
        }

        /// <summary>
        ///     The target default serializer
        /// </summary>
        private readonly ISterlingSerializer _target;

        // test data
        private const int FIVE = 5;
        private const double PI = 3.14;
        private const string TEST_STRING = "This string";

        public override void Cleanup()
        {
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

            using (var mem = new MemoryStream())
            using (var bw = new BinaryWriter(mem))
            {
                _target.Serialize(FIVE, bw);
                _target.Serialize(PI, bw);
                _target.Serialize(TEST_STRING, bw);
                _target.Serialize(charArray, bw);
                _target.Serialize(byteArray, bw);

                mem.Seek(0, SeekOrigin.Begin);

                using (var br = new BinaryReader(mem))
                {
                    targetFive = _target.Deserialize<int>(br);
                    targetPi = _target.Deserialize<double>(br);
                    targetTestString = _target.Deserialize<string>(br);
                    targetCharArray = _target.Deserialize<char[]>(br);
                    targetByteArray = (byte[]) _target.Deserialize(typeof(byte[]), br);
                }
            }

            Assert.Equal(FIVE, targetFive); //Integer did not deserialize correctly.");
            Assert.Equal(PI, targetPi); //Double did not deserialize correctly.");
            Assert.Equal(TEST_STRING, targetTestString); //String did not deserialize correctly.");

            Assert.Equal(charArray.Length, targetCharArray.Length); //Character array length mismatch.");
            if (charArray.Length == targetCharArray.Length)
                for (var idx = 0; idx < charArray.Length; idx++)
                    Assert.Equal(charArray[idx],
                        targetCharArray[idx]); //Character array did not deserialize correctly.");

            Assert.Equal(byteArray.Length, targetByteArray.Length); //Byte array length mismatch.");
            if (byteArray.Length == targetByteArray.Length)
                for (var idx = 0; idx < byteArray.Length; idx++)
                    Assert.Equal(byteArray[idx], targetByteArray[idx]); //Byte array did not deserialize correctly.");
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
        }
    }
}