
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SterlingDB.Core;

namespace SterlingDB.Test
{
    public abstract class TestBase
    {
        protected virtual ISterlingDriver GetDriver()
        {
            return new MemoryDriver();
        }
    }
}
