using System;
using System.IO;
using SterlingDB.Serialization;
using SterlingDB.Server;
using Xunit;

namespace SterlingDB.Test.Serializer
{
    /// <summary>
    ///     Default serializer test
    /// </summary>
    public class TestExtendedSerializer : TestBase
    {
        public TestExtendedSerializer()
        {
            _target = new ExtendedSerializer(new PlatformAdapter());
        }

        /// <summary>
        ///     The target default serializer
        /// </summary>
        private readonly ISterlingSerializer _target;

        // test data
        private const decimal DECIMAL = (decimal) 5.11;
        private readonly DateTime _date = DateTime.MinValue;
        private readonly DateTime _secondDate = DateTime.Now;
        private readonly Uri _uri = new Uri("http://sterling.codeplex.com", UriKind.Absolute);
        private readonly Guid _guid = Guid.NewGuid();
        private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(2);

        public override void Cleanup()
        {
        }

        /// <summary>
        ///     Test the serialization and deserialization
        /// </summary>
        [Fact]
        public void TestSerialization()
        {
            decimal decimalTest;
            DateTime dateTest, date2Test;
            Uri uriTest;
            Guid guidTest;
            TimeSpan timeSpanTest;

            using (var mem = new MemoryStream())
            using (var bw = new BinaryWriter(mem))
            {
                _target.Serialize(DECIMAL, bw);
                _target.Serialize(_date, bw);
                _target.Serialize(_secondDate, bw);
                _target.Serialize(_uri, bw);
                _target.Serialize(_guid, bw);
                _target.Serialize(_timeSpan, bw);

                mem.Seek(0, SeekOrigin.Begin);

                using (var br = new BinaryReader(mem))
                {
                    decimalTest = _target.Deserialize<decimal>(br);
                    dateTest = _target.Deserialize<DateTime>(br);
                    date2Test = _target.Deserialize<DateTime>(br);
                    uriTest = _target.Deserialize<Uri>(br);
                    guidTest = _target.Deserialize<Guid>(br);
                    timeSpanTest = (TimeSpan) _target.Deserialize(typeof(TimeSpan), br);
                }
            }

            Assert.Equal(DECIMAL, decimalTest); //Decimal did not deserialize correctly.");
            Assert.Equal(_date, dateTest); //DateTime did not deserialize correctly.");
            Assert.Equal(_secondDate, date2Test); //Second DateTime did not deserialize correctly.");
            Assert.Equal(_uri, uriTest); //Uri did not deserialize correctly.");            
            Assert.Equal(_guid, guidTest); //Guid did not de-serialized correctly.");
            Assert.Equal(_timeSpan, timeSpanTest); //Time span did not deserialize correctly.");
        }

        /// <summary>
        ///     Check that serialization checks are working
        /// </summary>
        [Fact]
        public void TestSerializationChecks()
        {
            Assert.True(_target.CanSerialize<decimal>()); //Failed to recognize decimal.");          
            Assert.True(_target.CanSerialize<DateTime>()); //Failed to recognize date time.");
            Assert.True(_target.CanSerialize<Uri>()); //Failed to recognize uri.");
            Assert.True(_target.CanSerialize<Guid>()); //Failed to recognize guid.");
            Assert.True(_target.CanSerialize<TimeSpan>()); //Failed to recognize timespan.");
        }
    }
}