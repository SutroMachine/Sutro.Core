using System;
using System.Collections.Generic;
using System.Text;

namespace Sutro.Core.FunctionalTests
{
    public abstract class TestBase
    {
        protected TestBase()
        {
#if DEBUG
            Models.Config.Parallel = false;
            Models.Config.Debug = true;
#else
            Models.Config.Parallel = true;
            Models.Config.Debug = false;
#endif

        }
    }
}
