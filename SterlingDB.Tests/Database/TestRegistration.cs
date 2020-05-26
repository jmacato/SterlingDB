using SterlingDB.Core;
using SterlingDB.Core.Database;
using SterlingDB.Server.FileSystem;
using SterlingDB.Test.Helpers;
using Xunit;
using SterlingDB.Core;
using SterlingDB.Core.Database;

namespace SterlingDB.Test.Database
{
#if SILVERLIGHT
    [Tag("DatabaseRegistration")]
#endif
    
    public class TestRegistration
    {        
        [Fact]
        public void TestDatabaseRegistration()
        {
            using (var engine = Factory.NewEngine())
            {
                var db = engine.SterlingDatabase;

                // test not activated yet 
                var raiseError = false;

                try
                {
                    db.RegisterDatabase<TestDatabaseInstance>( "register" );
                }
                catch(SterlingNotReadyException)
                {
                    raiseError = true;
                }

                Assert.True(raiseError, "Sterling did not throw activation error.");

                engine.Activate();

                var testDb2 = db.RegisterDatabase<TestDatabaseInstance>( "register" );

                Assert.NotNull(testDb2, "Database registration returned null.");
                Assert.InstanceOfType(testDb2, typeof(TestDatabaseInstance), "Incorrect database type returned.");
            
                Assert.Equal("register", testDb2.Name, "Incorrect database name.");

                // test bad database (no table definitions) 
                raiseError = false;

                try
                {
                    db.RegisterDatabase<DupDatabaseInstance>( "register" );
                }
                catch (SterlingDuplicateTypeException)
                {
                    raiseError = true;
                }

                Assert.True(raiseError, "Sterling did not catch the duplicate type registration.");
            }
        }
    }
}
