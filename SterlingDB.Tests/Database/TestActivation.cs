using SterlingDB.Exceptions;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    /// <summary>
    ///     Test activation-related database steps
    /// </summary>
    public class TestActivation : TestBase
    {
        public override void Cleanup()
        {
        }

        [Fact]
        public void TestActivationNotReady()
        {
            using var engine = Factory.NewEngine();

            var exception = false;

            try
            {
                engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
            }
            catch (SterlingNotReadyException)
            {
                exception = true;
            }

            Assert.True(exception, "Sterling did not throw a not ready exception on premature access.");

            engine.Activate();

            // this should not fail
            engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(TestContext.TestName, GetDriver());
        }

        /// <summary>
        ///     Test for a duplicate activation using different scenarios
        /// </summary>
        [Fact]
        public void TestDuplicateActivation()
        {
            var engine1 = Factory.NewEngine();
            var engine2 = Factory.NewEngine();

            Assert.NotSame(engine1.SterlingDatabase, engine2.SterlingDatabase);

            engine1.Activate();
            engine2.Activate();

            engine1.Dispose();

            engine2.Activate();
            engine2.Activate();

            engine2.Dispose();
            engine1.Dispose();
        }
    }
}