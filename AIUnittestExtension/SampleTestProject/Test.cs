using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleUnitTest
{
    public class Test : ITest
    {
        public int PlusOne(int a)
        {
            return a + 1;
        }
    }
}
