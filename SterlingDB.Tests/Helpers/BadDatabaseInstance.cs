using System.Collections.Generic;
using SterlingDB.Core;
using SterlingDB.Core.Database;

namespace SterlingDB.Test.Helpers
{
    public class BadDatabaseInstance : BaseDatabaseInstance 
    {
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return null; 
        }
    }
}
