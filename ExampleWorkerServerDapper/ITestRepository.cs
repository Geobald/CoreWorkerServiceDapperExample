using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleWorkerServerDapper
{
    public interface ITestRepository
    {
        bool Truncate();
        bool Populate();
    }
}
