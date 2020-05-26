
using SterlingDB.Server.FileSystem;
using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Core.Exceptions;
using SterlingDB.Test.Helpers;
using Xunit;

namespace SterlingDB.Test.Database
{
    public class TestActivationAltDriver : TestActivation
    {
        protected override ISterlingDriver GetDriver()
        {
            return new FileSystemDriver();
        }
    }

    public static class TestContext
    {
        public static string TestName = "testDb";
    }

    /// <summary>
    ///     Test activation-related database steps
    /// </summary> 
    public class TestActivation : TestBase
    {
 
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

        //[Fact]
        //public void TestDuplicateClass()
        //{
        //    using (var engine = Factory.NewEngine())
        //    {
        //        engine.Activate();

        //        // now cheat and try to make a new sterling database directly
        //        var database = new SterlingDatabase(SterlingFactory.GetLogger());

        //        var exception = false; 
        //        try
        //        {
        //            database.Activate();
        //        }
        //        catch (SterlingActivationException)
        //        {
        //            exception = true;
        //        }

        //        Assert.True(exception, "Sterling did not throw an activation exception on duplicate activation with new database class.");
        //    }
        //}

        [Fact]
        public void TestActivationNotReady()
        {
            using (var engine = Factory.NewEngine())
            {
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
        }
    }
}
