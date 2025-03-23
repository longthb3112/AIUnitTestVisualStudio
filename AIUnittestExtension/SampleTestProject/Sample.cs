using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleUnitTest
{
    public class Sample
    {
        private ITest _test;
        public Sample(ITest test)
        {
            _test = test;
        }

        public int Sum(int a, int b)
        {
            if (_test == null) throw new ArgumentNullException();

            int c = _test.PlusOne(a);
            return a + b + c;
        }
    }
}
